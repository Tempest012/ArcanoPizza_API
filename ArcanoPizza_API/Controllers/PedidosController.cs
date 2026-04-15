using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.DTOs;
using ArcanoPizza_API.Helpers;
using ArcanoPizza_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArcanoPizza_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PedidosController : ControllerBase
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IPedidoCreacionService _pedidoCreacion;

    public PedidosController(IPedidoRepository pedidoRepository, IPedidoCreacionService pedidoCreacion)
    {
        _pedidoRepository = pedidoRepository;
        _pedidoCreacion = pedidoCreacion;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PedidoListaDto>>> MisPedidos(CancellationToken ct)
    {
        var userId = User.GetUsuarioId();
        var lista = await _pedidoRepository.GetByUsuarioAsync(userId, ct);

<<<<<<< HEAD
=======
        // 🔥 CORRECCIÓN: Le pasamos exactamente los 6 datos que espera el PedidoListaDto
>>>>>>> cesar/cliente
        var dto = lista.Select(p => new PedidoListaDto(
                p.IdPedido,
                p.Estado,
                p.Total,
<<<<<<< HEAD
                p.CreatedAt, // Esto caerá en 'Creado'
                p.TipoEntrega,
                p.Promocion?.Titulo,
                p.MetodoPago // Esto caerá en el nuevo 'MetodoPago' que agregamos
            )).ToList();
=======
                p.TimeStamp ?? DateTime.UtcNow,
                p.TipoEntrega,
                p.Promocion?.Titulo
            ))
            .ToList();
>>>>>>> cesar/cliente

        return Ok(dto);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PedidoDetalleDto>> Obtener(int id, CancellationToken ct)
    {
        var userId = User.GetUsuarioId();
        var pedido = await _pedidoRepository.GetDetalleUsuarioAsync(id, userId, ct);
        if (pedido is null)
            return NotFound();

        var lineas = pedido.PedidosItem.Select(i => new PedidoLineaDetalleDto(
                i.IdPedidoItem,
                i.Producto?.Nombre ?? "(producto)",
                i.Cantidad,
                i.PrecioUnitario,
                i.TamanoPizza?.Nombre))
            .ToList();

        var direccion = pedido.Direccion is { } d
            ? new DireccionDto(d.IdDireccion, d.Calle, d.Colonia, d.CodigoPostal)
            : new DireccionDto(0, "Recoger en local", "—", "—");

        return Ok(new PedidoDetalleDto(
            pedido.IdPedido,
            pedido.Estado,
            pedido.Subtotal,
            pedido.DescuentoTotal,
            pedido.Impuestos,
            pedido.Total,
            pedido.TipoEntrega,
            pedido.TimeStamp,
            pedido.FkIdPromocion,
            pedido.Promocion?.Titulo,
            pedido.MetodoPago,
            direccion,
            lineas));
    }

    [HttpPost]
    public async Task<ActionResult<PedidoDetalleDto>> Crear([FromBody] PedidoCrearDto? dto, CancellationToken ct)
    {
        if (dto is null)
            return BadRequest("Cuerpo de pedido inválido o vacío.");

        int userId;
        try
        {
            userId = User.GetUsuarioId();
        }
        catch (InvalidOperationException)
        {
            return Unauthorized("Token sin identificador de usuario válido.");
        }

        var (detalle, error) = await _pedidoCreacion.CrearAsync(userId, dto, stripeCheckoutSessionId: null, ct);
        if (error is not null)
            return BadRequest(error);
        if (detalle is null)
            return Problem("No se pudo crear el pedido.");

        // Evita InvalidOperationException si el enrutador no resuelve CreatedAtAction.
        var location = $"/api/Pedidos/{detalle.IdPedido}";
        return Created(location, detalle);
    }
}