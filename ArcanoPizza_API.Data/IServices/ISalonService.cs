using ArcanoPizza_API.DTOs;

namespace ArcanoPizza_API.Data.IServices;

public interface ISalonService
{
    Task<(OrdenSalonDetalleDto? Detalle, string? Error, int Status)> CrearOrdenAsync(
        int operadorId,
        OrdenSalonCrearDto dto,
        CancellationToken ct = default);

    Task<IReadOnlyList<OrdenSalonListaDto>> ListarOrdenesAsync(
        string? rol,
        int userId,
        string? estado,
        int? mesaId,
        CancellationToken ct = default);

    Task<(OrdenSalonDetalleDto? Detalle, string? Error, int Status)> ObtenerOrdenAsync(
        int idPedido,
        string? rol,
        int userId,
        CancellationToken ct = default);

    Task<(bool Ok, string? Error, int Status)> ActualizarEstadoAsync(
        int idPedido,
        string nuevoEstado,
        int userId,
        string? rol,
        CancellationToken ct = default);

    Task<IReadOnlyList<OrdenSalonListaDto>> WatchPendientesAsync(int operadorId, CancellationToken ct = default);

    Task<(bool Ok, string? Error, int Status)> WatchRecogerAsync(int idPedido, int operadorId, CancellationToken ct = default);

    Task<(bool Ok, string? Error, int Status)> WatchEntregarAsync(int idPedido, int operadorId, CancellationToken ct = default);

    Task<(CuentaMesaDto? Cuenta, string? Error, int Status)> ObtenerCuentaAsync(int mesaId, CancellationToken ct = default);

    Task<(bool Ok, string? Error, int Status)> CerrarMesaAsync(
        int mesaId,
        int operadorId,
        string metodoPago,
        CancellationToken ct = default);

    Task<IReadOnlyList<NotificacionDto>> NotificacionesAsync(int userId, CancellationToken ct = default);
}
