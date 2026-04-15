using System.Text.Json;
using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.DTOs;
using ArcanoPizza_API.Helpers;
using ArcanoPizza_API.Model;
using ArcanoPizza_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace ArcanoPizza_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PagosController : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly IProductoRepository _productoRepository;
    private readonly IDireccionRepository _direccionRepository;
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IPedidoCreacionService _pedidoCreacion;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;

    public PagosController(
        IProductoRepository productoRepository,
        IDireccionRepository direccionRepository,
        IPedidoRepository pedidoRepository,
        IPedidoCreacionService pedidoCreacion,
        IConfiguration configuration,
        IWebHostEnvironment env)
    {
        _productoRepository = productoRepository;
        _direccionRepository = direccionRepository;
        _pedidoRepository = pedidoRepository;
        _pedidoCreacion = pedidoCreacion;
        _configuration = configuration;
        _env = env;
    }

    [HttpPost("crear-sesion")]
    public async Task<ActionResult> CrearSesionCheckout([FromBody] CrearSesionStripeDto body, CancellationToken ct)
    {
        var apiKey = _configuration["Stripe:SecretKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            return Problem("Falta configuración Stripe:SecretKey (User Secrets o appsettings.Development).");

        Stripe.StripeConfiguration.ApiKey = apiKey;

        if (body.Items is not { Count: > 0 })
            return BadRequest("El carrito está vacío.");

        var userId = User.GetUsuarioId();

        var tipoEntrega = string.IsNullOrWhiteSpace(body.TipoEntrega) ? "Reparto" : body.TipoEntrega.Trim();
        var recoger = TipoEntregaHelper.EsRecogerEnLocal(tipoEntrega);

        Model.Direccion? direccion = null;
        if (!recoger)
        {
            direccion = await ResolverDireccionAsync(userId, body.DireccionId, ct);
            if (direccion is null)
                return BadRequest("Necesitás una dirección de entrega para reparto a domicilio.");
        }
        else if (body.DireccionId is { } did && did > 0)
        {
            direccion = await _direccionRepository.GetByIdForUsuarioAsync(did, userId, ct);
            if (direccion is null)
                return BadRequest("La dirección indicada no es válida.");
        }

        var lineItems = new List<SessionLineItemOptions>();
        var productosEnBaseDeDatos = await _productoRepository.GetAllAsync();

        foreach (var item in body.Items)
        {
            var productoReal = productosEnBaseDeDatos.FirstOrDefault(p => p.IdProducto == item.ProductoId);
            if (productoReal is null) continue;

            // 🔥 Usamos el precio que manda Angular (que ya tiene el precio correcto del tamaño "Personal")
            decimal precioUsar = item.Precio > 0 ? item.Precio : productoReal.PrecioBase;

            // Le sumamos el 16% de IVA
            decimal precioConIva = precioUsar * 1.16m;

            lineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)Math.Round(precioConIva * 100m),
                    Currency = "mxn",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        // Le agregamos el nombre del producto normal
                        Name = productoReal.Nombre,
                    },
                },
                Quantity = item.Cantidad,
            });
        }

        if (lineItems.Count == 0)
            return BadRequest("El carrito está vacío o los productos no son válidos.");

        var lineasPayload = body.Items
            // 🔥 Antes decía null al final, ahora le pasamos el tamaño de la pizza
            .Select(i => new PedidoLineaCrearDto(i.ProductoId, i.Cantidad, i.TamanoPizzaId))
            .ToList();

        var lineasJson = JsonSerializer.Serialize(lineasPayload, JsonOpts);
        if (lineasJson.Length > 450)
            return BadRequest("El carrito es demasiado grande para completar el pago. Reduce ítems.");

        var promocionPart = body.PromocionId?.ToString() ?? "";

        var baseUrl = _configuration["Stripe:FrontendBaseUrl"]?.TrimEnd('/')
            ?? (_env.IsDevelopment() ? "http://localhost:4200" : Request.Scheme + "://" + Request.Host);

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = lineItems,
            Mode = "payment",
            SuccessUrl = $"{baseUrl}/pago-exito?session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl = $"{baseUrl}/pago-cancelado",
            Metadata = new Dictionary<string, string>
            {
                ["u"] = userId.ToString(),
                ["d"] = (direccion?.IdDireccion ?? 0).ToString(),
                ["t"] = tipoEntrega,
                ["p"] = promocionPart,
                ["l"] = lineasJson,
            },
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options, cancellationToken: ct);

        return Ok(new { url = session.Url });
    }

    /// <summary>Sin [Authorize]: el JWT puede expirar durante el checkout; la sesión de Stripe y los metadatos (usuario) validan el pedido.</summary>
    [HttpPost("confirmar-sesion")]
    [AllowAnonymous]
    public async Task<ActionResult<PedidoDetalleDto>> ConfirmarSesion(
        [FromBody] ConfirmarSesionStripeDto body,
        CancellationToken ct)
    {
        var apiKey = _configuration["Stripe:SecretKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            return Problem("Falta configuración Stripe:SecretKey.");

        if (string.IsNullOrWhiteSpace(body.SessionId))
            return BadRequest("sessionId es obligatorio.");

        Stripe.StripeConfiguration.ApiKey = apiKey;

        var service = new SessionService();
        var session = await service.GetAsync(body.SessionId, cancellationToken: ct);

        if (!string.Equals(session.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
            return BadRequest("El pago no está completado.");

        if (session.Metadata is null
            || !session.Metadata.TryGetValue("u", out var uMeta)
            || !int.TryParse(uMeta, out var userIdPedido))
            return BadRequest("Sesión de pago sin datos de usuario.");

        var existente = await _pedidoRepository.GetByStripeCheckoutSessionIdAsync(session.Id, ct);
        if (existente is not null)
        {
            if (existente.FkIdUsuario != userIdPedido)
                return Conflict();

            var detalleExistente = await _pedidoRepository.GetDetalleUsuarioAsync(existente.IdPedido, userIdPedido, ct);
            if (detalleExistente is null)
                return NotFound();
            return Ok(MapDetalle(detalleExistente));
        }

        session.Metadata.TryGetValue("t", out var tMeta);
        var tipoEntrega = string.IsNullOrWhiteSpace(tMeta) ? "Reparto" : tMeta.Trim();

        _ = session.Metadata.TryGetValue("d", out var dMeta);
        var tieneD = int.TryParse(dMeta, out var dVal);
        int? direccionId = null;
        if (TipoEntregaHelper.EsRecogerEnLocal(tipoEntrega))
        {
            if (tieneD && dVal > 0)
                direccionId = dVal;
        }
        else
        {
            if (!tieneD || dVal <= 0)
                return BadRequest("Sesión sin dirección de entrega.");
            direccionId = dVal;
        }

        int? promocionId = null;
        if (session.Metadata.TryGetValue("p", out var pMeta) && int.TryParse(pMeta, out var pId))
            promocionId = pId;

        if (!session.Metadata.TryGetValue("l", out var lMeta))
            return BadRequest("Sesión sin líneas de pedido.");

        var lineas = JsonSerializer.Deserialize<IReadOnlyList<PedidoLineaCrearDto>>(lMeta, JsonOpts);
        if (lineas is null || lineas.Count == 0)
            return BadRequest("No se pudieron leer las líneas del pedido.");

        var crearDto = new PedidoCrearDto(lineas, direccionId, promocionId, tipoEntrega);

        var (detalle, error) = await _pedidoCreacion.CrearAsync(userIdPedido, crearDto, session.Id, ct);
        if (error is not null)
            return BadRequest(error);
        if (detalle is null)
            return Problem("No se pudo crear el pedido.");

        return Ok(detalle);
    }

    private async Task<Model.Direccion?> ResolverDireccionAsync(int userId, int? direccionId, CancellationToken ct)
    {
        if (direccionId is { } did)
            return await _direccionRepository.GetByIdForUsuarioAsync(did, userId, ct);

        var list = await _direccionRepository.GetByUsuarioAsync(userId, ct);
        return list.FirstOrDefault();
    }

    private static PedidoDetalleDto MapDetalle(Pedido pedido)
    {
        var lineas = pedido.PedidosItem.Select(i => new PedidoLineaDetalleDto(
                i.IdPedidoItem,
                i.Producto?.Nombre ?? "(producto)",
                i.Cantidad,
                i.PrecioUnitario,
                i.TamanoPizza?.Nombre))
            .ToList();

        var direccion = pedido.Direccion is { } dDir
            ? new DireccionDto(dDir.IdDireccion, dDir.Calle, dDir.Colonia, dDir.CodigoPostal)
            : new DireccionDto(0, "Recoger en local", "—", "—");

        return new PedidoDetalleDto(
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
            lineas);
    }
}
