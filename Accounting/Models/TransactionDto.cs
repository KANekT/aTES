using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Enums;

namespace Accounting.Models;

[Table("Transactions", Schema = "Accounting")]
public class TransactionDto
{
    [Column(nameof(Id)), Key]
    public long Id { get; set; }
    [Column(nameof(Ulid))] 
    public string Ulid { get; set; } = new Ulid().ToString();
    [Column(nameof(CreatedAt))]
    public DateTime CreatedAt { get; set; }
    [Column(nameof(System.Type))]
    public TransactionTypeEnum Type { get; set; }
    [Column(nameof(PoPugId))]
    public string PoPugId { get; set; } = string.Empty;
    [Column(nameof(Money))]
    public decimal Money { get; set; }
}