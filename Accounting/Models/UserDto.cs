using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Enums;

namespace Accounting.Models;

[Table("Users", Schema = "Accounting")]
public record UserDto
{
    [Column(nameof(Id)), Key]
    public long Id { get; set; }
    [Column(nameof(Ulid))]
    public string Ulid { get; set; }
    [Column(nameof(Role))]
    public RoleEnum Role { get; set; }
    [Column(nameof(Balance))]
    public decimal Balance { get; set; }
}