using ArcanoPizza_API.Data.IServices;
using ArcanoPizza_API.DTOs;
using ArcanoPizza_API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArcanoPizza_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PagosController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;
    private readonly IStripeCheckoutService _stripe;

    public PagosController(
        IConfiguration configuration,
        IWebHostEnvironment env,
        IStripeCheckoutService stripe)
    {
        _configuration = configuration;
        _env = env;
        _stripe = stripe;
    }

    [HttpPost("crear-sesion")]
    public async Task<ActionResult> CrearSesionCheckout([FromBody] CrearSesionStripeDto body, CancellationToken ct)
    {
        var userId = User.GetUsuarioId();

        var baseUrl = _configuration["Stripe:FrontendBaseUrl"]?.TrimEnd('/')
            ?? (_env.IsDevelopment() ? "http://localhost:4200" : Request.Scheme + "://" + Request.Host);

        var result = await _stripe.CrearSesionCheckoutAsync(userId, body, baseUrl, ct);
        if (result.Error is not null) return BadRequest(result.Error);
        var url = result.Url;
        if (string.IsNullOrWhiteSpace(url)) return Problem("No se pudo crear la sesión de Stripe.");
        return Ok(new { url });
    }

    /// <summary>Sin [Authorize]: el JWT puede expirar durante el checkout; la sesión de Stripe y los metadatos (usuario) validan el pedido.</summary>
    [HttpPost("confirmar-sesion")]
    [AllowAnonymous]
    public async Task<ActionResult<PedidoDetalleDto>> ConfirmarSesion(
        [FromBody] ConfirmarSesionStripeDto body,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(body.SessionId))
            return BadRequest("sessionId es obligatorio.");

        var (detalle, error, status) = await _stripe.ConfirmarSesionAsync(body.SessionId, ct);
        if (error is not null)
            return status switch
            {
                400 => BadRequest(error),
                404 => NotFound(error),
                409 => Conflict(error),
                _ => Problem(error),
            };

        if (detalle is null) return Problem("No se pudo confirmar la sesión.");
        return Ok(detalle);
    }
}
