using ArcanoPizza_API.DTOs;

namespace ArcanoPizza_API.Data.IServices;

public interface IPromocionService
{
    Task<IReadOnlyList<PromocionResponseDto>> GetActivasAsync(CancellationToken ct);
    Task<IReadOnlyList<PromocionResponseDto>> GetAllAdminAsync(CancellationToken ct);
    Task<PromocionResponseDto?> GetByIdActivaAsync(int id, CancellationToken ct);

    Task<(PromocionResponseDto? Created, string? Error)> CreateAsync(PromocionCreateDto dto, CancellationToken ct);
    Task<(bool Found, string? Error)> UpdateAsync(int id, PromocionUpdateDto dto, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}
