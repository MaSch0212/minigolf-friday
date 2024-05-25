using FastEnumUtility;
using MaSch.Core.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MinigolfFriday.Models;
using MinigolfFriday.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MinigolfFriday.Services;

[GenerateAutoInterface]
public class JwtService(IOptionsMonitor<JwtOptions> jwtOptions, IIdService idService) : IJwtService
{
    private readonly IOptionsMonitor<JwtOptions> _jwtOptions = jwtOptions;

    public JwtSecurityToken GenerateToken(long userid, Role[] roles)
    {
        var jwtOptions = _jwtOptions.CurrentValue;

        var keyBytes = Encoding.UTF8.GetBytes(jwtOptions.Secret);
        if (keyBytes.Length < 32)
        {
            var tmp = keyBytes;
            keyBytes = new byte[32];
            Array.Copy(tmp, keyBytes, tmp.Length);
        }
        var key = new SymmetricSecurityKey(keyBytes);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userid < 0 ? "admin" : idService.User.Encode(userid)),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        foreach (var role in roles.Select(FastEnum.GetName).WhereNotNull())
            claims.Add(new(ClaimTypes.Role, role));

        var token = new JwtSecurityToken(
            jwtOptions.Issuer,
            jwtOptions.Audience,
            claims,
            expires: DateTime.UtcNow.Add(jwtOptions.Expiration),
            signingCredentials: creds
        );

        return token;
    }

    public bool TryGetUserId(ClaimsPrincipal user, out long userId)
    {
        var sub = user.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        if (sub != null)
        {
            if (sub == "admin")
            {
                userId = -1;
                return true;
            }

            var decoded = idService.User.Decode(sub);
            if (decoded.Count == 1)
            {
                userId = decoded[0];
                return true;
            }
        }

        userId = -1;
        return false;
    }
}
