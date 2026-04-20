using ArcanoPizza_API.Data;
using ArcanoPizza_API.DTOs;
using ArcanoPizza_API.Data.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArcanoPizza_API.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Roles = "Tecnico")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogsQueryService _query;

    public AuditLogsController(IAuditLogsQueryService query)
    {
        _query = query;
    }

    [HttpGet]
    public async Task<ActionResult<PagedAuditLogsResponseDto>> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null,
        [FromQuery] string? categoria = null,
        [FromQuery] string? nivel = null,
        [FromQuery] int? statusCode = null,
        [FromQuery] string? metodoHttp = null,
        [FromQuery] string? ip = null,
        [FromQuery] string? usuario = null,
        [FromQuery] string? q = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _query.QueryAsync(
            page,
            pageSize,
            desde,
            hasta,
            categoria,
            nivel,
            statusCode,
            metodoHttp,
            ip,
            usuario,
            q,
            cancellationToken);

        return Ok(result);
    }
}
