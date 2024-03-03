using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Enums;

namespace Tasks.Models;

[Table("Users", Schema = "Tasks")]
public record UserDto
{
    [Column(nameof(Id)), Key]
    public long Id { get; set; }
    [Column(nameof(Ulid))]
    public string Ulid { get; set; } = new Ulid().ToString();
    [Column(nameof(Role))]
    public RoleEnum Role { get; set; }
}