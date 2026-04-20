using ArcanoPizza_API.DTOs;

namespace ArcanoPizza_API.Data.IServices;

public interface IPedidosService
{
    Task<IReadOnlyList<PedidoListaDto>> MisPedidosAsync(int userId, CancellationToken ct);
    Task<PedidoDetalleDto?> ObtenerDetalleAsync(int idPedido, int userId, CancellationToken ct);

    Task<(PedidoDetalleDto? Detalle, string? Error, int? HttpStatusCode)> CrearAsync(int userId, PedidoCrearDto dto, CancellationToken ct);

    Task<IReadOnlyList<PedidoDashboardDto>> DashboardAsync(CancellationToken ct);
    Task<IReadOnlyList<PedidoDashboardDto>> MisAsignadosAsync(int repartidorId, CancellationToken ct);

    Task<(bool Ok, string? Error, int? HttpStatusCode)> ActualizarEstadoAsync(
        int idPedido,
        string nuevoEstado,
        int? actingUserId,
        string? actingRole,
        CancellationToken ct);

    Task<IReadOnlyList<EmpleadoResumenDto>> GetRepartidoresAsync(CancellationToken ct);

    Task<(bool Ok, string? Error, int? HttpStatusCode)> AsignarRepartidorAsync(
        int idPedido,
        int repartidorId,
        CancellationToken ct);
}
