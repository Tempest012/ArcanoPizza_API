using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.Data.IServices;
using ArcanoPizza_API.DTOs;
using ArcanoPizza_API.Model;

namespace ArcanoPizza_API.Data.Services;

public class ExtraService : IExtraService
{
    private readonly IExtraRepository _extras;

    public ExtraService(IExtraRepository extras)
    {
        _extras = extras;
    }

    public async Task<IReadOnlyList<ExtraResponseDto>> GetAllAsync(CancellationToken ct)
    {
        var extras = await _extras.GetAllAsync(ct);
        return extras.Select(e => new ExtraResponseDto(e.IdExtra, e.Nombre, e.Precio, e.Activo)).ToList();
    }

    public async Task<ExtraResponseDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var extra = await _extras.GetByIdAsync(id, ct);
        return extra is null ? null : new ExtraResponseDto(extra.IdExtra, extra.Nombre, extra.Precio, extra.Activo);
    }

    public async Task<ExtraResponseDto> CreateAsync(ExtraCreateDto dto, CancellationToken ct)
    {
        var extra = new Extra
        {
            Nombre = dto.Nombre,
            Precio = dto.Precio,
            Activo = dto.Activo,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _extras.AddAsync(extra, ct);
        return new ExtraResponseDto(created.IdExtra, created.Nombre, created.Precio, created.Activo);
    }

    public async Task<(bool Found, string? Error)> UpdateAsync(int id, ExtraUpdateDto dto, CancellationToken ct)
    {
        var extra = await _extras.GetByIdAsync(id, ct);
        if (extra is null) return (false, null);

        if (dto.Nombre is not null) extra.Nombre = dto.Nombre;
        if (dto.Precio.HasValue) extra.Precio = dto.Precio.Value;
        if (dto.Activo.HasValue) extra.Activo = dto.Activo.Value;
        extra.UpdatedAt = DateTime.UtcNow;

        await _extras.UpdateAsync(extra, ct);
        return (true, null);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var extra = await _extras.GetByIdAsync(id, ct);
        if (extra is null) return false;

        await _extras.DeleteAsync(extra, ct);
        return true;
    }
}
