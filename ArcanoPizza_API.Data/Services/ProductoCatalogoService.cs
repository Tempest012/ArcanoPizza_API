using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.Data.IServices;
using ArcanoPizza_API.DTOs;
using Microsoft.Extensions.Logging;

namespace ArcanoPizza_API.Data.Services;

public class ProductoCatalogoService : IProductoCatalogoService
{
    private readonly IProductoRepository _productos;
    private readonly ILogger<ProductoCatalogoService> _log;

    public ProductoCatalogoService(IProductoRepository productos, ILogger<ProductoCatalogoService> log)
    {
        _productos = productos;
        _log = log;
    }

    public async Task<IReadOnlyList<ProductoDto>?> GetActivosAsync(CancellationToken ct)
    {
        try
        {
            var productos = await _productos.GetAllAsync();
            var activos = productos.Where(p => p.Activo);

            return activos.Select(p => new ProductoDto
            {
                Id = p.IdProducto,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Ingredientes = p.Ingredientes,
                ImagenURL = p.ImagenURL,
                Precio = p.PrecioBase,
                CategoriaNombre = p.Categoria?.Nombre ?? "Sin Categoría"
            }).ToList();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error crítico al obtener productos de la base de datos.");
            return null;
        }
    }
}
