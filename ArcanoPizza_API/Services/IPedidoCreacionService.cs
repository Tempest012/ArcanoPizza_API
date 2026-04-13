using ArcanoPizza_API.DTOs;

namespace ArcanoPizza_API.Services;

public interface IPedidoCreacionService
{
    /// <summary>Crea pedido + ítems. <paramref name="stripeCheckoutSessionId"/> queda guardado para idempotencia con Stripe.</summary>
    Task<(PedidoDetalleDto? Detalle, string? Error)> CrearAsync(
        int userId,
        PedidoCrearDto dto,
        string? stripeCheckoutSessionId,
        CancellationToken ct = default);
}
