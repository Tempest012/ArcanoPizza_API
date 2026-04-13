using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.DTOs;
using ArcanoPizza_API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArcanoPizza_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PromocionesController : ControllerBase
{
    private readonly IPromocionRepository _promociones;

    public PromocionesController(IPromocionRepository promociones)
    {
        _promociones = promociones;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PromocionResponseDto>>> GetActivas(CancellationToken ct)
    {
        var lista = await _promociones.FindAsync(p => p.Activo, ct);
        var ordenada = lista.OrderBy(p => p.IdPromocion);
        return Ok(ordenada.Select(ToDto));
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<IEnumerable<PromocionResponseDto>>> GetAllAdmin(CancellationToken ct)
    {
        var lista = await _promociones.FindAsync(_ => true, ct);
        var ordenada = lista.OrderBy(p => p.IdPromocion);
        return Ok(ordenada.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PromocionResponseDto>> GetById(int id, CancellationToken ct)
    {
        var p = await _promociones.GetByIdAsync(id, ct);
        if (p is null || !p.Activo)
            return NotFound();

        return Ok(ToDto(p));
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<PromocionResponseDto>> Create([FromBody] PromocionCreateDto dto, CancellationToken ct)
    {
        if (!Enum.IsDefined(typeof(TipoVigenciaPromocion), dto.TipoVigencia))
            return BadRequest("TipoVigencia no válido.");

        var tipo = (TipoVigenciaPromocion)dto.TipoVigencia;
        if (tipo == TipoVigenciaPromocion.FechaHasta && !dto.FechaValidaHasta.HasValue)
            return BadRequest("FechaValidaHasta es requerida para vigencia por fecha.");
        if (tipo == TipoVigenciaPromocion.DiaSemanaRecurrente && dto.DiaSemanaRecurrente is null)
            return BadRequest("DiaSemanaRecurrente es requerido para promoción recurrente.");

        var ahora = DateTime.UtcNow;
        var entity = new Promocion
        {
            Titulo = dto.Titulo,
            Descripcion = dto.Descripcion,
            Contenido = dto.Contenido,
            ImagenURL = dto.ImagenURL,
            PrecioOriginal = dto.PrecioOriginal,
            PrecioPromocional = dto.PrecioPromocional,
            TipoVigencia = tipo,
            FechaValidaHasta = tipo == TipoVigenciaPromocion.FechaHasta ? dto.FechaValidaHasta : null,
            DiaSemanaRecurrente = tipo == TipoVigenciaPromocion.DiaSemanaRecurrente ? dto.DiaSemanaRecurrente : null,
            Activo = dto.Activo,
            CreatedAt = ahora,
            UpdatedAt = ahora
        };

        var created = await _promociones.AddAsync(entity, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.IdPromocion }, ToDto(created));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult> Update(int id, [FromBody] PromocionUpdateDto dto, CancellationToken ct)
    {
        var p = await _promociones.GetByIdAsync(id, ct);
        if (p is null)
            return NotFound();

        var error = AplicarActualizacionParcial(p, dto);
        if (error is not null)
            return BadRequest(error);

        p.UpdatedAt = DateTime.UtcNow;
        await _promociones.UpdateAsync(p, ct);
        return NoContent();
    }

    /// <summary>
    /// Actualización tipo PATCH: solo se modifican propiedades presentes en el DTO.
    /// </summary>
    /// <returns>Mensaje de error de validación, o null si todo es válido.</returns>
    private static string? AplicarActualizacionParcial(Promocion p, PromocionUpdateDto dto)
    {
        if (dto.Titulo is not null) p.Titulo = dto.Titulo;
        if (dto.Descripcion is not null) p.Descripcion = dto.Descripcion;
        if (dto.Contenido is not null) p.Contenido = dto.Contenido;
        if (dto.ImagenURL is not null) p.ImagenURL = dto.ImagenURL;
        if (dto.PrecioOriginal.HasValue) p.PrecioOriginal = dto.PrecioOriginal.Value;
        if (dto.PrecioPromocional.HasValue) p.PrecioPromocional = dto.PrecioPromocional.Value;

        if (dto.TipoVigencia.HasValue)
        {
            if (!Enum.IsDefined(typeof(TipoVigenciaPromocion), dto.TipoVigencia.Value))
                return "TipoVigencia no válido.";
            p.TipoVigencia = (TipoVigenciaPromocion)dto.TipoVigencia.Value;
        }

        if (dto.FechaValidaHasta.HasValue) p.FechaValidaHasta = dto.FechaValidaHasta;
        if (dto.DiaSemanaRecurrente.HasValue) p.DiaSemanaRecurrente = dto.DiaSemanaRecurrente;
        if (dto.Activo.HasValue) p.Activo = dto.Activo.Value;

        return null;
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult> Delete(int id, CancellationToken ct)
    {
        var p = await _promociones.GetByIdAsync(id, ct);
        if (p is null)
            return NotFound();

        await _promociones.DeleteAsync(p, ct);
        return NoContent();
    }

    private static PromocionResponseDto ToDto(Promocion p)
    {
        var ahorro = p.PrecioOriginal >= p.PrecioPromocional
            ? p.PrecioOriginal - p.PrecioPromocional
            : 0m;

        return new PromocionResponseDto(
            p.IdPromocion,
            p.Titulo,
            p.Descripcion,
            p.Contenido,
            p.ImagenURL,
            p.PrecioOriginal,
            p.PrecioPromocional,
            ahorro,
            (int)p.TipoVigencia,
            p.FechaValidaHasta,
            p.DiaSemanaRecurrente,
            p.Activo);
    }
}
