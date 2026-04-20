using ArcanoPizza_API.Data.IServices;
using ArcanoPizza_API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ArcanoPizza_API.Data.Services;

public class AuditLogsQueryService : IAuditLogsQueryService
{
    private readonly ArcanoPizzaDbContext _db;

    public AuditLogsQueryService(ArcanoPizzaDbContext db)
    {
        _db = db;
    }

    public async Task<PagedAuditLogsResponseDto> QueryAsync(
        int page,
        int pageSize,
        DateTime? desde,
        DateTime? hasta,
        string? categoria,
        string? nivel,
        int? statusCode,
        string? metodoHttp,
        string? ip,
        string? usuario,
        string? q,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var query = _db.AuditLogs.AsNoTracking().AsQueryable();

        if (desde.HasValue)
            query = query.Where(x => x.OcurrioEn >= desde.Value);
        if (hasta.HasValue)
            query = query.Where(x => x.OcurrioEn <= hasta.Value);
        if (!string.IsNullOrWhiteSpace(categoria))
        {
            var cat = categoria.Trim();
            query = query.Where(x => x.Categoria == cat);
        }
        if (!string.IsNullOrWhiteSpace(nivel))
        {
            var n = nivel.Trim();
            query = query.Where(x => x.Nivel == n);
        }
        if (statusCode.HasValue)
            query = query.Where(x => x.CodigoEstado == statusCode.Value);
        if (!string.IsNullOrWhiteSpace(metodoHttp))
        {
            var m = metodoHttp.Trim();
            query = query.Where(x => x.MetodoHttp == m);
        }
        if (!string.IsNullOrWhiteSpace(ip))
        {
            var ipNeedle = ip.Trim();
            query = query.Where(x => x.Ip != null && EF.Functions.ILike(x.Ip, $"%{ipNeedle}%"));
        }
        if (!string.IsNullOrWhiteSpace(usuario))
        {
            var u = usuario.Trim();
            query = query.Where(x =>
                (x.FkIdUsuario != null && x.FkIdUsuario.ToString() == u) ||
                (x.Usuario != null && x.Usuario.Correo != null && EF.Functions.ILike(x.Usuario.Correo, $"%{u}%")));
        }
        if (!string.IsNullOrWhiteSpace(q))
        {
            var needle = q.Trim();
            query = query.Where(x =>
                EF.Functions.ILike(x.Mensaje, $"%{needle}%")
                || (x.Ruta != null && EF.Functions.ILike(x.Ruta, $"%{needle}%"))
                || (x.Detalle != null && EF.Functions.ILike(x.Detalle, $"%{needle}%")));
        }

        var total = await query.CountAsync(ct);

        var rows = await query
            .Include(x => x.Usuario)
            .OrderByDescending(x => x.OcurrioEn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

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
            UserAgent = x.UserAgent,
            MetodoHttp = x.MetodoHttp,
            Ruta = x.Ruta,
            CodigoEstado = x.CodigoEstado,
            DuracionMs = x.DuracionMs,
            TraceId = x.TraceId,
            Detalle = x.Detalle,
        }).ToList();

        return new PagedAuditLogsResponseDto
        {
            Total = total,
            Page = page,
            PageSize = pageSize,
            Items = items,
        };
    }
}

