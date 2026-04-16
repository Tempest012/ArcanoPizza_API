using ArcanoPizza_API.Model;

namespace ArcanoPizza_API.Data.Interface;

public interface IPedidoRepository
{
    Task<IReadOnlyList<Pedido>> GetByUsuarioAsync(int idUsuario, CancellationToken ct = default);
    Task<Pedido?> GetDetalleUsuarioAsync(int idPedido, int idUsuario, CancellationToken ct = default);

    Task<Pedido?> GetByStripeCheckoutSessionIdAsync(string stripeCheckoutSessionId, CancellationToken ct = default);

    Task<Pedido> CrearConItemsAsync(Pedido pedido, IReadOnlyList<PedidoItem> items, CancellationToken ct = default);

    Task<IReadOnlyList<Pedido>> GetPedidosActivosDashboardAsync(CancellationToken ct);

    Task<IReadOnlyList<Pedido>> GetAsignadosARepartidorAsync(int repartidorId, CancellationToken ct);

    Task<bool> ActualizarEstadoAsync(int idPedido, string nuevoEstado, CancellationToken ct);
}
