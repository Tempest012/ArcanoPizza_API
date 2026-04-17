namespace ArcanoPizza_API.Services;

public interface IAuditLogService
{
    Task WriteHttpRequestAsync(
        HttpContext context,
        int? duracionMs = null,
        CancellationToken cancellationToken = default);
}
