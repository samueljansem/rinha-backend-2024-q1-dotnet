using System.Collections.Concurrent;
using Npgsql;

public interface ICommandPoolService
{
  NpgsqlCommand RentCommand(NpgsqlConnection conn, string sql);
  void ReturnCommand(NpgsqlCommand cmd);
}

public class CommandPoolService : ICommandPoolService
{
  private ConcurrentBag<NpgsqlCommand> _commands = new ConcurrentBag<NpgsqlCommand>();

  public NpgsqlCommand RentCommand(NpgsqlConnection conn, string sql)
  {
    if (_commands.TryTake(out var cmd))
    {
      cmd.Connection = conn;
      cmd.CommandText = sql;
      cmd.Parameters.Clear();
      return cmd;
    }

    return new NpgsqlCommand(sql, conn);
  }

  public void ReturnCommand(NpgsqlCommand cmd)
  {
    _commands.Add(cmd);
  }
}