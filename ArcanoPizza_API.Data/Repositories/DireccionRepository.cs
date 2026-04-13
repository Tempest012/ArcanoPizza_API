using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.Model;
using Microsoft.EntityFrameworkCore;

namespace ArcanoPizza_API.Data.Repositories;

public class DireccionRepository : IDireccionRepository
{
    private readonly ArcanoPizzaDbContext _context;

    public DireccionRepository(ArcanoPizzaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Direccion>> GetByUsuarioAsync(int idUsuario, CancellationToken ct = default)
    {
        return await _context.Direcciones
            .AsNoTracking()
            .Where(d => d.FkIdUsuario == idUsuario)
            .OrderByDescending(d => d.IdDireccion)
            .ToListAsync(ct);
    }

    public async Task<Direccion?> GetByIdForUsuarioAsync(int idDireccion, int idUsuario, CancellationToken ct = default)
    {
        return await _context.Direcciones
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.IdDireccion == idDireccion && d.FkIdUsuario == idUsuario, ct);
    }

    public async Task<Direccion> AddAsync(Direccion entity, CancellationToken ct = default)
    {
        await _context.Direcciones.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
        return entity;
    }
}
