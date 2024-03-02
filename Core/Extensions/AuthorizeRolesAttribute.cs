using Core.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Core.Extensions;

public class AuthorizeRolesAttribute : AuthorizeAttribute
{
    public AuthorizeRolesAttribute(params RoleEnum[] roles) : base()
    {
        var list = roles.Select(x => x.ToString("G"));
        Roles = string.Join(",", list);
    }
}