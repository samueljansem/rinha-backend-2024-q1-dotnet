using System;
using Microsoft.Extensions.ObjectPool;

public class Transacao : IResettable
{
  public int Valor { get; set; }
  public string Descricao { get; set; } = string.Empty;
  public char Tipo { get; set; }
  public DateTime RealizadaEm { get; set; } = DateTime.Now;

  public bool TryReset()
  {
    Valor = 0;
    Descricao = string.Empty;
    Tipo = default;
    RealizadaEm = DateTime.Now;
    return true;
  }
}

public class TransacaoPoolPolicy : IPooledObjectPolicy<Transacao>
{
  public Transacao Create() => new Transacao();
  public bool Return(Transacao obj)
  {
    obj.TryReset();
    return true;
  }
}