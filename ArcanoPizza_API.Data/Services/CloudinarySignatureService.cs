using System.Security.Cryptography;
using System.Text;
using ArcanoPizza_API.Data.IServices;
using ArcanoPizza_API.Data.Services.Models;
using Microsoft.Extensions.Configuration;

namespace ArcanoPizza_API.Data.Services;

public class CloudinarySignatureService : ICloudinarySignatureService
{
    private readonly IConfiguration _config;

    public CloudinarySignatureService(IConfiguration config)
    {
        _config = config;
    }

    public (CloudinarySignatureResult? Result, string? Error) CreateSignature(string? folder, string? publicId)
    {
        var cloudName = _config["Cloudinary:CloudName"] ?? "";
        var apiKey = _config["Cloudinary:ApiKey"] ?? "";
        var apiSecret = _config["Cloudinary:ApiSecret"] ?? "";
        var defaultFolder = _config["Cloudinary:DefaultFolder"] ?? "arcanoPizza";

        if (string.IsNullOrWhiteSpace(cloudName)
            || string.IsNullOrWhiteSpace(apiKey)
            || string.IsNullOrWhiteSpace(apiSecret))
        {
            return (null, "Falta configuración Cloudinary (Cloudinary:CloudName/ApiKey/ApiSecret).");
        }

        var finalFolder = string.IsNullOrWhiteSpace(folder) ? defaultFolder : folder.Trim();
        var finalPublicId = string.IsNullOrWhiteSpace(publicId) ? null : publicId.Trim();

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var pairs = new List<KeyValuePair<string, string>>
        {
            new("folder", finalFolder),
            new("timestamp", timestamp.ToString())
        };
        if (finalPublicId is not null) pairs.Add(new("public_id", finalPublicId));

        var sorted = pairs.OrderBy(p => p.Key, StringComparer.Ordinal).ToList();
        var toSign = string.Join("&", sorted.Select(p => $"{p.Key}={p.Value}")) + apiSecret;

        var signature = Sha1Hex(toSign);

        return (new CloudinarySignatureResult(
            cloudName,
            apiKey,
            timestamp,
            signature,
            finalFolder,
            finalPublicId), null);
    }

    private static string Sha1Hex(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA1.HashData(bytes);
        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}

