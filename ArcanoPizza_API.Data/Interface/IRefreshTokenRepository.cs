using ArcanoPizza_API.Model;

namespace ArcanoPizza_API.Data.Interface;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    /// <summary>Obtiene refresh token activo (no revocado, no expirado) con su usuario.</summary>
    Task<RefreshToken?> GetActiveWithUsuarioByTokenHashAsync(string tokenHash, CancellationToken ct = default);

    /// <summary>Revoca el token anterior y persiste el nuevo en una transacción.</summary>
    Task RotateAsync(RefreshToken oldToken, RefreshToken newToken, CancellationToken ct = default);
}
