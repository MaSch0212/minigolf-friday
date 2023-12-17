using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MinigolfFriday.Services;

namespace MinigolfFriday;

public record GetAuthorizationResponse(bool IsAuthorized, string Reason);

[Route("api/auth")]
public class AuthController(
    IFacebookService facebookService,
    IOptionsMonitor<FacebookOptions> facebookOptions
) : Controller
{
    private readonly IFacebookService _facebookService = facebookService;
    private readonly IOptionsMonitor<FacebookOptions> _facebookOptions = facebookOptions;

    [HttpGet]
    [AllowAnonymous]
    public async ValueTask<IActionResult> GetIsAuthorized()
    {
        var options = _facebookOptions.CurrentValue;

        var fbsr = _facebookService.GetSignedRequestFromCookie(Request.Cookies, options.AppId);
        if (fbsr is null)
            return Ok(new GetAuthorizationResponse(false, "Not authenticated."));

        var parsed = _facebookService.ParseSignedRequest(fbsr, options.AppSecret);
        if (parsed is null)
            return Ok(new GetAuthorizationResponse(false, "Invalid Facebook Signed Request."));

        var user = await _facebookService.GetUserFromSignedRequestAsync(parsed);
        if (user is null)
            return Ok(new GetAuthorizationResponse(false, "Not authorized."));

        return Ok(new GetAuthorizationResponse(true, "Authorized."));
    }
}
