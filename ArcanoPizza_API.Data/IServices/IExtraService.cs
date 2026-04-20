using ArcanoPizza_API.DTOs;

namespace ArcanoPizza_API.Data.IServices;

public interface IExtraService
{
    Task<IReadOnlyList<ExtraResponseDto>> GetAllAsync(CancellationToken ct);
    Task<ExtraResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<ExtraResponseDto> CreateAsync(ExtraCreateDto dto, CancellationToken ct);
    Task<(bool Found, string? Error)> UpdateAsync(int id, ExtraUpdateDto dto, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}
