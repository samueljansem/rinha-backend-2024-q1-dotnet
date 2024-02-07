using System.Text.Json.Serialization;

public class SaldoDTO
{
  [JsonPropertyName("total")]
  public int Total { get; set; }

  [JsonPropertyName("limite")]
  public int Limite { get; set; }

  [JsonPropertyName("data_extrato")]
  public DateTime DataExtrato { get; set; }
}