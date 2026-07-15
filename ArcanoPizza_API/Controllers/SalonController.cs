using System.Security.Claims;
using ArcanoPizza_API.Data.IServices;
using ArcanoPizza_API.DTOs;
using ArcanoPizza_API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArcanoPizza_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalonController : ControllerBase
{
    private readonly ISalonService _salon;

    public SalonController(ISalonService salon)
    {
        _salon = salon;
    }

    [HttpPost("ordenes")]
    [Authorize(Roles = "Operador,Administrador")]
    public async Task<ActionResult<OrdenSalonDetalleDto>> CrearOrden(
        [FromBody] OrdenSalonCrearDto dto,
        CancellationToken ct)
    {
        var userId = User.GetUsuarioId();
        var (detalle, error, status) = await _salon.CrearOrdenAsync(userId, dto, ct);
        if (error is not null)
            return StatusCode(status, new { mensaje = error });
        return Created($"/api/Salon/ordenes/{detalle!.IdPedido}", detalle);
    }

    [HttpGet("ordenes")]
    [Authorize(Roles = "Administrador,Tecnico,Despachador,Operador")]
    public async Task<ActionResult<IReadOnlyList<OrdenSalonListaDto>>> ListarOrdenes(
        [FromQuery] string? estado,
        [FromQuery] int? mesaId,
        CancellationToken ct)
    {
        var userId = User.GetUsuarioId();
        var rol = GetRol();
        var lista = await _salon.ListarOrdenesAsync(rol, userId, estado, mesaId, ct);
        return Ok(lista);
    }

    [HttpGet("ordenes/{id:int}")]
    [Authorize(Roles = "Administrador,Tecnico,Despachador,Operador")]
    public async Task<ActionResult<OrdenSalonDetalleDto>> ObtenerOrden(int id, CancellationToken ct)
    {
        var userId = User.GetUsuarioId();
        var rol = GetRol();
        var (detalle, error, status) = await _salon.ObtenerOrdenAsync(id, rol, userId, ct);
        if (error is not null)
            return StatusCode(status, new { mensaje = error });
        return Ok(detalle);
    }

    [HttpPatch("ordenes/{id:int}/estado")]
    [Authorize(Roles = "Administrador,Tecnico,Despachador,Operador")]
    public async Task<IActionResult> ActualizarEstado(
        int id,
        [FromBody] OrdenSalonEstadoDto dto,
        CancellationToken ct)
    {
        var userId = User.GetUsuarioId();
        var rol = GetRol();
        var (ok, error, status) = await _salon.ActualizarEstadoAsync(id, dto.Estado, userId, rol, ct);
        if (!ok)
            return StatusCode(status, new { mensaje = error });
        return Ok(new { mensaje = "Estado actualizado", estadoAsignado = dto.Estado });
    }

    [HttpGet("watch/pendientes")]
    [Authorize(Roles = "Operador")]
    public async Task<ActionResult<IReadOnlyList<OrdenSalonListaDto>>> WatchPendientes(CancellationToken ct)
    {
        var userId = User.GetUsuarioId();
        return Ok(await _salon.WatchPendientesAsync(userId, ct));
    }

    [HttpPost("watch/ordenes/{id:int}/recoger")]
    [Authorize(Roles = "Operador")]
    public async Task<IActionResult> WatchRecoger(int id, CancellationToken ct)
    {
        var userId = User.GetUsuarioId();
        var (ok, error, status) = await _salon.WatchRecogerAsync(id, userId, ct);
        if (!ok)
            return StatusCode(status, new { mensaje = error });
        return Ok(new { mensaje = "Recolección confirmada", estadoAsignado = "Recogida" });
    }

    [HttpPost("watch/ordenes/{id:int}/entregar")]
    [Authorize(Roles = "Operador")]
    public async Task<IActionResult> WatchEntregar(int id, CancellationToken ct)
    {
        var userId = User.GetUsuarioId();
        var (ok, error, status) = await _salon.WatchEntregarAsync(id, userId, ct);
        if (!ok)
            return StatusCode(status, new { mensaje = error });
        return Ok(new { mensaje = "Entrega confirmada", estadoAsignado = "Entregado" });
    }

    [HttpGet("mesas/{id:int}/cuenta")]
    [Authorize(Roles = "Operador,Administrador,Despachador")]
    public async Task<ActionResult<CuentaMesaDto>> ObtenerCuenta(int id, CancellationToken ct)
    {
        var (cuenta, error, status) = await _salon.ObtenerCuentaAsync(id, ct);
        if (error is not null)
            return StatusCode(status, new { mensaje = error });
        return Ok(cuenta);
    }

    [HttpPost("mesas/{id:int}/cerrar")]
    [Authorize(Roles = "Operador,Administrador")]
    public async Task<IActionResult> CerrarMesa(int id, [FromBody] CerrarMesaDto dto, CancellationToken ct)
    {
        var userId = User.GetUsuarioId();
        var (ok, error, status) = await _salon.CerrarMesaAsync(id, userId, dto.MetodoPago, ct);
        if (!ok)
            return StatusCode(status, new { mensaje = error });
        return Ok(new { mensaje = "Mesa cerrada y cuenta cobrada", metodoPago = dto.MetodoPago });
    }

    [HttpGet("notificaciones")]
    [Authorize(Roles = "Operador,Administrador,Despachador")]
    public async Task<ActionResult<IReadOnlyList<NotificacionDto>>> Notificaciones(CancellationToken ct)
    {
        var userId = User.GetUsuarioId();
        return Ok(await _salon.NotificacionesAsync(userId, ct));
    }

    private string? GetRol() =>
        User.FindFirstValue(ClaimTypes.Role)
        ?? User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
}
