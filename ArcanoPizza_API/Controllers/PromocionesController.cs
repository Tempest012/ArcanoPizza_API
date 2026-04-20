using ArcanoPizza_API.Data.IServices;
using ArcanoPizza_API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArcanoPizza_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PromocionesController : ControllerBase
{
    private readonly IPromocionService _promocionService;

    public PromocionesController(IPromocionService promocionService)
    {
        _promocionService = promocionService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PromocionResponseDto>>> GetActivas(CancellationToken ct)
    {
        var lista = await _promocionService.GetActivasAsync(ct);
        return Ok(lista);
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Administrador,Tecnico")]
    public async Task<ActionResult<IEnumerable<PromocionResponseDto>>> GetAllAdmin(CancellationToken ct)
    {
        var lista = await _promocionService.GetAllAdminAsync(ct);
        return Ok(lista);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PromocionResponseDto>> GetById(int id, CancellationToken ct)
    {
        var dto = await _promocionService.GetByIdActivaAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Tecnico")]
    public async Task<ActionResult<PromocionResponseDto>> Create([FromBody] PromocionCreateDto dto, CancellationToken ct)
    {
        var (created, error) = await _promocionService.CreateAsync(dto, ct);
        if (error is not null) return BadRequest(error);
        if (created is null) return Problem("No se pudo crear la promoción.");
        return CreatedAtAction(nameof(GetById), new { id = created.IdPromocion }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador,Tecnico")]
    public async Task<ActionResult> Update(int id, [FromBody] PromocionUpdateDto dto, CancellationToken ct)
    {
        var (found, error) = await _promocionService.UpdateAsync(id, dto, ct);
        if (!found) return NotFound();
        if (error is not null) return BadRequest(error);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador,Tecnico")]
    public async Task<ActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _promocionService.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }
}
