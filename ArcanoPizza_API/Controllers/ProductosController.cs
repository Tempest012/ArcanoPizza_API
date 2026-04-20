using ArcanoPizza_API.Data.IServices;
using ArcanoPizza_API.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ArcanoPizza_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly IProductoCatalogoService _catalogo;

    public ProductosController(IProductoCatalogoService catalogo)
    {
        _catalogo = catalogo;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProductos(CancellationToken ct)
    {
        var resultado = await _catalogo.GetActivosAsync(ct);
        if (resultado is null)
            return StatusCode(500, "Error interno al obtener los productos. Intente más tarde.");

        return Ok(resultado);
    }
}
