using System.Text.Json.Serialization;

public class CriarTransacaoResponse
{
  [JsonPropertyName("limite")]
  public int Limite { get; set; }

  [JsonPropertyName("saldo")]
  public int Saldo { get; set; }
}