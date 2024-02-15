using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;

public interface IClientService
{
  Task<Cliente?> GetCliente(NpgsqlConnection conn, int id);
  Task<IEnumerable<Transacao>> GetTransacoes(NpgsqlConnection conn, int id);
  Task<(int, int)?> CriarTransacaoEAtualizarSaldo(NpgsqlConnection conn, int id, CriarTransacaoRequest request);
}

public class ClientService : IClientService
{
  private readonly IObjectPoolService _pool;
  private readonly ICommandPoolService _commandPool;

  public ClientService(IObjectPoolService pool, ICommandPoolService commandPool)
  {
    _pool = pool;
    _commandPool = commandPool;
  }

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

    var cmd = _commandPool.RentCommand(conn, sql);

    try
    {
      cmd.Parameters.AddWithValue("id", id);

      await cmd.PrepareAsync();

      using var reader = await cmd.ExecuteReaderAsync();

      if (await reader.ReadAsync())
      {
        var cliente = _pool.Rent<Cliente>();

        cliente.Id = id;
        cliente.Saldo = reader.GetInt32(0);
        cliente.Limite = reader.GetInt32(1);


        return cliente;
      }

      return null;
    }
    finally
    {
      _commandPool.ReturnCommand(cmd);
    }
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

    var cmd = _commandPool.RentCommand(conn, sql);

    try
    {
      cmd.Parameters.AddWithValue("id", id);

      await cmd.PrepareAsync();

      using var reader = await cmd.ExecuteReaderAsync();

      while (await reader.ReadAsync())
      {
        var transacao = _pool.Rent<Transacao>();

        transacao.Valor = reader.GetInt32(0);
        transacao.Tipo = reader.GetChar(1);
        transacao.Descricao = reader.GetString(2);
        transacao.RealizadaEm = reader.GetDateTime(3);

        transacoes.Add(transacao);
      }

      return transacoes;
    }
    finally
    {
      _commandPool.ReturnCommand(cmd);
    }
  }

  public async Task<(int, int)?> CriarTransacaoEAtualizarSaldo(NpgsqlConnection conn, int id, CriarTransacaoRequest request)
  {
    var sql = "CALL criar_transacao_e_atualizar_saldo(@id_cliente, @valor, @tipo, @descricao, @realizada_em);";

    var cmd = _commandPool.RentCommand(conn, sql);

    try
    {
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
        var saldo = reader.GetInt32(0);
        var limite = reader.GetInt32(1);
        return (saldo, limite);
      }
      catch (InvalidCastException)
      {
        return null;
      }
    }
    finally
    {
      _commandPool.ReturnCommand(cmd);
    }
  }
}