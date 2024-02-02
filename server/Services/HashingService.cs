using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace MinigolfFriday.Services;

public class HashingService : IHashingService
{
    private const int SALT_BIT_SIZE = 128;
    private const int ITERATIONS = 10000;
    private const int HASH_BIT_SIZE = 256;

    public string HashPassword(string password)
    {
        var salt = new byte[SALT_BIT_SIZE / 8];
        var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);

        var hash = KeyDerivation.Pbkdf2(
            password,
            salt,
            KeyDerivationPrf.HMACSHA512,
            ITERATIONS,
            HASH_BIT_SIZE / 8
        );

        var hashWithSalt = new byte[salt.Length + hash.Length];
        salt.CopyTo(hashWithSalt, 0);
        hash.CopyTo(hashWithSalt, salt.Length);

        return $"pbkdf2v1;{SALT_BIT_SIZE};{ITERATIONS};{HASH_BIT_SIZE};{Convert.ToBase64String(hashWithSalt)}";
    }

    public bool VerifyPassword(string password, string hash)
    {
        if (hash.StartsWith("pbkdf2v1;"))
        {
            var parts = hash.Split(';');
            if (parts.Length != 5)
                return false;

            var saltSize = int.Parse(parts[1]);
            var iterations = int.Parse(parts[2]);
            var hashSize = int.Parse(parts[3]);
            var hashWithSalt = Convert.FromBase64String(parts[4]);

            var salt = new byte[saltSize / 8];
            var expectedHash = new byte[hashSize / 8];
            Array.Copy(hashWithSalt, 0, salt, 0, salt.Length);
            Array.Copy(hashWithSalt, salt.Length, expectedHash, 0, expectedHash.Length);

            var actualHash = KeyDerivation.Pbkdf2(
                password,
                salt,
                KeyDerivationPrf.HMACSHA512,
                iterations,
                hashSize / 8
            );

            return actualHash.Length == expectedHash.Length
                && actualHash.SequenceEqual(expectedHash);
        }
        return false;
    }
}
