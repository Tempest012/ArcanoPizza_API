using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.Data.IServices;
using ArcanoPizza_API.DTOs;
using ArcanoPizza_API.Model;

namespace ArcanoPizza_API.Data.Services;

public class DireccionService : IDireccionService
{
    private readonly IDireccionRepository _direcciones;

    public DireccionService(IDireccionRepository direcciones)
    {
        _direcciones = direcciones;
    }

    public async Task<IReadOnlyList<DireccionDto>> MisDireccionesAsync(int userId, CancellationToken ct)
    {
        var list = await _direcciones.GetByUsuarioAsync(userId, ct);
        return list.Select(d => new DireccionDto(d.IdDireccion, d.Calle, d.Colonia, d.CodigoPostal)).ToList();
    }

    public async Task<(DireccionDto? Creada, string? Error)> CrearAsync(int userId, DireccionCrearDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Calle)
            || string.IsNullOrWhiteSpace(dto.Colonia)
            || string.IsNullOrWhiteSpace(dto.CodigoPostal))
            return (null, "Calle, colonia y código postal son obligatorios.");

        var now = DateTime.UtcNow;
        var entity = new Direccion
        {
            Calle = dto.Calle.Trim(),
            Colonia = dto.Colonia.Trim(),
            CodigoPostal = dto.CodigoPostal.Trim(),
            FkIdUsuario = userId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var creada = await _direcciones.AddAsync(entity, ct);
        return (new DireccionDto(creada.IdDireccion, creada.Calle, creada.Colonia, creada.CodigoPostal), null);
    }
}
