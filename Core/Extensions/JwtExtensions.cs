using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using Core.Options;
using Microsoft.IdentityModel.Tokens;

namespace Core.Extensions;

public static class JwtExtensions
{
    public static JwtSecurityToken GenerateToken(this AuthenticationOptions jwtOptions, string identityName, Claim[] claims)
    {
        var handler = new JwtSecurityTokenHandler();
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecurityKey));
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

        var identity = new ClaimsIdentity(new GenericIdentity(identityName), claims);
        var token = handler.CreateJwtSecurityToken(subject: identity,
            signingCredentials: signingCredentials,
            audience: jwtOptions.Audience,
            issuer: jwtOptions.Issuer,
            expires: DateTime.UtcNow.AddMinutes(jwtOptions.ExpirationMinutes));

        return token;
    }

    public static SymmetricSecurityKey GetSymmetricSecurityKey(this string key)
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    }
}