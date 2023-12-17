using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MinigolfFriday.Data;
using MinigolfFriday.Services;

namespace MinigolfFriday;

public class FacebookSignedRequestOptions : AuthenticationSchemeOptions
{
    public string? AppId { get; set; }
    public string? AppSecret { get; set; }
}

public class FacebookSignedRequestAuthHandler : AuthenticationHandler<FacebookSignedRequestOptions>
{
    private readonly IFacebookService _facebookService;

    public FacebookSignedRequestAuthHandler(
        IOptionsMonitor<FacebookSignedRequestOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IFacebookService facebookService
    )
        : base(options, logger, encoder)
    {
        _facebookService = facebookService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (string.IsNullOrEmpty(Options.AppId))
        {
            Logger.LogWarning("Facebook AppId is null or empty");
            return AuthenticateResult.NoResult();
        }

        if (string.IsNullOrEmpty(Options.AppSecret))
        {
            Logger.LogWarning("Facebook AppSecret is null or empty");
            return AuthenticateResult.NoResult();
        }

        var fbsr = _facebookService.GetSignedRequestFromCookie(Request.Cookies, Options.AppId);
        if (fbsr is null)
            return AuthenticateResult.NoResult();

        var parsed = _facebookService.ParseSignedRequest(fbsr, Options.AppSecret);
        if (parsed is null)
            return AuthenticateResult.Fail("Invalid Facebook Signed Request");

        var user = await _facebookService.GetUserFromSignedRequestAsync(parsed);
        if (user is null)
            return AuthenticateResult.Fail("User not found");

        var identity = new GenericIdentity(user.Name);
        identity.AddClaim(new(ClaimTypes.NameIdentifier, user.Id.ToString()));
        identity.AddClaim(new("FacebookId", user.FacebookId));
        identity.AddClaim(new("FacebookSignedRequest", fbsr));
        var principal = new GenericPrincipal(identity, null);
        return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
    }
}
