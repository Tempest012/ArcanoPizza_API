using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArcanoPizza_API.Controllers;

[ApiController]
[Route("api/uploads")]
[Authorize(Roles = "Administrador,Tecnico")]
public class UploadsController : ControllerBase
{
    private readonly IConfiguration _config;

    public UploadsController(IConfiguration config)
    {
        _config = config;
    }

    public record CloudinarySignatureRequest(string? Folder, string? PublicId);

    public record CloudinarySignatureResponse(
        string CloudName,
        string ApiKey,
        long Timestamp,
        string Signature,
        string Folder,
        string? PublicId);

    /// <summary>
    /// Devuelve firma para upload firmado a Cloudinary desde el frontend (sin exponer ApiSecret).
    /// </summary>
    [HttpPost("cloudinary/signature")]
    public ActionResult<CloudinarySignatureResponse> GetCloudinarySignature([FromBody] CloudinarySignatureRequest body)
    {
        var cloudName = _config["Cloudinary:CloudName"] ?? "";
        var apiKey = _config["Cloudinary:ApiKey"] ?? "";
        var apiSecret = _config["Cloudinary:ApiSecret"] ?? "";
        var defaultFolder = _config["Cloudinary:DefaultFolder"] ?? "arcanoPizza";

        if (string.IsNullOrWhiteSpace(cloudName) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiSecret))
            return Problem("Falta configuración Cloudinary (Cloudinary:CloudName/ApiKey/ApiSecret).");

        var folder = string.IsNullOrWhiteSpace(body.Folder) ? defaultFolder : body.Folder.Trim();
        var publicId = string.IsNullOrWhiteSpace(body.PublicId) ? null : body.PublicId.Trim();

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Cloudinary signature: SHA1 of sorted params querystring + api_secret
        // params that we sign must match what the frontend sends to Cloudinary.
        var pairs = new List<KeyValuePair<string, string>>
        {
            new("folder", folder),
            new("timestamp", timestamp.ToString())
        };
        if (publicId is not null) pairs.Add(new("public_id", publicId));

        var sorted = pairs.OrderBy(p => p.Key, StringComparer.Ordinal).ToList();
        var toSign = string.Join("&", sorted.Select(p => $"{p.Key}={p.Value}")) + apiSecret;

        var signature = Sha1Hex(toSign);

        return Ok(new CloudinarySignatureResponse(
            cloudName,
            apiKey,
            timestamp,
            signature,
            folder,
            publicId));
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

