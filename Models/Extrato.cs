using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.ObjectPool;

public class Extrato : IResettable
{
  public Saldo? Saldo { get; set; } = null;
  public IEnumerable<Transacao> UltimasTransacoes { get; set; } = Enumerable.Empty<Transacao>();

  public bool TryReset()
  {
    Saldo = null;
    UltimasTransacoes = Enumerable.Empty<Transacao>();
    return true;
  }
}

public class ExtratoPoolPolicy : IPooledObjectPolicy<Extrato>
{
  public Extrato Create() => new Extrato();
  public bool Return(Extrato obj)
  {
    obj.TryReset();
    return true;
  }
}