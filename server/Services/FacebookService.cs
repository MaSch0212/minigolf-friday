using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MinigolfFriday.Data;

namespace MinigolfFriday.Services;

public class FacebookService(
    ILoggerFactory loggerFactory,
    IHttpClientFactory httpClientFactory,
    IFacebookAccessTokenProvider facebookAccessTokenProvider,
    IUserService userService,
    IOptionsMonitor<FacebookOptions> facebookOptions
) : IFacebookService
{
    private readonly ILogger<FacebookService> _logger =
        loggerFactory.CreateLogger<FacebookService>();
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IUserService _userService = userService;
    private readonly IFacebookAccessTokenProvider _facebookAccessTokenProvider =
        facebookAccessTokenProvider;
    private readonly IOptionsMonitor<FacebookOptions> _facebookOptions = facebookOptions;

    public async ValueTask<Result<FacebookValidationResult>> ValidateAsync(
        IRequestCookieCollection cookies,
        bool requiresUser
    )
    {
        var fbOptions = _facebookOptions.CurrentValue;
        var fbsr = cookies[$"fbsr_{fbOptions.AppId}"];
        if (fbsr is null)
            return Result.Fail("Not authenticated.");

        var parsed = ParseSignedRequest(fbsr, fbOptions.AppSecret);
        if (parsed is null)
            return Result.Fail("Invalid Facebook Signed Request.");

        var user = await _userService.GetUserByFacebookIdAsync(parsed.UserId);
        if (user is null && requiresUser)
            return Result.Fail("Not authenticated.");

        return Result.Ok(new FacebookValidationResult(parsed, user));
    }

    public async ValueTask<string?> GetNameOfUserAsync(string userId)
    {
        var fbOptions = _facebookOptions.CurrentValue;
        var accessToken = await _facebookAccessTokenProvider.GetAccessTokenAsync(
            fbOptions.AppId,
            fbOptions.AppSecret
        );
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

    private FacebookSignedRequest? ParseSignedRequest(string signedRequest, string appSecret)
    {
        var data = DecodeSignedRequest(signedRequest, appSecret);
        return data != null ? JsonSerializer.Deserialize<FacebookSignedRequest>(data) : null;
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
