using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.Model;
using Microsoft.EntityFrameworkCore;

namespace ArcanoPizza_API.Data.Repositories;

public class MesaRepository : IMesaRepository
{
    private readonly ArcanoPizzaDbContext _db;

    public MesaRepository(ArcanoPizzaDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Mesa>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Mesas
            .AsNoTracking()
            .OrderBy(m => m.Numero)
            .ToListAsync(ct);
    }

    public Task<Mesa?> GetByIdAsync(int idMesa, CancellationToken ct = default) =>
        _db.Mesas.FirstOrDefaultAsync(m => m.IdMesa == idMesa, ct);

    public Task<Mesa?> GetByNumeroAsync(int numero, CancellationToken ct = default) =>
        _db.Mesas.AsNoTracking().FirstOrDefaultAsync(m => m.Numero == numero, ct);

    public async Task<Mesa> CrearAsync(Mesa mesa, CancellationToken ct = default)
    {
        _db.Mesas.Add(mesa);
        await _db.SaveChangesAsync(ct);
        return mesa;
    }

    public async Task ActualizarAsync(Mesa mesa, CancellationToken ct = default)
    {
        mesa.UpdatedAt = DateTime.UtcNow;
        _db.Mesas.Update(mesa);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> EliminarAsync(int idMesa, CancellationToken ct = default)
    {
        var mesa = await _db.Mesas.FindAsync(new object[] { idMesa }, ct);
        if (mesa is null) return false;

        var tienePedidosActivos = await _db.Pedidos.AnyAsync(
            p => p.FkIdMesa == idMesa
                 && p.TipoEntrega == SalonEstados.TipoEntregaSalon
                 && p.Estado != SalonEstados.Pagado
                 && p.Estado != SalonEstados.Cancelado,
            ct);
        if (tienePedidosActivos)
            throw new InvalidOperationException("No se puede eliminar una mesa con órdenes activas.");

        _db.Mesas.Remove(mesa);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
