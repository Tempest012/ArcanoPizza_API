using ArcanoPizza_API.Services;

namespace ArcanoPizza_API.Middleware;

public sealed class AuditLogMiddleware
{
    private readonly RequestDelegate _next;

    public AuditLogMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuditLogService auditLogService)
    {
        await _next(context);

        if (!ShouldLog(context))
            return;

        try
        {
            await auditLogService.WriteHttpRequestAsync(context, context.RequestAborted);
        }
        catch
        {
            // La respuesta ya se envió; no propagar.
        }
    }

    private static bool ShouldLog(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (!path.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
            return false;
        if (HttpMethods.IsOptions(context.Request.Method))
            return false;
        if (path.StartsWith("/api/audit-logs", StringComparison.OrdinalIgnoreCase))
            return false;
        return true;
    }
}
