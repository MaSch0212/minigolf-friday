using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MinigolfFriday.Services;

public class JwtService(IOptionsMonitor<JwtOptions> jwtOptions) : IJwtService
{
    private readonly JwtSecurityTokenHandler _tokenHandler = new();
    private readonly IOptionsMonitor<JwtOptions> _jwtOptions = jwtOptions;

    public JwtSecurityToken GenerateToken(UserEntity user)
    {
        var jwtOptions = _jwtOptions.CurrentValue;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            jwtOptions.Issuer,
            jwtOptions.Audience,
            claims:
            [
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Name),
                new(JwtOptions.FacebookIdClaim, user.FacebookId),
                new(ClaimTypes.Role, user.IsAdmin ? Roles.Admin : Roles.Player)
            ],
            expires: DateTime.UtcNow.Add(jwtOptions.Expiration),
            signingCredentials: creds
        );

        return token;
    }

    public string WriteToken(JwtSecurityToken token)
    {
        return _tokenHandler.WriteToken(token);
    }
}
