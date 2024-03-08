using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Enums;

namespace Analytics.Models;

[Table("Users", Schema = "Analytics")]
public record UserDto
{
    [Column(nameof(Id)), Key]
    public long Id { get; set; }
    [Column(nameof(Ulid))]
    public string Ulid { get; set; }
    [Column(nameof(Role))]
    public RoleEnum Role { get; set; }
}