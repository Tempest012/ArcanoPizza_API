using ArcanoPizza_API.Data;
using ArcanoPizza_API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArcanoPizza_API.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Roles = "Tecnico")]
public class AuditLogsController : ControllerBase
{
    private readonly ArcanoPizzaDbContext _context;

    public AuditLogsController(ArcanoPizzaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<PagedAuditLogsResponseDto>> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null,
        [FromQuery] string? categoria = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        if (desde.HasValue)
            query = query.Where(x => x.OcurrioEn >= desde.Value);
        if (hasta.HasValue)
            query = query.Where(x => x.OcurrioEn <= hasta.Value);
        if (!string.IsNullOrWhiteSpace(categoria))
        {
            var cat = categoria.Trim();
            query = query.Where(x => x.Categoria == cat);
        }

        var total = await query.CountAsync(cancellationToken);

        var rows = await query
            .Include(x => x.Usuario)
            .OrderByDescending(x => x.OcurrioEn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = rows.Select(x => new AuditLogItemDto
        {
            Id = x.IdAuditLog,
            OcurrioEn = x.OcurrioEn,
            Nivel = x.Nivel,
            Categoria = x.Categoria,
            Mensaje = x.Mensaje,
            IdUsuario = x.FkIdUsuario,
            CorreoUsuario = x.Usuario?.Correo,
            Ip = x.Ip,
            MetodoHttp = x.MetodoHttp,
            Ruta = x.Ruta,
            CodigoEstado = x.CodigoEstado,
        }).ToList();

        return Ok(new PagedAuditLogsResponseDto
        {
            Total = total,
            Page = page,
            PageSize = pageSize,
            Items = items,
        });
    }
}
