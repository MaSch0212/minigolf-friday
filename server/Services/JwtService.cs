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

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Name, user.Name),
            new(CustomClaimNames.LoginType, user.GetLoginType().ToString()),
            new("role", user.IsAdmin ? Roles.Admin : Roles.Player),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (user.FacebookId is not null)
            claims.Add(new(CustomClaimNames.FacebookId, user.FacebookId));
        if (user.Email is not null)
            claims.Add(new(JwtRegisteredClaimNames.Email, user.Email));

        var token = new JwtSecurityToken(
            jwtOptions.Issuer,
            jwtOptions.Audience,
            claims,
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
