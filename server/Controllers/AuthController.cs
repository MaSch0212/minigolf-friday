using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MinigolfFriday.Services;

namespace MinigolfFriday;

public record GetAuthorizationResponse(bool IsAuthorized, string Reason);

[Route("api/[controller]")]
public class AuthController(
    FacebookService facebookService,
    IOptionsMonitor<FacebookSignedRequestOptions> facebookOptions
) : Controller
{
    private readonly FacebookService _facebookService = facebookService;
    private readonly IOptionsMonitor<FacebookSignedRequestOptions> _facebookOptions =
        facebookOptions;

    [HttpGet]
    public async ValueTask<IActionResult> Get()
    {
        var options = _facebookOptions.CurrentValue;
        if (string.IsNullOrEmpty(options.AppId) || string.IsNullOrEmpty(options.AppSecret))
            return Ok(new GetAuthorizationResponse(false, "Authentication not configured."));

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
