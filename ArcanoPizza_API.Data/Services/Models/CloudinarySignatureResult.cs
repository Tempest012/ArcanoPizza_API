namespace ArcanoPizza_API.Data.Services.Models;

public record CloudinarySignatureResult(
    string CloudName,
    string ApiKey,
    long Timestamp,
    string Signature,
    string Folder,
    string? PublicId);

