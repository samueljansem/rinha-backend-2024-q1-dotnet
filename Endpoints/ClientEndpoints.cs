
using System.Data;

public static class ClientEndpoints
{
    public static void MapClientEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("clientes/{id}/extrato", async (SqlConnectionFactory connectionFactory, int id) =>
        {
            using var conn = connectionFactory.Create();

            await conn.OpenAsync();

            var (saldo, limite) = await ClientService.GetCliente(conn, id);

            if (saldo == null || limite == null)
            {
                return Results.NotFound();
            }

            var ultimasTransacoes = await ClientService.GetTransacoes(conn, id);

            var saldoDto = new SaldoDTO
            {
                Total = (int)saldo,
                Limite = (int)limite,
                DataExtrato = DateTime.UtcNow
            };

            var extrato = new ExtratoDTO
            {
                Saldo = saldoDto,
                UltimasTransacoes = ultimasTransacoes
            };

            return Results.Ok(extrato);
        });


        app.MapPost("clientes/{id}/transacoes", async (SqlConnectionFactory connectionFactory, int id, HttpRequest r) =>
        {
            CriarTransacaoRequest? request;

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

            var (saldo, limite) = await ClientService.GetCliente(conn, id);

            if (saldo == null || limite == null)
            {
                return Results.NotFound();
            }

            if (request.Tipo == 'd')
            {
                if (saldo - request.Valor < -limite)
                {
                    return Results.UnprocessableEntity("Saldo insuficiente.");
                }
            }

            saldo += request.Tipo == 'd' ? -request.Valor : request.Valor;

            await ClientService.AtualizarSaldo(conn, id, (int)saldo);
            await ClientService.CriarTransacao(conn, id, request);

            var response = new CriarTransacaoResponse
            {
                Saldo = (int)saldo,
                Limite = (int)limite
            };

            return Results.Ok(response);
        });
    }
}