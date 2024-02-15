using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddSingleton(provider =>
    new NpgsqlConnectionFactory(provider.GetRequiredService<IConfiguration>())
);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
builder.Services.AddSingleton<IObjectPoolService, ObjectPoolService>();
builder.Services.AddSingleton<ICommandPoolService, CommandPoolService>();
builder.Services.AddSingleton<IClientService, ClientService>();
builder.Services.AddScoped<IPooledObjectTracker, PooledObjectTracker>();

var app = builder.Build();

var objectPoolService = app.Services.GetRequiredService<IObjectPoolService>();
objectPoolService.RegisterPolicy(new ClientePoolPolicy(), 1000);
// objectPoolService.RegisterPolicy(new CriarTransacaoRequestPoolPolicy(), 400);
objectPoolService.RegisterPolicy(new CriarTransacaoResponsePoolPolicy(), 23000);
objectPoolService.RegisterPolicy(new ExtratoPoolPolicy(), 1000);
objectPoolService.RegisterPolicy(new SaldoPoolPolicy(), 1000);
objectPoolService.RegisterPolicy(new TransacaoPoolPolicy(), 10000);

app.UseMiddleware<ObjectPoolMiddleware>();

app.MapGet("clientes/{id}/extrato", async (NpgsqlConnectionFactory connectionFactory, IObjectPoolService pool, IPooledObjectTracker tracker, IClientService service, int id) =>
{
    if (id > 5)
    {
        return Results.NotFound();
    }

    var clienteTask = Task.Run(async () =>
    {
        using var conn = connectionFactory.Create();
        await conn.OpenAsync();
        return await service.GetCliente(conn, id);
    });

    var transacoesTask = Task.Run(async () =>
    {
        using var conn = connectionFactory.Create();
        await conn.OpenAsync();
        return await service.GetTransacoes(conn, id);
    });

    await Task.WhenAll(clienteTask, transacoesTask);

    var cliente = await clienteTask;
    var ultimasTransacoes = await transacoesTask;

    if (cliente == null)
    {
        return Results.NotFound();
    }

    var extrato = pool.Rent<Extrato>();
    var saldo = pool.Rent<Saldo>();

    tracker.TrackObject(cliente);
    tracker.TrackObject(saldo);
    tracker.TrackObject(extrato);
    foreach (var t in ultimasTransacoes) tracker.TrackObject(t);

    saldo.Total = cliente.Saldo;
    saldo.Limite = cliente.Limite;
    saldo.DataExtrato = DateTime.UtcNow;

    extrato.Saldo = saldo;
    extrato.UltimasTransacoes = ultimasTransacoes;

    return Results.Ok(extrato);
});

app.MapPost("clientes/{id}/transacoes", async (NpgsqlConnectionFactory connectionFactory, HttpRequest r, IObjectPoolService pool, IPooledObjectTracker tracker, IClientService service, int id) =>
{
    if (id > 5)
    {
        return Results.NotFound();
    }

    CriarTransacaoRequest? request = null;

    try
    {
        request = await r.ReadFromJsonAsync<CriarTransacaoRequest>();
    }
    catch
    {
        return Results.UnprocessableEntity("Requisição inválida.");
    }

    if (request == null)
    {
        return Results.UnprocessableEntity("Requisição inválida.");
    }

    if (request.Valor <= 0)
    {
        return Results.UnprocessableEntity("Valor inválido.");
    }

    if (request.Tipo != 'd' && request.Tipo != 'c')
    {
        return Results.UnprocessableEntity("Tipo inválido.");
    }

    if (string.IsNullOrEmpty(request.Descricao) || request.Descricao.Length > 10)
    {
        return Results.UnprocessableEntity("Descrição inválida.");
    }

    using var conn = connectionFactory.Create();

    await conn.OpenAsync();

    var tuple = await service.CriarTransacaoEAtualizarSaldo(conn, id, request);

    if (tuple == null)
    {
        return Results.UnprocessableEntity("Saldo insuficiente.");
    }

    var response = pool.Rent<CriarTransacaoResponse>();
    tracker.TrackObject(response);

    response.Saldo = tuple.Value.Item1;
    response.Limite = tuple.Value.Item2;

    return Results.Ok(response);
});

app.Run();
