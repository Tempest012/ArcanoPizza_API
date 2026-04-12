namespace ArcanoPizza_API.Model;

public class RefreshToken
{
    public int IdRefreshToken { get; set; }
    public int FkIdUsuario { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByTokenHash { get; set; }

    public Usuario Usuario { get; set; } = null!;
}
