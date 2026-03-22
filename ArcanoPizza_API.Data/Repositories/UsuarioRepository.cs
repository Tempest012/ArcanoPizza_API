using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.Model;
using Microsoft.EntityFrameworkCore;

namespace ArcanoPizza_API.Data.Repositories;

public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(ArcanoPizzaDbContext context) : base(context) { }

    public async Task<Usuario?> GetByCorreoNormalizedAsync(string correo, CancellationToken ct = default)
    {
        var normalized = correo.Trim().ToLowerInvariant();
        return await _dbSet.FirstOrDefaultAsync(u => u.Correo.ToLower() == normalized, ct);
    }
}
