using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.DTOs;
using ArcanoPizza_API.Helpers;
using ArcanoPizza_API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArcanoPizza_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DireccionesController : ControllerBase
{
    private readonly IDireccionRepository _direccionRepository;

    public DireccionesController(IDireccionRepository direccionRepository)
    {
        _direccionRepository = direccionRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DireccionDto>>> MisDirecciones(CancellationToken ct)
    {
        var userId = User.GetUsuarioId();
        var list = await _direccionRepository.GetByUsuarioAsync(userId, ct);
        var dto = list.Select(d => new DireccionDto(d.IdDireccion, d.Calle, d.Colonia, d.CodigoPostal)).ToList();
        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<DireccionDto>> Crear([FromBody] DireccionCrearDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Calle)
            || string.IsNullOrWhiteSpace(dto.Colonia)
            || string.IsNullOrWhiteSpace(dto.CodigoPostal))
            return BadRequest("Calle, colonia y código postal son obligatorios.");

        var userId = User.GetUsuarioId();
        var now = DateTime.UtcNow;
        var entity = new Direccion
        {
            Calle = dto.Calle.Trim(),
            Colonia = dto.Colonia.Trim(),
            CodigoPostal = dto.CodigoPostal.Trim(),
            FkIdUsuario = userId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var creada = await _direccionRepository.AddAsync(entity, ct);
        return Ok(new DireccionDto(creada.IdDireccion, creada.Calle, creada.Colonia, creada.CodigoPostal));
    }
}
