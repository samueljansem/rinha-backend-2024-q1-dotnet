using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;

public interface IClientService
{
  Task<Cliente?> GetCliente(NpgsqlConnection conn, int id);
  Task<IEnumerable<Transacao>> GetTransacoes(NpgsqlConnection conn, int id);
  Task<CriarTransacaoResponse?> CriarTransacaoEAtualizarSaldo(NpgsqlConnection conn, int id, CriarTransacaoRequest request);
}

public class ClientService : IClientService
{

  public async Task<Cliente?> GetCliente(NpgsqlConnection conn, int id)
  {
    const string sql = @"
      SELECT
          saldo,
          limite
      FROM
          clientes
      WHERE
          id = @id;
    ";

    using var cmd = new NpgsqlCommand(sql, conn);

    cmd.Parameters.AddWithValue("id", id);

    await cmd.PrepareAsync();

    using var reader = await cmd.ExecuteReaderAsync();

    if (await reader.ReadAsync())
    {

      var cliente = new Cliente
      {
        Id = id,
        Saldo = reader.GetInt32(0),
        Limite = reader.GetInt32(1)
      };

      return cliente;
    }

    return null;
  }

  public async Task<IEnumerable<Transacao>> GetTransacoes(NpgsqlConnection conn, int id)
  {
    const string sql = @"
      SELECT
          valor,
          tipo,
          descricao,
          realizada_em
      FROM
          transacoes
      WHERE
          id_cliente = @id
      ORDER BY
          realizada_em DESC
      LIMIT 10;
    ";

    var transacoes = new List<Transacao>(10);


    using var cmd = new NpgsqlCommand(sql, conn);

    cmd.Parameters.AddWithValue("id", id);

    await cmd.PrepareAsync();

    using var reader = await cmd.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
      var transacao = new Transacao
      {
        Valor = reader.GetInt32(0),
        Tipo = reader.GetChar(1),
        Descricao = reader.GetString(2),
        RealizadaEm = reader.GetDateTime(3)
      };

      transacoes.Add(transacao);
    }

    return transacoes;
  }

  public async Task<CriarTransacaoResponse?> CriarTransacaoEAtualizarSaldo(NpgsqlConnection conn, int id, CriarTransacaoRequest request)
  {
    var sql = "CALL criar_transacao_e_atualizar_saldo(@id_cliente, @valor, @tipo, @descricao, @realizada_em);";

    using var cmd = new NpgsqlCommand(sql, conn);

    cmd.Parameters.AddWithValue("id_cliente", id);
    cmd.Parameters.AddWithValue("valor", request.Valor);
    cmd.Parameters.AddWithValue("tipo", request.Tipo);
    cmd.Parameters.AddWithValue("descricao", request.Descricao);
    cmd.Parameters.AddWithValue("realizada_em", DateTime.UtcNow);

    await cmd.PrepareAsync();

    using var reader = await cmd.ExecuteReaderAsync();

    if (!await reader.ReadAsync()) return null;

    try
    {
      var response = new CriarTransacaoResponse
      {
        Saldo = reader.GetInt32(0),
        Limite = reader.GetInt32(1)
      };

      return response;
    }
    catch (InvalidCastException)
    {
      return null;
    }
  }
}