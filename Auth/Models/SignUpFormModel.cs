using Core.Enums;

namespace Auth.Models;

public record SignUpFormModel
{
    public string Login  { get; set; }
    public string UserName  { get; set; }
    public RoleEnum Role  { get; set; }
}