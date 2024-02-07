using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Npgsql;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

app.MapGet("clientes/{id}/extrato", async (IConfiguration configuration, int id) =>
{
    var connectionString = configuration.GetConnectionString("DefaultConnection");

    using var conn = new NpgsqlConnection(connectionString);

    await conn.OpenAsync();

    const string sqlSaldo = @"
        SELECT
            c.saldo as cliente_saldo,
            c.limite as cliente_limite
        FROM
            clientes c
        WHERE
            c.id = @id;
    ";

    var saldo = new SaldoDTO();
    var ultimasTransacoes = new List<TransacaoDTO>();

    using (var cmdSaldo = new NpgsqlCommand(sqlSaldo, conn))
    {
        cmdSaldo.Parameters.AddWithValue("id", id);

        using (var reader = await cmdSaldo.ExecuteReaderAsync())
        {
            if (!await reader.ReadAsync())
            {
                return Results.NotFound();
            }

            saldo.Total = reader.GetInt32(0);
            saldo.Limite = reader.GetInt32(1);
            saldo.DataExtrato = DateTime.UtcNow;
        }
    }

    const string sqlTransacoes = @"
        SELECT
            t.valor,
            t.tipo,
            t.descricao,
            t.realizada_em
        FROM
            transacoes t
        WHERE
            t.id_cliente = @id
        ORDER BY
            t.realizada_em DESC
        LIMIT 10;
    ";

    using (var cmdTransacoes = new NpgsqlCommand(sqlTransacoes, conn))
    {
        cmdTransacoes.Parameters.AddWithValue("id", id);

        using (var readerTransacoes = await cmdTransacoes.ExecuteReaderAsync())
        {
            while (await readerTransacoes.ReadAsync())
            {
                ultimasTransacoes.Add(new TransacaoDTO
                {
                    Valor = readerTransacoes.GetInt32(0),
                    Tipo = readerTransacoes.GetChar(1),
                    Descricao = readerTransacoes.GetString(2),
                    RealizadaEm = readerTransacoes.GetDateTime(3)
                });
            }
        }
    }

    var extrato = new ExtratoDTO
    {
        Saldo = saldo,
        UltimasTransacoes = ultimasTransacoes
    };

    return Results.Ok(extrato);
});


app.MapPost("clientes/{id}/transacoes", async (IConfiguration configuration, HttpContext httpContext, int id, CriarTransacaoRequest request) => {
    var connectionString = configuration.GetConnectionString("DefaultConnection");

    if (request.Valor <= 0) {
        return Results.UnprocessableEntity("Valor inválido.");
    }

    if (request.Tipo != 'd' && request.Tipo != 'c') {
        return Results.UnprocessableEntity("Tipo inválido.");
    }

    if (string.IsNullOrEmpty(request.Descricao) || request.Descricao.Length > 10) {
        return Results.UnprocessableEntity("Descrição inválida.");
    }

    using var conn = new NpgsqlConnection(connectionString);
    await conn.OpenAsync();

    using var transaction = await conn.BeginTransactionAsync();

    int saldo = 0;
    int limite = 0;

    if (request.Tipo == 'd') {
        var checkBalanceSql = "SELECT saldo, limite FROM clientes WHERE id = @id";
        using var checkBalanceCmd = new NpgsqlCommand(checkBalanceSql, conn);
        checkBalanceCmd.Parameters.AddWithValue("id", id);
        using var reader = await checkBalanceCmd.ExecuteReaderAsync();
        if (await reader.ReadAsync()) {
            saldo = reader.GetInt32(0);
            limite = reader.GetInt32(1);
            reader.Close();

            if (saldo - request.Valor < -limite) {
                await transaction.RollbackAsync();
                return Results.UnprocessableEntity("Saldo insuficiente.");
            }
        } else {
            await transaction.RollbackAsync();
            return Results.NotFound();
        }
    }

    const string sql = @"
        INSERT INTO transacoes (id_cliente, valor, tipo, descricao, realizada_em)
        VALUES (@id_cliente, @valor, @tipo, @descricao, @realizada_em);
    ";
    using var cmd = new NpgsqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("id_cliente", id);
    cmd.Parameters.AddWithValue("valor", request.Valor);
    cmd.Parameters.AddWithValue("tipo", request.Tipo);
    cmd.Parameters.AddWithValue("descricao", request.Descricao);
    cmd.Parameters.AddWithValue("realizada_em", DateTime.UtcNow);
    await cmd.ExecuteNonQueryAsync();

    const string sqlUpdateSaldo = @"
        UPDATE clientes
        SET saldo = saldo + @valor
        WHERE id = @id;
    ";
    using var cmdUpdateSaldo = new NpgsqlCommand(sqlUpdateSaldo, conn);
    cmdUpdateSaldo.Parameters.AddWithValue("id", id);
    cmdUpdateSaldo.Parameters.AddWithValue("valor", request.Tipo == 'd' ? -request.Valor : request.Valor);
    await cmdUpdateSaldo.ExecuteNonQueryAsync();

    using (var cmdSaldo = new NpgsqlCommand("SELECT saldo, limite FROM clientes WHERE id = @id", conn))
    {
        cmdSaldo.Parameters.AddWithValue("id", id);
        using var readerSaldo = await cmdSaldo.ExecuteReaderAsync();
        if (await readerSaldo.ReadAsync())
        {
            saldo = readerSaldo.GetInt32(0);
            limite = readerSaldo.GetInt32(1);
        }
    }

    await transaction.CommitAsync();

    var response = new CriarTransacaoResponse
    {
        Saldo = saldo,
        Limite = limite
    };

    return Results.Ok(response);
});

app.Run();

[JsonSerializable(typeof(CriarTransacaoRequest))]
[JsonSerializable(typeof(CriarTransacaoResponse))]
[JsonSerializable(typeof(ExtratoDTO))]
[JsonSerializable(typeof(TransacaoDTO))]
[JsonSerializable(typeof(SaldoDTO))]
internal partial class AppJsonSerializerContext : JsonSerializerContext {}