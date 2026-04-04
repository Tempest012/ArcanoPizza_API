using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.Model;
using Microsoft.EntityFrameworkCore;

namespace ArcanoPizza_API.Data.Repositories;

public class PedidoRepository : IPedidoRepository
{
    private readonly ArcanoPizzaDbContext _context;

    public PedidoRepository(ArcanoPizzaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Pedido>> GetByUsuarioAsync(int idUsuario, CancellationToken ct = default)
    {
        return await _context.Pedidos
            .AsNoTracking()
            .Include(p => p.Promocion)
            .Where(p => p.FkIdUsuario == idUsuario)
            .OrderByDescending(p => p.IdPedido)
            .ToListAsync(ct);
    }

    public async Task<Pedido?> GetDetalleUsuarioAsync(int idPedido, int idUsuario, CancellationToken ct = default)
    {
        return await _context.Pedidos
            .AsNoTracking()
            .Include(p => p.PedidosItem).ThenInclude(i => i.Producto)
            .Include(p => p.PedidosItem).ThenInclude(i => i.TamanoPizza)
            .Include(p => p.Direccion)
            .Include(p => p.Promocion)
            .FirstOrDefaultAsync(p => p.IdPedido == idPedido && p.FkIdUsuario == idUsuario, ct);
    }

    public async Task<Pedido?> GetByStripeCheckoutSessionIdAsync(string stripeCheckoutSessionId, CancellationToken ct = default)
    {
        return await _context.Pedidos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.StripeCheckoutSessionId == stripeCheckoutSessionId, ct);
    }

    public async Task<Pedido> CrearConItemsAsync(Pedido pedido, IReadOnlyList<PedidoItem> items, CancellationToken ct = default)
    {
        await using var tx = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync(ct);

            foreach (var item in items)
            {
                item.FkIdPedido = pedido.IdPedido;
                item.IdPedidoItem = 0;
            }

            _context.PedidosItem.AddRange(items);
            await _context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            pedido.PedidosItem = items.ToList();
            return pedido;
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }
    //Parte de pedididos para el dashboard del empleado.
    public async Task<IReadOnlyList<Pedido>> GetPedidosActivosDashboardAsync(CancellationToken ct)
    {
        return await _context.Pedidos
            .Include(p => p.Usuario)
            .Include(p => p.Direccion) // Para la dirección del cliente
            .Include(p => p.PedidosItem)
                .ThenInclude(pi => pi.Producto) // Para los nombres de los artículos
                                                // Filtramos para no traer los pedidos que ya se entregaron o cancelaron
            .Where(p => p.Estado != "Entregado" && p.Estado != "Cancelado")
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<bool> ActualizarEstadoAsync(int idPedido, string nuevoEstado, CancellationToken ct)
    {
        var pedido = await _context.Pedidos.FindAsync(new object[] { idPedido }, ct);

        if (pedido == null) return false;

        pedido.Estado = nuevoEstado;
        pedido.UpdatedAt = DateTime.UtcNow; // Es buena práctica actualizar la fecha de modificación

        return await _context.SaveChangesAsync(ct) > 0;
    }
}
