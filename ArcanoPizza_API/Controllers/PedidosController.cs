using ArcanoPizza_API.Data;
using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.Data.IServices;
using ArcanoPizza_API.DTOs;
using ArcanoPizza_API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArcanoPizza_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]

public class PedidosController : ControllerBase
{
    private readonly IPedidosService _pedidos;

    public PedidosController(IPedidosService pedidos)
    {
        _pedidos = pedidos;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PedidoListaDto>>> MisPedidos(CancellationToken ct)
    {
        var userId = User.GetUsuarioId();
        var dto = await _pedidos.MisPedidosAsync(userId, ct);
        return Ok(dto);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PedidoDetalleDto>> Obtener(int id, CancellationToken ct)
    {
        var userId = User.GetUsuarioId();
        var dto = await _pedidos.ObtenerDetalleAsync(id, userId, ct);
        return dto is null ? NotFound() : Ok(dto);
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

        var (detalle, error, status) = await _pedidos.CrearAsync(userId, dto, ct);
        if (error is not null)
            return status switch
            {
                400 => BadRequest(error),
                401 => Unauthorized(error),
                _ => Problem(error),
            };
        if (detalle is null) return Problem("No se pudo crear el pedido.");

        var location = $"/api/Pedidos/{detalle.IdPedido}";
        return Created(location, detalle);
    }

    // GET: api/Pedidos/dashboard
    [HttpGet("dashboard")]
    [Authorize(Roles = "Empleado,Administrador,Tecnico")]
    public async Task<ActionResult<IReadOnlyList<PedidoDashboardDto>>> GetDashboard(CancellationToken ct)
    {
        var dto = await _pedidos.DashboardAsync(ct);
        return Ok(dto);
    }

    // GET: api/Pedidos/mis-asignados
    [HttpGet("mis-asignados")]
    [Authorize(Roles = "Repartidor")]
    public async Task<ActionResult<IReadOnlyList<PedidoDashboardDto>>> MisAsignados(CancellationToken ct)
    {
        var userId = User.GetUsuarioId();
        var dto = await _pedidos.MisAsignadosAsync(userId, ct);
        return Ok(dto);
    }

    [HttpPatch("{id:int}/estado")]
    [Authorize(Roles = "Empleado,Administrador,Tecnico,Repartidor")]
    public async Task<IActionResult> ActualizarEstado(int id, [FromBody] string nuevoEstado, CancellationToken ct)
    {
        int? actingUserId = null;
        try { actingUserId = User.GetUsuarioId(); } catch { /* ignore */ }
        var rol = User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

        var (ok, error, status) = await _pedidos.ActualizarEstadoAsync(id, nuevoEstado, actingUserId, rol, ct);
        if (!ok)
            return status switch
            {
                400 => BadRequest(new { mensaje = error }),
                401 => Unauthorized(new { mensaje = error }),
                403 => Forbid(),
                404 => NotFound(new { mensaje = error }),
                _ => Problem(error),
            };

        return Ok(new { mensaje = "Estado actualizado correctamente", estadoAsignado = nuevoEstado });
    }

    // GET: api/Pedidos/repartidores
    [HttpGet("repartidores")]
    public async Task<ActionResult<IEnumerable<EmpleadoResumenDto>>> GetRepartidores(CancellationToken ct)
    {
        var empleados = await _pedidos.GetRepartidoresAsync(ct);
        return Ok(empleados);
    }

    // PATCH: api/Pedidos/{id}/asignar-repartidor
    [HttpPatch("{id:int}/asignar-repartidor")]
    [Authorize(Roles = "Empleado,Administrador,Tecnico")]
    public async Task<IActionResult> AsignarRepartidor(int id, [FromBody] AsignarRepartidorRequest request, CancellationToken ct)
    {
        var (ok, error, status) = await _pedidos.AsignarRepartidorAsync(id, request.RepartidorId, ct);
        if (!ok)
            return status switch
            {
                400 => BadRequest(new { mensaje = error }),
                404 => NotFound(new { mensaje = error }),
                _ => Problem(error),
            };

        return Ok(new { mensaje = "Repartidor asignado correctamente" });
    }

}
