namespace ArcanoPizza_API.Model;

public class AuditLog
{
    public int IdAuditLog { get; set; }
    public DateTime OcurrioEn { get; set; }
    public string Nivel { get; set; } = "Info";
    public string Categoria { get; set; } = "Http";
    public string Mensaje { get; set; } = string.Empty;
    public int? FkIdUsuario { get; set; }
    public string? Ip { get; set; }
    public string? UserAgent { get; set; }
    public string? MetodoHttp { get; set; }
    public string? Ruta { get; set; }
    public int? CodigoEstado { get; set; }
    public int? DuracionMs { get; set; }
    public string? TraceId { get; set; }
    public string? Detalle { get; set; }

    public Usuario? Usuario { get; set; }
}
