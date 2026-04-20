using ArcanoPizza_API.DTOs;

namespace ArcanoPizza_API.Data.IServices;

public interface IPedidoCreacionService
{
    /// <summary>Crea pedido + ítems. <paramref name="stripeCheckoutSessionId"/> queda guardado para idempotencia con Stripe.</summary>
    Task<(PedidoDetalleDto? Detalle, string? Error)> CrearAsync(
        int userId,
        PedidoCrearDto dto,
        string? stripeCheckoutSessionId,
        CancellationToken ct = default);
}
