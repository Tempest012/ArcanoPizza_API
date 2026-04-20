using ArcanoPizza_API.DTOs;

namespace ArcanoPizza_API.Data.IServices;

public interface IAuditLogsQueryService
{
    Task<PagedAuditLogsResponseDto> QueryAsync(
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
        CancellationToken ct = default);
}
