using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.DTOs;
using ArcanoPizza_API.Model;
using Microsoft.AspNetCore.Mvc;

namespace ArcanoPizza_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly IProductoRepository _productoRepository;

    public ProductosController(IProductoRepository productoRepository)
    {
        _productoRepository = productoRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductoResponseDto>>> GetAll(CancellationToken ct)
    {
        var productos = await _productoRepository.GetAllAsync(ct);
        var dtos = productos.Select(p => new ProductoResponseDto(
            p.IdProducto, p.Nombre, p.Descripcion, p.PrecioBase, p.Activo, p.FkIdCategoria));
        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductoResponseDto>> GetById(int id, CancellationToken ct)
    {
        var producto = await _productoRepository.GetByIdAsync(id, ct);
        if (producto is null)
            return NotFound();

        return Ok(new ProductoResponseDto(
            producto.IdProducto, producto.Nombre, producto.Descripcion,
            producto.PrecioBase, producto.Activo, producto.FkIdCategoria));
    }

    [HttpPost]
    public async Task<ActionResult<ProductoResponseDto>> Create([FromBody] ProductoCreateDto dto, CancellationToken ct)
    {
        var producto = new Producto
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            PrecioBase = dto.PrecioBase,
            Activo = dto.Activo,
            FkIdCategoria = dto.FkIdCategoria,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _productoRepository.AddAsync(producto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.IdProducto },
            new ProductoResponseDto(created.IdProducto, created.Nombre, created.Descripcion,
                created.PrecioBase, created.Activo, created.FkIdCategoria));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] ProductoUpdateDto dto, CancellationToken ct)
    {
        var producto = await _productoRepository.GetByIdAsync(id, ct);
        if (producto is null)
            return NotFound();

        if (dto.Nombre is not null) producto.Nombre = dto.Nombre;
        if (dto.Descripcion is not null) producto.Descripcion = dto.Descripcion;
        if (dto.PrecioBase.HasValue) producto.PrecioBase = dto.PrecioBase.Value;
        if (dto.Activo.HasValue) producto.Activo = dto.Activo.Value;
        if (dto.FkIdCategoria.HasValue) producto.FkIdCategoria = dto.FkIdCategoria.Value;
        producto.UpdatedAt = DateTime.UtcNow;

        await _productoRepository.UpdateAsync(producto, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id, CancellationToken ct)
    {
        var producto = await _productoRepository.GetByIdAsync(id, ct);
        if (producto is null)
            return NotFound();

        await _productoRepository.DeleteAsync(producto, ct);
        return NoContent();
    }
}
