using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ArcanoPizza_API.Data;
using ArcanoPizza_API.Model;

namespace ArcanoPizza_API.Services;

public class AuditLogService : IAuditLogService
{
    private readonly ArcanoPizzaDbContext _db;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(ArcanoPizzaDbContext db, ILogger<AuditLogService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task WriteHttpRequestAsync(
        HttpContext context,
        int? duracionMs = null,
        CancellationToken cancellationToken = default)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        var method = context.Request.Method;
        var status = context.Response.StatusCode;

        int? userId = null;
        var sub = context.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(sub, out var uid))
            userId = uid;

        var nivel = status >= 500 ? "Error" : status >= 400 ? "Warning" : "Info";
        var ruta = path.Length > 2048 ? path[..2048] : path;
        var metodo = method.Length > 10 ? method[..10] : method;
        var traceId = context.TraceIdentifier;
        if (traceId.Length > 64)
            traceId = traceId[..64];

        var ua = context.Request.Headers.UserAgent.ToString();
        if (!string.IsNullOrWhiteSpace(ua) && ua.Length > 512)
            ua = ua[..512];

        string? detalle = null;
        if (context.Items.TryGetValue("AuditLog:ExceptionSummary", out var summaryObj)
            && summaryObj is string summary
            && !string.IsNullOrWhiteSpace(summary))
        {
            detalle = summary.Length > 4000 ? summary[..4000] : summary;
        }

        var log = new AuditLog
        {
            OcurrioEn = DateTime.UtcNow,
            Nivel = nivel,
            Categoria = "Http",
            Mensaje = $"{method} {path} -> {status}",
            FkIdUsuario = userId,
            Ip = context.Connection.RemoteIpAddress?.ToString(),
            UserAgent = string.IsNullOrWhiteSpace(ua) ? null : ua,
            MetodoHttp = metodo,
            Ruta = string.IsNullOrEmpty(ruta) ? null : ruta,
            CodigoEstado = status,
            DuracionMs = duracionMs,
            TraceId = traceId,
            Detalle = detalle,
        };

        _db.AuditLogs.Add(log);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo guardar fila de auditoría HTTP.");
        }
    }
}
