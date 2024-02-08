using Npgsql;

public static class ClientService
{

  public static async Task<(int?, int?)> GetCliente(NpgsqlConnection conn, int id)
  {
    const string sqlSaldo = @"
      SELECT
          c.saldo as cliente_saldo,
          c.limite as cliente_limite
      FROM
          clientes c
      WHERE
          c.id = @id;
    ";

    int saldo;
    int limite;

    using (var cmdSaldo = new NpgsqlCommand(sqlSaldo, conn))
    {
      cmdSaldo.Parameters.AddWithValue("id", id);

      using (var reader = await cmdSaldo.ExecuteReaderAsync())
      {
        if (!await reader.ReadAsync()) return (null, null);

        saldo = reader.GetInt32(0);
        limite = reader.GetInt32(1);
      }
    }

    return (saldo, limite);
  }

  public static async Task<IEnumerable<TransacaoDTO>> GetTransacoes(NpgsqlConnection conn, int id)
  {
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

    var transacoes = new List<TransacaoDTO>();

    using (var cmdTransacoes = new NpgsqlCommand(sqlTransacoes, conn))
    {
      cmdTransacoes.Parameters.AddWithValue("id", id);

      using (var readerTransacoes = await cmdTransacoes.ExecuteReaderAsync())
      {
        while (await readerTransacoes.ReadAsync())
        {
          transacoes.Add(new TransacaoDTO
          {
            Valor = readerTransacoes.GetInt32(0),
            Tipo = readerTransacoes.GetChar(1),
            Descricao = readerTransacoes.GetString(2),
            RealizadaEm = readerTransacoes.GetDateTime(3)
          });
        }
      }
    }

    return transacoes;
  }

  public static async Task<(int?, int?)> CriarTransacaoEAtualizarSaldo(NpgsqlConnection conn, int id, CriarTransacaoRequest request)
  {
    using var cmd = new NpgsqlCommand("CALL criar_transacao_e_atualizar_saldo(@id_cliente, @valor, @tipo, @descricao, @realizada_em)", conn);

    cmd.Parameters.AddWithValue("id_cliente", id);
    cmd.Parameters.AddWithValue("valor", request.Valor);
    cmd.Parameters.AddWithValue("tipo", request.Tipo);
    cmd.Parameters.AddWithValue("descricao", request.Descricao);
    cmd.Parameters.AddWithValue("realizada_em", DateTime.UtcNow);

    using var reader = await cmd.ExecuteReaderAsync();

    try
    {
      if (await reader.ReadAsync())
      {
        var novoSaldo = reader.GetInt32(0);
        var limite = reader.GetInt32(1);
        return (novoSaldo, limite);
      }

      return (null, null);
    }
    catch (InvalidCastException)
    {
      return (null, null);
    }
  }
}