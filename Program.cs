using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddSingleton<INpgsqlConnectionFactory, NpgsqlConnectionFactory>(provider =>
    new NpgsqlConnectionFactory(provider.GetRequiredService<IConfiguration>())
);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddSingleton<IClientService, ClientService>();

var app = builder.Build();

app.MapGet("clientes/{id}/extrato", async (INpgsqlConnectionFactory connectionFactory, IClientService service, int id) =>
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

    if (cliente == null || ultimasTransacoes == null)
    {
        return Results.NotFound();
    }

    var saldo = new Saldo
    {
        Total = cliente.Value.Saldo,
        Limite = cliente.Value.Limite,
        DataExtrato = DateTime.UtcNow
    };

    var extrato = new Extrato
    {
        Saldo = saldo,
        UltimasTransacoes = ultimasTransacoes
    };

    return Results.Ok(extrato);
});

app.MapPost("clientes/{id}/transacoes", async (INpgsqlConnectionFactory connectionFactory, HttpRequest r, IClientService service, int id) =>
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

    if (request.Value.Valor <= 0)
    {
        return Results.UnprocessableEntity("Valor inválido.");
    }

    if (request.Value.Tipo != 'd' && request.Value.Tipo != 'c')
    {
        return Results.UnprocessableEntity("Tipo inválido.");
    }

    if (string.IsNullOrEmpty(request.Value.Descricao) || request.Value.Descricao.Length > 10)
    {
        return Results.UnprocessableEntity("Descrição inválida.");
    }

    using var conn = connectionFactory.Create();

    await conn.OpenAsync();

    var response = await service.CriarTransacaoEAtualizarSaldo(conn, id, request.Value);

    if (response == null)
    {
        return Results.UnprocessableEntity("Saldo insuficiente.");
    }

    return Results.Ok(response.Value);
});

app.Run();
