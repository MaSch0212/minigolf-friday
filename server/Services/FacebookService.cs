using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;

namespace MinigolfFriday.Services;

public class FacebookService(
    ILoggerFactory loggerFactory,
    IHttpClientFactory httpClientFactory,
    MinigolfFridayContext dbContext,
    IFacebookAccessTokenProvider facebookAccessTokenProvider
) : IFacebookService
{
    private readonly ILogger<FacebookService> _logger =
        loggerFactory.CreateLogger<FacebookService>();
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly MinigolfFridayContext _dbContext = dbContext;
    private readonly IFacebookAccessTokenProvider _facebookAccessTokenProvider =
        facebookAccessTokenProvider;

    public string? GetSignedRequestFromCookie(IRequestCookieCollection cookies, string appId)
    {
        return cookies[$"fbsr_{appId}"];
    }

    public FacebookSignedRequest? ParseSignedRequest(string signedRequest, string appSecret)
    {
        var data = DecodeSignedRequest(signedRequest, appSecret);
        return data != null ? JsonSerializer.Deserialize<FacebookSignedRequest>(data) : null;
    }

    public async Task<UserEntity?> GetUserFromSignedRequestAsync(
        FacebookSignedRequest signedRequest
    )
    {
        return await _dbContext
            .Users
            .FirstOrDefaultAsync(u => u.FacebookId == signedRequest.UserId);
    }

    public async Task<string?> GetNameOfUserAsync(string appId, string appSecret, string userId)
    {
        var accessToken = await _facebookAccessTokenProvider.GetAccessTokenAsync(appId, appSecret);
        if (accessToken is null)
            return null;

        using var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            accessToken
        );
        var response = await httpClient.GetAsync($"https://graph.facebook.com/{userId}");
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<FacebookUserInfo>(content);
            if (json is { Name: not null or "" })
                return json.Name;
            else
                _logger.LogError("Failed to get user info from Facebook: {content}", content);
        }
        else
        {
            _logger.LogError(
                "Failed to get user info from Facebook: {statusCode}",
                response.StatusCode
            );
        }

        return null;
    }

    private static string DecodeSignedRequest(string signedRequest, string appSecret)
    {
        try
        {
            if (signedRequest.Contains('.'))
            {
                string[] split = signedRequest.Split('.');

                string signatureRaw = FixBase64String(split[0]);
                string dataRaw = FixBase64String(split[1]);

                // the decoded signature
                byte[] signature = Convert.FromBase64String(signatureRaw);

                byte[] dataBuffer = Convert.FromBase64String(dataRaw);

                // JSON object
                string data = Encoding.UTF8.GetString(dataBuffer);

                byte[] appSecretBytes = Encoding.UTF8.GetBytes(appSecret);
                System.Security.Cryptography.HMAC hmac =
                    new System.Security.Cryptography.HMACSHA256(appSecretBytes);
                byte[] expectedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(split[1]));
                if (expectedHash.SequenceEqual(signature))
                {
                    return data;
                }
            }
        }
        catch
        {
            // error
        }
        return "";
    }

    private static string FixBase64String(string str)
    {
        while (str.Length % 4 != 0)
        {
            str = str.PadRight(str.Length + 1, '=');
        }
        return str.Replace("-", "+").Replace("_", "/");
    }

    private record struct FacebookUserInfo(
        [property: JsonPropertyName("id")] string? Id,
        [property: JsonPropertyName("name")] string? Name
    );
}
