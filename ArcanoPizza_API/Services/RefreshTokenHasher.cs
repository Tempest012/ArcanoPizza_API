using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace ArcanoPizza_API.Services;

public static class RefreshTokenHasher
{
    public static string Sha256Hex(string rawToken)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public static (string Raw, string Hash) GeneratePair()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        var raw = WebEncoders.Base64UrlEncode(bytes);
        return (raw, Sha256Hex(raw));
    }
}
