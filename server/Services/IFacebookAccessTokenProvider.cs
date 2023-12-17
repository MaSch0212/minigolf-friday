namespace MinigolfFriday.Services;

public interface IFacebookAccessTokenProvider
{
    public Task<string?> GetAccessTokenAsync(string appId, string appSecret);
}
