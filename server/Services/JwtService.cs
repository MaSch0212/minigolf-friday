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
        return GenerateToken(
            user.Id.ToString(),
            user.Name,
            user.GetLoginType(),
            user.IsAdmin ? Roles.Admin : Roles.Player,
            user.Email,
            user.FacebookId
        );
    }

    public JwtSecurityToken GenerateAdminToken()
    {
        return GenerateToken(
            Globals.AdminUser.Id,
            Globals.AdminUser.Name,
            Globals.AdminUser.LoginType,
            Roles.Admin,
            null,
            null
        );
    }

    public string WriteToken(JwtSecurityToken token)
    {
        return _tokenHandler.WriteToken(token);
    }

    private JwtSecurityToken GenerateToken(
        string userid,
        string name,
        UserLoginType loginType,
        string role,
        string? email,
        string? facebookId
    )
    {
        var jwtOptions = _jwtOptions.CurrentValue;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userid),
            new(JwtRegisteredClaimNames.Name, name),
            new(CustomClaimNames.LoginType, loginType.ToString()),
            new("role", role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (facebookId is not null)
            claims.Add(new(CustomClaimNames.FacebookId, facebookId));
        if (email is not null)
            claims.Add(new(JwtRegisteredClaimNames.Email, email));

        var token = new JwtSecurityToken(
            jwtOptions.Issuer,
            jwtOptions.Audience,
            claims,
            expires: DateTime.UtcNow.Add(jwtOptions.Expiration),
            signingCredentials: creds
        );

        return token;
    }
}
