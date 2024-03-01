using Core.Enums;

namespace Core.EventModels;

public class AccountRoleChangeEventModel
{
    public string PublicId { get; set; }
    public RoleEnum Role { get; set; }
}