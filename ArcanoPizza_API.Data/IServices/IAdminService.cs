using ArcanoPizza_API.DTOs;

namespace ArcanoPizza_API.Data.IServices;

public interface IAdminService
{
    Task<IEnumerable<UsuarioResponseDto>> GetUsuariosAsync();
    Task<UsuarioResponseDto?> GetUsuarioAsync(int id);
    Task<(UsuarioResponseDto? Ok, string? Error)> CrearUsuarioAsync(UsuarioAdminDto dto);
    Task<(bool Found, string? Error)> UpdateUsuarioAsync(int id, UsuarioUpdateDto dto);
    Task<bool> ToggleUsuarioAsync(int id);
    Task<bool> DeleteUsuarioAsync(int id);

    Task<IEnumerable<ProductoResponseDto>> GetProductosAsync();
    Task<ProductoResponseDto> CrearProductoAsync(ProductoAdminDto dto);
    Task<(bool Found, string? Error)> UpdateProductoAsync(int id, ProductoUpdateDto dto);
    Task<bool> ToggleProductoAsync(int id);
    Task<bool> DeleteProductoAsync(int id);

    Task<(DashboardDto? Ok, string? Error)> ObtenerMetricasDashboardAsync();
}
