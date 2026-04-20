using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.Data.IServices;
using ArcanoPizza_API.DTOs;
using ArcanoPizza_API.Model;
using Microsoft.EntityFrameworkCore;

namespace ArcanoPizza_API.Data.Services;

public class AdminService : IAdminService
{
    private readonly IAdminRepository _repo;
    private readonly ArcanoPizzaDbContext _context;

    public AdminService(IAdminRepository repo, ArcanoPizzaDbContext context)
    {
        _repo = repo;
        _context = context;
    }

    public async Task<IEnumerable<UsuarioResponseDto>> GetUsuariosAsync()
    {
        var usuarios = await _repo.GetUsuariosAsync();
        return usuarios.Select(u => new UsuarioResponseDto
        {
            Id = u.IdUsuario,
            Nombre = u.NombreUsuario,
            Email = u.Correo,
            Telefono = u.Telefono,
            Tipo = u.Rol,
            Activo = u.Activo,
            FechaMiembro = u.CreatedAt
        });
    }

    public async Task<UsuarioResponseDto?> GetUsuarioAsync(int id)
    {
        var usuario = await _repo.GetUsuarioByIdAsync(id);
        if (usuario is null) return null;

        return new UsuarioResponseDto
        {
            Id = usuario.IdUsuario,
            Nombre = usuario.NombreUsuario,
            Email = usuario.Correo,
            Telefono = usuario.Telefono,
            Tipo = usuario.Rol,
            Activo = usuario.Activo,
            FechaMiembro = usuario.CreatedAt
        };
    }

    public async Task<(UsuarioResponseDto? Ok, string? Error)> CrearUsuarioAsync(UsuarioAdminDto dto)
    {
        var usuario = new Usuario
        {
            NombreUsuario = dto.Nombre,
            Correo = dto.Email,
            Telefono = dto.Telefono,
            Rol = dto.Tipo,
            Activo = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        try
        {
            var creado = await _repo.CrearUsuarioAsync(usuario);
            return (new UsuarioResponseDto
            {
                Id = creado.IdUsuario,
                Nombre = creado.NombreUsuario,
                Email = creado.Correo,
                Telefono = creado.Telefono,
                Tipo = creado.Rol,
                Activo = creado.Activo,
                FechaMiembro = creado.CreatedAt
            }, null);
        }
        catch (Exception ex)
        {
            return (null, ex.InnerException?.Message ?? ex.Message);
        }
    }

    public async Task<(bool Found, string? Error)> UpdateUsuarioAsync(int id, UsuarioUpdateDto dto)
    {
        var usuario = await _repo.GetUsuarioByIdAsync(id);
        if (usuario is null) return (false, null);

        usuario.NombreUsuario = dto.Nombre;
        usuario.Correo = dto.Email;
        usuario.Telefono = dto.Telefono;
        usuario.Rol = dto.Tipo;
        usuario.Activo = dto.Activo;
        usuario.UpdatedAt = DateTime.UtcNow;

        var actualizado = await _repo.ActualizarUsuarioAsync(usuario);
        if (actualizado is null) return (true, "No se pudo actualizar el usuario");

        return (true, null);
    }

    public async Task<bool> ToggleUsuarioAsync(int id)
    {
        var usuario = await _repo.GetUsuarioByIdAsync(id);
        if (usuario is null) return false;

        usuario.Activo = !usuario.Activo;
        usuario.UpdatedAt = DateTime.UtcNow;

        await _repo.ActualizarUsuarioAsync(usuario);
        return true;
    }

    public async Task<bool> DeleteUsuarioAsync(int id) =>
        await _repo.EliminarUsuarioAsync(id);

    public async Task<IEnumerable<ProductoResponseDto>> GetProductosAsync()
    {
        var productos = await _repo.GetProductosAsync();
        return productos.Select(p => new ProductoResponseDto
        {
            Id = p.IdProducto,
            Nombre = p.Nombre,
            Descripcion = p.Descripcion,
            ImagenURL = p.ImagenURL,
            PrecioBase = p.PrecioBase,
            Activo = p.Activo,
            IdCategoria = p.FkIdCategoria,
            Ingredientes = p.Ingredientes
        });
    }

    public async Task<ProductoResponseDto> CrearProductoAsync(ProductoAdminDto dto)
    {
        var producto = new Producto
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            ImagenURL = dto.ImagenURL,
            PrecioBase = dto.Precio,
            Activo = true,
            FkIdCategoria = dto.FkIdCategoria,
            Ingredientes = dto.Ingredientes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var creado = await _repo.CrearProductoAsync(producto);
        return new ProductoResponseDto
        {
            Id = creado.IdProducto,
            Nombre = creado.Nombre,
            Descripcion = creado.Descripcion,
            ImagenURL = creado.ImagenURL,
            PrecioBase = creado.PrecioBase,
            Activo = creado.Activo,
            IdCategoria = creado.FkIdCategoria,
            Ingredientes = creado.Ingredientes
        };
    }

    public async Task<(bool Found, string? Error)> UpdateProductoAsync(int id, ProductoUpdateDto dto)
    {
        var producto = await _repo.GetProductoByIdAsync(id);
        if (producto is null) return (false, null);

        producto.Nombre = dto.Nombre;
        producto.Descripcion = dto.Descripcion;
        producto.PrecioBase = dto.Precio;
        producto.Activo = dto.Activo;
        producto.FkIdCategoria = dto.FkIdCategoria;
        producto.Ingredientes = dto.Ingredientes;
        producto.ImagenURL = dto.ImagenURL;
        producto.UpdatedAt = DateTime.UtcNow;

        var actualizado = await _repo.ActualizarProductoAsync(producto);
        if (actualizado is null) return (true, "No se pudo actualizar el producto");

        return (true, null);
    }

    public async Task<bool> ToggleProductoAsync(int id)
    {
        var producto = await _repo.GetProductoByIdAsync(id);
        if (producto is null) return false;

        producto.Activo = !producto.Activo;
        producto.UpdatedAt = DateTime.UtcNow;

        await _repo.ActualizarProductoAsync(producto);
        return true;
    }

    public async Task<bool> DeleteProductoAsync(int id) =>
        await _repo.EliminarProductoAsync(id);

    public async Task<(DashboardDto? Ok, string? Error)> ObtenerMetricasDashboardAsync()
    {
        try
        {
            var horaLocalSonora = DateTime.UtcNow.AddHours(-7);
            var inicioHoyLocal = horaLocalSonora.Date;
            var finHoyLocal = inicioHoyLocal.AddDays(1).AddTicks(-1);

            var inicioHoyUTC = inicioHoyLocal.AddHours(7);
            var finHoyUTC = finHoyLocal.AddHours(7);

            var pedidosHoy = await _context.Pedidos
                .Include(p => p.PedidosItem)
                .ThenInclude(i => i.Producto)
                .Where(p => p.CreatedAt >= inicioHoyUTC && p.CreatedAt <= finHoyUTC)
                .ToListAsync();

            var dashboard = new DashboardDto
            {
                VentasHoy = pedidosHoy
                    .Where(p => p.Estado != "Cancelado" && p.Estado != "Rechazado")
                    .Sum(p => p.Total),

                PedidosActivos = pedidosHoy
                    .Count(p => p.Estado == "Pendiente" || p.Estado == "En Preparacion"),

                ProductosVendidos = pedidosHoy
                    .Where(p => p.Estado != "Cancelado")
                    .SelectMany(p => p.PedidosItem)
                    .GroupBy(i => i.Producto.Nombre)
                    .Select(g => new ProductoVendidoDto
                    {
                        Nombre = g.Key,
                        Vendidos = g.Sum(i => i.Cantidad),
                        Total = g.Sum(i => i.Cantidad * i.PrecioUnitario)
                    })
                    .OrderByDescending(p => p.Vendidos)
                    .Take(5)
                    .ToList(),

                PedidosPorHora = pedidosHoy
                    .GroupBy(p => p.CreatedAt.AddHours(-7).Hour)
                    .Select(g => new PedidosHoraDto
                    {
                        Hora = $"{g.Key:00}:00",
                        Cantidad = g.Count()
                    })
                    .OrderBy(p => p.Hora)
                    .ToList()
            };

            return (dashboard, null);
        }
        catch (Exception ex)
        {
            return (null, "Error interno al calcular métricas: " + ex.Message);
        }
    }
}
