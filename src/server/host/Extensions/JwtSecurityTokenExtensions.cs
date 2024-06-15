using System.IdentityModel.Tokens.Jwt;

namespace MinigolfFriday.Host.Extensions;

public static class JwtSecurityTokenExtensions
{
    public static string ToTokenString(this JwtSecurityToken token)
    {
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
