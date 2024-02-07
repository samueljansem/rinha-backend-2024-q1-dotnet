using System.Text.Json.Serialization;

public class ExtratoDTO
{
  [JsonPropertyName("saldo")]
  public SaldoDTO Saldo { get; set; }

  [JsonPropertyName("ultimas_transacoes")]
  public IEnumerable<TransacaoDTO> UltimasTransacoes { get; set; }
}