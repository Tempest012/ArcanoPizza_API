namespace ArcanoPizza_API.DTOs;

public class AuditLogItemDto
{
    public int Id { get; set; }
    public DateTime OcurrioEn { get; set; }
    public string Nivel { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public int? IdUsuario { get; set; }
    public string? CorreoUsuario { get; set; }
    public string? Ip { get; set; }
    public string? MetodoHttp { get; set; }
    public string? Ruta { get; set; }
    public int? CodigoEstado { get; set; }
}

public class PagedAuditLogsResponseDto
{
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public IReadOnlyList<AuditLogItemDto> Items { get; set; } = Array.Empty<AuditLogItemDto>();
}
