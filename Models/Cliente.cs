using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("clientes")]
public class Cliente
{
  [Key]
  [Column("id")]
  public int Id { get; set; }

  [Column("saldo")]
  public int Saldo { get; set; }

  [Column("limite")]
  public int Limite { get; set; }

  public IEnumerable<Transacao> Transacoes { get; set; }
}