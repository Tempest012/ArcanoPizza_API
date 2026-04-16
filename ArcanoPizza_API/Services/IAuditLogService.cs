namespace ArcanoPizza_API.Services;

public interface IAuditLogService
{
    Task WriteHttpRequestAsync(HttpContext context, CancellationToken cancellationToken = default);
}
