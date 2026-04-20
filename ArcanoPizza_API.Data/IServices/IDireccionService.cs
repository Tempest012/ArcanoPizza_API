using ArcanoPizza_API.DTOs;

namespace ArcanoPizza_API.Data.IServices;

public interface IDireccionService
{
    Task<IReadOnlyList<DireccionDto>> MisDireccionesAsync(int userId, CancellationToken ct);
    Task<(DireccionDto? Creada, string? Error)> CrearAsync(int userId, DireccionCrearDto dto, CancellationToken ct);
}
