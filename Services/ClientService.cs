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

  public static async Task<int> AtualizarSaldo(NpgsqlConnection conn, int id, int valor)
  {
    const string sqlAtualizarSaldo = @"
      UPDATE clientes
      SET saldo = @valor
      WHERE id = @id;
    ";

    using (var cmdAtualizarSaldo = new NpgsqlCommand(sqlAtualizarSaldo, conn))
    {
      cmdAtualizarSaldo.Parameters.AddWithValue("id", id);
      cmdAtualizarSaldo.Parameters.AddWithValue("valor", valor);

      return await cmdAtualizarSaldo.ExecuteNonQueryAsync();
    }
  }

  public static async Task<int> CriarTransacao(NpgsqlConnection conn, int id, CriarTransacaoRequest request)
  {
    const string sqlInserirTransacao = @"
      INSERT INTO transacoes (id_cliente, valor, tipo, descricao, realizada_em)
      VALUES (@id, @valor, @tipo, @descricao, @realizada_em);
    ";

    using (var cmdInserirTransacao = new NpgsqlCommand(sqlInserirTransacao, conn))
    {
      cmdInserirTransacao.Parameters.AddWithValue("id", id);
      cmdInserirTransacao.Parameters.AddWithValue("valor", request.Valor);
      cmdInserirTransacao.Parameters.AddWithValue("tipo", request.Tipo);
      cmdInserirTransacao.Parameters.AddWithValue("descricao", request.Descricao);
      cmdInserirTransacao.Parameters.AddWithValue("realizada_em", DateTime.UtcNow);

      return await cmdInserirTransacao.ExecuteNonQueryAsync();
    }
  }
}