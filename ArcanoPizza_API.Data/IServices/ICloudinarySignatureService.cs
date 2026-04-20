using ArcanoPizza_API.Data.Services.Models;

namespace ArcanoPizza_API.Data.IServices;

public interface ICloudinarySignatureService
{
    (CloudinarySignatureResult? Result, string? Error) CreateSignature(string? folder, string? publicId);
}
