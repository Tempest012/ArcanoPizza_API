using ArcanoPizza_API.Services;
using System.Diagnostics;

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
        var sw = Stopwatch.StartNew();
        await _next(context);
        sw.Stop();

        if (!ShouldLog(context))
            return;

        try
        {
            await auditLogService.WriteHttpRequestAsync(
                context,
                duracionMs: (int)Math.Min(int.MaxValue, sw.ElapsedMilliseconds),
                cancellationToken: context.RequestAborted);
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
