using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinigolfFriday.Data;
using MinigolfFriday.Services;

namespace MinigolfFriday;

public record GetAccessTokenResponse(string Token, DateTime ExpiresAt);

[Route("api/auth")]
public class AuthController(
    IFacebookService facebookService,
    IUSerService userService,
    IJwtService jwtService
) : Controller
{
    private readonly IFacebookService _facebookService = facebookService;
    private readonly IUSerService _userService = userService;
    private readonly IJwtService _jwtService = jwtService;

    [HttpPost("token")]
    [AllowAnonymous]
    public async ValueTask<IActionResult> GetAccessToken()
    {
        var fbResult = await _facebookService.ValidateAsync(Request.Cookies, false);
        if (fbResult.IsFailed)
            return Unauthorized(fbResult.Errors);

        if (fbResult.Value.User is null)
        {
            var addUserResult = await _userService.AddUserAsync(
                fbResult.Value.SignedRequest.UserId,
                false
            );
            if (addUserResult.IsFailed)
                return addUserResult.ToActionResult();
        }

        var token = _jwtService.GenerateToken(fbResult.Value.User!);
        return Ok(new GetAccessTokenResponse(_jwtService.WriteToken(token), token.ValidTo));
    }
}
