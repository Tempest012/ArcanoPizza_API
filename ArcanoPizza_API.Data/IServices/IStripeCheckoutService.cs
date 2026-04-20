using ArcanoPizza_API.DTOs;

namespace ArcanoPizza_API.Data.IServices;

public interface IStripeCheckoutService
{
    Task<(string? Url, string? Error)> CrearSesionCheckoutAsync(
        int userId,
        CrearSesionStripeDto body,
        string baseUrl,
        CancellationToken ct);

    Task<(PedidoDetalleDto? Detalle, string? Error, int? HttpStatusCode)> ConfirmarSesionAsync(
        string sessionId,
        CancellationToken ct);
}
