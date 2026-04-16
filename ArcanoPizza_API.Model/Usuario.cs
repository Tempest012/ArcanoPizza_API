namespace ArcanoPizza_API.Model;

public class Usuario
{
    public int IdUsuario { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string? Telefono { get; set; }
    public DateTime? TimeStamp { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string Rol { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;


    public ICollection<Direccion> Direcciones { get; set; } = new List<Direccion>();
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    public ICollection<Pedido> PedidosComoRepartidor { get; set; } = new List<Pedido>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
