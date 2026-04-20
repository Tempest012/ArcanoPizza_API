using ArcanoPizza_API.Data.IServices;
using ArcanoPizza_API.DTOs;
using ArcanoPizza_API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArcanoPizza_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DireccionesController : ControllerBase
{
    private readonly IDireccionService _direcciones;

    public DireccionesController(IDireccionService direcciones)
    {
        _direcciones = direcciones;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DireccionDto>>> MisDirecciones(CancellationToken ct)
    {
        var userId = User.GetUsuarioId();
        var dto = await _direcciones.MisDireccionesAsync(userId, ct);
        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<DireccionDto>> Crear([FromBody] DireccionCrearDto dto, CancellationToken ct)
    {
        var userId = User.GetUsuarioId();
        var (creada, error) = await _direcciones.CrearAsync(userId, dto, ct);
        if (error is not null) return BadRequest(error);
        return Ok(creada);
    }
}
