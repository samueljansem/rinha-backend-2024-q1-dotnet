using Microsoft.Extensions.ObjectPool;

public class Cliente : IResettable
{
  public int Id { get; set; }
  public int Limite { get; set; }
  public int Saldo { get; set; }

  public bool TryReset()
  {
    Id = 0;
    Limite = 0;
    Saldo = 0;
    return true;
  }
}

public class ClientePoolPolicy : IPooledObjectPolicy<Cliente>
{
  public Cliente Create() => new Cliente();
  public bool Return(Cliente obj)
  {
    obj.TryReset();
    return true;
  }
}