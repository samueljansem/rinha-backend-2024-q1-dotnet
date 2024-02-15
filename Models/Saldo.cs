using System;
using Microsoft.Extensions.ObjectPool;

public class Saldo : IResettable
{
  public int Total { get; set; }
  public int Limite { get; set; }
  public DateTime DataExtrato { get; set; } = DateTime.Now;

  public bool TryReset()
  {
    Total = 0;
    Limite = 0;
    DataExtrato = DateTime.MinValue;
    return true;
  }
}

public class SaldoPoolPolicy : IPooledObjectPolicy<Saldo>
{
  public Saldo Create() => new Saldo();
  public bool Return(Saldo obj)
  {
    obj.TryReset();
    return true;
  }
}