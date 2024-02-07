using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("transacoes")]
public class Transacao
{
  [Key]
  [Column("id")]
  public int Id { get; set; }

  [Column("valor")]
  public int Valor { get; set; }

  [Column("realizada_em")]
  public DateTime RealizadaEm { get; set; }

  [Column("tipo")]
  public char Tipo { get; set; }

  [Column("descricao")]
  public string Descricao { get; set; }

  [Column("id_cliente")]
  public int ClienteId { get; set; }

  public Cliente Cliente { get; set; }
}