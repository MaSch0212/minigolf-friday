using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;

namespace MinigolfFriday.Services;

public class FacebookService(MinigolfFridayContext dbContext) : IFacebookService
{
    private readonly MinigolfFridayContext _dbContext = dbContext;

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
}
