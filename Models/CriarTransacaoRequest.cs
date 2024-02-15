using Microsoft.Extensions.ObjectPool;

public class CriarTransacaoRequest : IResettable
{
  public int Valor { get; set; }
  public char Tipo { get; set; }
  public string Descricao { get; set; } = string.Empty;

  public bool TryReset()
  {
    Valor = 0;
    Tipo = default;
    Descricao = string.Empty;
    return true;
  }
}

public class CriarTransacaoRequestPoolPolicy : IPooledObjectPolicy<CriarTransacaoRequest>
{
  public CriarTransacaoRequest Create() => new CriarTransacaoRequest();
  public bool Return(CriarTransacaoRequest obj)
  {
    obj.TryReset();
    return true;
  }
}