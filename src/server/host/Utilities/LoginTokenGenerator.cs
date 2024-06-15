using System.Security.Cryptography;

namespace MinigolfFriday.Host.Utilities;

public static class LoginTokenGenerator
{
    private const string CHARS = "abcdefghijklmnopqrstuvwxyz0123456789";
    private const int LENGTH = 10;

    public static string GetLoginToken()
    {
        return RandomNumberGenerator.GetString(CHARS, LENGTH);
    }
}
