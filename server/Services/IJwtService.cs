using System.IdentityModel.Tokens.Jwt;

namespace MinigolfFriday.Services;

public interface IJwtService
{
    JwtSecurityToken GenerateToken(UserEntity user);
    string WriteToken(JwtSecurityToken token);
}
