using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.Model;
using Microsoft.EntityFrameworkCore;

namespace ArcanoPizza_API.Data.Repositories;

public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(ArcanoPizzaDbContext context) : base(context) { }

    public async Task<RefreshToken?> GetActiveWithUsuarioByTokenHashAsync(string tokenHash, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Include(x => x.Usuario)
            .FirstOrDefaultAsync(
                x => x.TokenHash == tokenHash
                     && x.RevokedAt == null
                     && x.ExpiresAt > now,
                ct);
    }

    public async Task RotateAsync(RefreshToken oldToken, RefreshToken newToken, CancellationToken ct = default)
    {
        await using var tx = await _context.Database.BeginTransactionAsync(ct);
        oldToken.RevokedAt = DateTime.UtcNow;
        oldToken.ReplacedByTokenHash = newToken.TokenHash;
        _dbSet.Update(oldToken);
        await _dbSet.AddAsync(newToken, ct);
        await _context.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }
}
