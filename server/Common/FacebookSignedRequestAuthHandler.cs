using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using MinigolfFriday.Services;

namespace MinigolfFriday;

public class FacebookSignedRequestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IFacebookService _facebookService;
    private readonly IOptionsMonitor<FacebookOptions> _facebookOptions;

    public FacebookSignedRequestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IFacebookService facebookService,
        IOptionsMonitor<FacebookOptions> facebookOptions
    )
        : base(options, logger, encoder)
    {
        _facebookService = facebookService;
        _facebookOptions = facebookOptions;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var facebookOptions = _facebookOptions.CurrentValue;

        var fbsr = _facebookService.GetSignedRequestFromCookie(
            Request.Cookies,
            facebookOptions.AppId
        );
        if (fbsr is null)
            return AuthenticateResult.NoResult();

        var parsed = _facebookService.ParseSignedRequest(fbsr, facebookOptions.AppSecret);
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
