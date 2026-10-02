using System.Security.Cryptography;
using System.Text;

namespace MoviePlatform.Infrastructure.Identity;

public static class TokenHashing
{
    public static string Hash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }

    public static string GenerateSecureToken(int bytes = 48)
    {
        var buffer = RandomNumberGenerator.GetBytes(bytes);
        return Convert.ToBase64String(buffer);
    }
}
