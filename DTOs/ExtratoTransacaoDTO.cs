using System.Text.Json.Serialization;

public class TransacaoDTO
{
  [JsonPropertyName("valor")]
  public int Valor { get; set; }

  [JsonPropertyName("descricao")]
  public string Descricao { get; set; }

  [JsonPropertyName("tipo")]
  public char Tipo { get; set; }

  [JsonPropertyName("realizada_em")]
  public DateTime RealizadaEm { get; set; }
}