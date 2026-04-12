using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ArcanoPizza_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly IProductoRepository _productoRepository;
        private readonly ILogger<ProductosController> _logger;

        public ProductosController(IProductoRepository productoRepository, ILogger<ProductosController> logger)
        {
            _productoRepository = productoRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProductos()
        {
            try
            {
                var productos = await _productoRepository.GetAllAsync();

                // Mapeo manual siguiendo los nombres de tu DbContext
                var resultado = productos.Select(p => new ProductoDto
                {
                    Id = p.IdProducto,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Ingredientes = p.Ingredientes,
                    ImagenURL = p.ImagenURL,
                    Precio = p.PrecioBase,
                    CategoriaNombre = p.Categoria?.Nombre ?? "Sin Categoría"
                });

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                // Si la base de datos falla, esto guardará el error técnico en el servidor
                _logger.LogError(ex, "Error crítico al obtener productos de la base de datos.");
                return StatusCode(500, "Error interno al obtener los productos. Intente más tarde.");
            }
        }
    }
}