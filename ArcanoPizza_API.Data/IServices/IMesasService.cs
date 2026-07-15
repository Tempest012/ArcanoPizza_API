using ArcanoPizza_API.DTOs;

namespace ArcanoPizza_API.Data.IServices;

public interface IMesasService
{
    Task<IReadOnlyList<MesaDto>> ListarAsync(CancellationToken ct = default);
    Task<MesaDto?> ObtenerAsync(int id, CancellationToken ct = default);
    Task<(MesaDto? Mesa, string? Error, int Status)> CrearAsync(MesaCrearDto dto, CancellationToken ct = default);
    Task<(bool Ok, string? Error, int Status)> CambiarEstadoAsync(int id, string estado, CancellationToken ct = default);
    Task<(bool Ok, string? Error, int Status)> EliminarAsync(int id, CancellationToken ct = default);
}
