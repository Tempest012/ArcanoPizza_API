using ArcanoPizza_API.Data.IServices;
using ArcanoPizza_API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArcanoPizza_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExtrasController : ControllerBase
{
    private readonly IExtraService _extras;

    public ExtrasController(IExtraService extras)
    {
        _extras = extras;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExtraResponseDto>>> GetAll(CancellationToken ct)
    {
        var dtos = await _extras.GetAllAsync(ct);
        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExtraResponseDto>> GetById(int id, CancellationToken ct)
    {
        var dto = await _extras.GetByIdAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Tecnico")]
    public async Task<ActionResult<ExtraResponseDto>> Create([FromBody] ExtraCreateDto dto, CancellationToken ct)
    {
        var created = await _extras.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.IdExtra }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador,Tecnico")]
    public async Task<ActionResult> Update(int id, [FromBody] ExtraUpdateDto dto, CancellationToken ct)
    {
        var (found, _) = await _extras.UpdateAsync(id, dto, ct);
        return found ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador,Tecnico")]
    public async Task<ActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _extras.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }
}
