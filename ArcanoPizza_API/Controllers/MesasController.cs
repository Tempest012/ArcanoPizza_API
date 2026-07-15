using ArcanoPizza_API.Data.IServices;
using ArcanoPizza_API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArcanoPizza_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MesasController : ControllerBase
{
    private readonly IMesasService _mesas;

    public MesasController(IMesasService mesas)
    {
        _mesas = mesas;
    }

    [HttpGet]
    [Authorize(Roles = "Administrador,Tecnico,Despachador,Operador")]
    public async Task<ActionResult<IReadOnlyList<MesaDto>>> Listar(CancellationToken ct)
    {
        return Ok(await _mesas.ListarAsync(ct));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Administrador,Tecnico,Despachador,Operador")]
    public async Task<ActionResult<MesaDto>> Obtener(int id, CancellationToken ct)
    {
        var mesa = await _mesas.ObtenerAsync(id, ct);
        return mesa is null ? NotFound() : Ok(mesa);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Tecnico")]
    public async Task<ActionResult<MesaDto>> Crear([FromBody] MesaCrearDto dto, CancellationToken ct)
    {
        var (mesa, error, status) = await _mesas.CrearAsync(dto, ct);
        if (error is not null)
            return StatusCode(status, new { mensaje = error });
        return Created($"/api/Mesas/{mesa!.IdMesa}", mesa);
    }

    [HttpPatch("{id:int}/estado")]
    [Authorize(Roles = "Administrador,Tecnico")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] MesaEstadoDto dto, CancellationToken ct)
    {
        var (ok, error, status) = await _mesas.CambiarEstadoAsync(id, dto.Estado, ct);
        if (!ok)
            return StatusCode(status, new { mensaje = error });
        return Ok(new { mensaje = "Estado de mesa actualizado", estado = dto.Estado });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador,Tecnico")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        var (ok, error, status) = await _mesas.EliminarAsync(id, ct);
        if (!ok)
            return StatusCode(status, new { mensaje = error });
        return NoContent();
    }
}
