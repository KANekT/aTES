using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Enums;

namespace Auth.Models;

[Table("Users", Schema = "Auth")]
public record UserDto
{
    [Column(nameof(Id)), Key]
    public long Id { get; set; }
    [Column(nameof(Ulid))]
    public string Ulid { get; set; } = new Ulid().ToString();
    [Column(nameof(CreatedAt))]
    public DateTime CreatedAt { get; set; }
    [Column(nameof(Login))]
    public string Login { get; set; }
    [Column(nameof(UserName))]
    public string UserName { get; set; }
    [Column(nameof(Role))]
    public RoleEnum Role { get; set; }
}