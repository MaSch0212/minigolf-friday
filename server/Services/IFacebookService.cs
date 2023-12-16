using System.Net;

namespace MinigolfFriday.Services;

public interface IFacebookService
{
    public string? GetSignedRequestFromCookie(IRequestCookieCollection cookies, string appId);
    public FacebookSignedRequest? ParseSignedRequest(string signedRequest, string appSecret);
    public Task<UserEntity?> GetUserFromSignedRequestAsync(FacebookSignedRequest signedRequest);
}
