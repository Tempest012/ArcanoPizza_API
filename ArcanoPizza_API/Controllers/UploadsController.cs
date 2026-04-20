using ArcanoPizza_API.Data.IServices;
using ArcanoPizza_API.Data.Services.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArcanoPizza_API.Controllers;

[ApiController]
[Route("api/uploads")]
[Authorize(Roles = "Administrador,Tecnico")]
public class UploadsController : ControllerBase
{
    private readonly ICloudinarySignatureService _cloudinarySignature;

    public UploadsController(ICloudinarySignatureService cloudinarySignature)
    {
        _cloudinarySignature = cloudinarySignature;
    }

    public record CloudinarySignatureRequest(string? Folder, string? PublicId);

    /// <summary>
    /// Devuelve firma para upload firmado a Cloudinary desde el frontend (sin exponer ApiSecret).
    /// </summary>
    [HttpPost("cloudinary/signature")]
    public ActionResult<CloudinarySignatureResult> GetCloudinarySignature([FromBody] CloudinarySignatureRequest body)
    {
        var (result, error) = _cloudinarySignature.CreateSignature(body.Folder, body.PublicId);
        if (error is not null) return Problem(error);
        if (result is null) return Problem("No se pudo generar la firma.");
        return Ok(result);
    }
}

