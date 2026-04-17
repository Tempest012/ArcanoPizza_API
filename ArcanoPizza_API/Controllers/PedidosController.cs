using ArcanoPizza_API.Data;
using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.DTOs;
using ArcanoPizza_API.Helpers;
using ArcanoPizza_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArcanoPizza_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]

public class PedidosController : ControllerBase
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IPedidoCreacionService _pedidoCreacion;

    // 👇 1. Declaramos la variable del contexto
    private readonly ArcanoPizzaDbContext _context;
    public PedidosController(IPedidoRepository pedidoRepository, IPedidoCreacionService pedidoCreacion, ArcanoPizzaDbContext context)
    {
        _pedidoRepository = pedidoRepository;
        _pedidoCreacion = pedidoCreacion;

        // 👇 3. Lo asignamos para que los métodos de abajo puedan usarlo
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PedidoListaDto>>> MisPedidos(CancellationToken ct)
    {
        var userId = User.GetUsuarioId();
        var lista = await _pedidoRepository.GetByUsuarioAsync(userId, ct);

        var dto = lista.Select(p => new PedidoListaDto(
                p.IdPedido,
                p.Estado,
                p.Total,
                p.TimeStamp ?? p.CreatedAt,
                p.TipoEntrega,
                p.Promocion?.Titulo,
                p.MetodoPago
            )).ToList();

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

        var location = $"/api/Pedidos/{detalle.IdPedido}";
        return Created(location, detalle);
    }

    // GET: api/Pedidos/dashboard
    [HttpGet("dashboard")]
    [Authorize(Roles = "Empleado,Administrador,Tecnico")]
    public async Task<ActionResult<IReadOnlyList<PedidoDashboardDto>>> GetDashboard(CancellationToken ct)
    {
        // 1. Obtenemos todos los pedidos activos (sin filtrar por UsuarioId)
        var pedidos = await _pedidoRepository.GetPedidosActivosDashboardAsync(ct);

        // 2. Mapeamos hacia el DTO que creamos para Angular
        var dto = pedidos.Select(p => new PedidoDashboardDto(
            Id: $"ORD-{p.IdPedido:D6}",
            Estado: p.Estado,
            TipoEntrega: p.TipoEntrega,
            Urgente: p.TipoEntrega.Equals("Express", StringComparison.OrdinalIgnoreCase), // Ejemplo de regla
            HoraRecibido: p.CreatedAt.ToString("HH:mm"),
            HoraEntrega: p.CreatedAt.AddMinutes(30).ToString("HH:mm"), // Estimado 30 mins
            Cliente: new ClienteResumenDto(
                Nombre: p.Usuario?.NombreUsuario ?? "Cliente Desconocido",
                Telefono: p.Usuario?.Telefono ?? "Sin teléfono",
                Direccion: p.Direccion is { } d
                    ? $"{d.Calle}, {d.Colonia}"
                    : "Recoger en local"
            ),
            Productos: p.PedidosItem.Select(pi => new ProductoResumenDto(
                Cantidad: pi.Cantidad,
                Nombre: pi.Producto?.Nombre ?? "(producto sin nombre)",
                Nota: null, // Si agregas notas especiales en el futuro, va aquí
                Ingredientes: pi.Producto?.Ingredientes ?? "Sin ingredientes"
            )).ToList(),
            Total: p.Total
        )).ToList();

        return Ok(dto);
    }

    // GET: api/Pedidos/mis-asignados
    [HttpGet("mis-asignados")]
    [Authorize(Roles = "Repartidor")]
    public async Task<ActionResult<IReadOnlyList<PedidoDashboardDto>>> MisAsignados(CancellationToken ct)
    {
        var userId = User.GetUsuarioId();
        var pedidos = await _pedidoRepository.GetAsignadosARepartidorAsync(userId, ct);

        var dto = pedidos.Select(p => new PedidoDashboardDto(
            Id: $"ORD-{p.IdPedido:D6}",
            Estado: p.Estado,
            TipoEntrega: p.TipoEntrega,
            Urgente: p.TipoEntrega.Equals("Express", StringComparison.OrdinalIgnoreCase),
            HoraRecibido: p.CreatedAt.ToString("HH:mm"),
            HoraEntrega: p.CreatedAt.AddMinutes(30).ToString("HH:mm"),
            Cliente: new ClienteResumenDto(
                Nombre: p.Usuario?.NombreUsuario ?? "Cliente Desconocido",
                Telefono: p.Usuario?.Telefono ?? "Sin teléfono",
                Direccion: p.Direccion is { } d
                    ? $"{d.Calle}, {d.Colonia}"
                    : "Recoger en local"
            ),
            Productos: p.PedidosItem.Select(pi => new ProductoResumenDto(
                Cantidad: pi.Cantidad,
                Nombre: pi.Producto?.Nombre ?? "(producto sin nombre)",
                Nota: null,
                Ingredientes: pi.Producto?.Ingredientes ?? "Sin ingredientes"
            )).ToList(),
            Total: p.Total
        )).ToList();

        return Ok(dto);
    }

    [HttpPatch("{id:int}/estado")]
    [Authorize(Roles = "Empleado,Administrador,Tecnico,Repartidor")]
    public async Task<IActionResult> ActualizarEstado(int id, [FromBody] string nuevoEstado, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(nuevoEstado)) return BadRequest(new { mensaje = "El estado no puede estar vacío." });

        // Si es repartidor, solo puede modificar pedidos asignados a él.
        var rol = User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
        if (string.Equals(rol, "Repartidor", StringComparison.OrdinalIgnoreCase))
        {
            var userId = User.GetUsuarioId();
            var pedido = await _context.Pedidos.AsNoTracking().FirstOrDefaultAsync(p => p.IdPedido == id, ct);
            if (pedido is null) return NotFound(new { mensaje = $"No se encontró el pedido con ID {id}" });
            if (pedido.FkIdRepartidor != userId) return Forbid();
        }

        var exito = await _pedidoRepository.ActualizarEstadoAsync(id, nuevoEstado, ct);

        if (!exito) return NotFound(new { mensaje = $"No se encontró el pedido con ID {id}" });

        // 👇 EL CAMBIO ESTÁ AQUÍ: Devolvemos un JSON real en lugar de vacío
        return Ok(new
        {
            mensaje = "Estado actualizado correctamente",
            estadoAsignado = nuevoEstado
        });
    }

    // GET: api/Pedidos/repartidores
    [HttpGet("repartidores")]
    public async Task<ActionResult<IEnumerable<EmpleadoResumenDto>>> GetRepartidores(CancellationToken ct)
    {
        // Buscamos solo a los usuarios activos con rol de Repartidor
        var empleados = await _context.Usuarios
            .Where(u => u.Rol == "Repartidor" && u.Activo)
            .Select(u => new EmpleadoResumenDto(u.IdUsuario, u.NombreUsuario))
            .ToListAsync(ct);

        return Ok(empleados);
    }

    // PATCH: api/Pedidos/{id}/asignar-repartidor
    [HttpPatch("{id:int}/asignar-repartidor")]
    [Authorize(Roles = "Empleado,Administrador,Tecnico")]
    public async Task<IActionResult> AsignarRepartidor(int id, [FromBody] AsignarRepartidorRequest request, CancellationToken ct)
    {
        var pedido = await _context.Pedidos.FindAsync(new object[] { id }, ct);

        if (pedido == null) return NotFound(new { mensaje = "Pedido no encontrado" });

        if (!string.Equals(pedido.TipoEntrega, "Reparto", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { mensaje = "Solo se puede asignar repartidor a pedidos con tipoEntrega='Reparto'." });

        // Actualizamos el estado y enlazamos la FK del repartidor
        pedido.Estado = "En Ruta";
        pedido.FkIdRepartidor = request.RepartidorId;

        await _context.SaveChangesAsync(ct);

        return Ok(new { mensaje = "Repartidor asignado correctamente" });
    }

}
