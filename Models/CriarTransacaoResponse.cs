using Microsoft.Extensions.ObjectPool;

public class CriarTransacaoResponse : IResettable
{
  public int Limite { get; set; }
  public int Saldo { get; set; }

  public bool TryReset()
  {
    Limite = 0;
    Saldo = 0;
    return true;
  }
}

public class CriarTransacaoResponsePoolPolicy : IPooledObjectPolicy<CriarTransacaoResponse>
{
  public CriarTransacaoResponse Create() => new CriarTransacaoResponse();
  public bool Return(CriarTransacaoResponse obj)
  {
    obj.TryReset();
    return true;
  }
}
