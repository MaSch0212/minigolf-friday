using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MinigolfFriday.Services;

public class FacebookAccessTokenProvider(
    ILoggerFactory loggerFactory,
    IHttpClientFactory httpClientFactory
) : IFacebookAccessTokenProvider
{
    private readonly ILogger<FacebookAccessTokenProvider> _logger =
        loggerFactory.CreateLogger<FacebookAccessTokenProvider>();
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    private readonly Dictionary<string, string> _accessTokens = [];

    public async Task<string?> GetAccessTokenAsync(string appId, string appSecret)
    {
        if (_accessTokens.TryGetValue(appId, out string? accessToken))
            return accessToken;

        using var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.GetAsync(
            $"https://graph.facebook.com/oauth/access_token?client_id={appId}&client_secret={appSecret}&grant_type=client_credentials"
        );
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<FacebookAccessTokenResponse>(content);
            if (json is { AccessToken: not null or "" })
            {
                _accessTokens[appId] = json.AccessToken;
                return json.AccessToken;
            }
            else
            {
                _logger.LogError("Failed to get access token from Facebook: {content}", content);
            }
        }
        else
        {
            _logger.LogError(
                "Failed to get access token from Facebook: {statusCode}",
                response.StatusCode
            );
        }

        return null;
    }

    private record struct FacebookAccessTokenResponse(
        [property: JsonPropertyName("access_token")] string? AccessToken,
        [property: JsonPropertyName("token_type")] string? TokenType
    );
}
