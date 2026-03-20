using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.DTOs;
using ArcanoPizza_API.Model;
using Microsoft.AspNetCore.Mvc;

namespace ArcanoPizza_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExtrasController : ControllerBase
{
    private readonly IExtraRepository _extraRepository;

    public ExtrasController(IExtraRepository extraRepository)
    {
        _extraRepository = extraRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExtraResponseDto>>> GetAll(CancellationToken ct)
    {
        var extras = await _extraRepository.GetAllAsync(ct);
        var dtos = extras.Select(e => new ExtraResponseDto(e.IdExtra, e.Nombre, e.Precio, e.Activo));
        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExtraResponseDto>> GetById(int id, CancellationToken ct)
    {
        var extra = await _extraRepository.GetByIdAsync(id, ct);
        if (extra is null)
            return NotFound();

        return Ok(new ExtraResponseDto(extra.IdExtra, extra.Nombre, extra.Precio, extra.Activo));
    }

    [HttpPost]
    public async Task<ActionResult<ExtraResponseDto>> Create([FromBody] ExtraCreateDto dto, CancellationToken ct)
    {
        var extra = new Extra
        {
            Nombre = dto.Nombre,
            Precio = dto.Precio,
            Activo = dto.Activo,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _extraRepository.AddAsync(extra, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.IdExtra },
            new ExtraResponseDto(created.IdExtra, created.Nombre, created.Precio, created.Activo));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] ExtraUpdateDto dto, CancellationToken ct)
    {
        var extra = await _extraRepository.GetByIdAsync(id, ct);
        if (extra is null)
            return NotFound();

        if (dto.Nombre is not null) extra.Nombre = dto.Nombre;
        if (dto.Precio.HasValue) extra.Precio = dto.Precio.Value;
        if (dto.Activo.HasValue) extra.Activo = dto.Activo.Value;
        extra.UpdatedAt = DateTime.UtcNow;

        await _extraRepository.UpdateAsync(extra, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id, CancellationToken ct)
    {
        var extra = await _extraRepository.GetByIdAsync(id, ct);
        if (extra is null)
            return NotFound();

        await _extraRepository.DeleteAsync(extra, ct);
        return NoContent();
    }
}
