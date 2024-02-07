using System.Text.Json.Serialization;

public class CriarTransacaoRequest
{
  [JsonPropertyName("valor")]
  public int Valor { get; set; }

  [JsonPropertyName("tipo")]
  public char Tipo { get; set; }

  [JsonPropertyName("descricao")]
  public string Descricao { get; set; }
}