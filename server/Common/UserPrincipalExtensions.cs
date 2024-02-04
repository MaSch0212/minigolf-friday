using System.Security.Claims;

namespace MinigolfFriday;

public static class UserPrincipalExtensions
{
    public static UserLoginType? GetLoginType(this ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true)
            return null;

        var loginType = user.Claims
            .FirstOrDefault(x => x.Type == CustomClaimNames.LoginType)
            ?.Value;
        if (!Enum.TryParse<UserLoginType>(loginType, out var parsedLoginType))
            return null;

        return parsedLoginType;
    }
}
