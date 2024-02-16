using System.IdentityModel.Tokens.Jwt;

namespace MinigolfFriday.Services;

public interface IJwtService
{
    JwtSecurityToken GenerateToken(UserEntity user);
    JwtSecurityToken GenerateAdminToken();
    string WriteToken(JwtSecurityToken token);
}
