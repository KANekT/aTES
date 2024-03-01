using Core.Enums;

namespace Auth.Models;

public record UpdateRoleFormModel
{
    public RoleEnum Role { get; set; }
}