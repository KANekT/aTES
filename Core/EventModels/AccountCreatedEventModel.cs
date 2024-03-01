using Core.Enums;

namespace Core.EventModels;

public class AccountCreatedEventModel
{
    public string PublicId { get; set; }
    public RoleEnum Role { get; set; }
}