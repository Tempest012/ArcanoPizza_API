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

    public async Task WriteHttpRequestAsync(HttpContext context, CancellationToken cancellationToken = default)
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

        var log = new AuditLog
        {
            OcurrioEn = DateTime.UtcNow,
            Nivel = nivel,
            Categoria = "Http",
            Mensaje = $"{method} {path} -> {status}",
            FkIdUsuario = userId,
            Ip = context.Connection.RemoteIpAddress?.ToString(),
            MetodoHttp = metodo,
            Ruta = string.IsNullOrEmpty(ruta) ? null : ruta,
            CodigoEstado = status,
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
