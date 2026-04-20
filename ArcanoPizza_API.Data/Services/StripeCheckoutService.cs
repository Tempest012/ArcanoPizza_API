using System.Text.Json;
using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.Data.IServices;
using ArcanoPizza_API.DTOs;
using Stripe.Checkout;
using Microsoft.Extensions.Configuration;

namespace ArcanoPizza_API.Data.Services;

public class StripeCheckoutService : IStripeCheckoutService
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

    public StripeCheckoutService(
        IProductoRepository productoRepository,
        IDireccionRepository direccionRepository,
        IPedidoRepository pedidoRepository,
        IPedidoCreacionService pedidoCreacion,
        IConfiguration configuration)
    {
        _productoRepository = productoRepository;
        _direccionRepository = direccionRepository;
        _pedidoRepository = pedidoRepository;
        _pedidoCreacion = pedidoCreacion;
        _configuration = configuration;
    }

    public async Task<(string? Url, string? Error)> CrearSesionCheckoutAsync(
        int userId,
        CrearSesionStripeDto body,
        string baseUrl,
        CancellationToken ct)
    {
        var apiKey = _configuration["Stripe:SecretKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            return (null, "Falta configuración Stripe:SecretKey (User Secrets o appsettings.Development).");

        Stripe.StripeConfiguration.ApiKey = apiKey;

        if (body.Items is not { Count: > 0 })
            return (null, "El carrito está vacío.");

        var tipoEntrega = string.IsNullOrWhiteSpace(body.TipoEntrega) ? "Reparto" : body.TipoEntrega.Trim();
        var recoger = string.Equals(tipoEntrega, "Recoger", StringComparison.OrdinalIgnoreCase);

        Model.Direccion? direccion = null;
        if (!recoger)
        {
            direccion = await ResolverDireccionAsync(userId, body.DireccionId, ct);
            if (direccion is null)
                return (null, "Necesitás una dirección de entrega para reparto a domicilio.");
        }
        else if (body.DireccionId is { } did && did > 0)
        {
            direccion = await _direccionRepository.GetByIdForUsuarioAsync(did, userId, ct);
            if (direccion is null)
                return (null, "La dirección indicada no es válida.");
        }

        var lineItems = new List<SessionLineItemOptions>();
        var productosEnBaseDeDatos = await _productoRepository.GetAllAsync();

        foreach (var item in body.Items)
        {
            var productoReal = productosEnBaseDeDatos.FirstOrDefault(p => p.IdProducto == item.ProductoId);
            if (productoReal is null)
                return (null, $"Producto {item.ProductoId} no existe.");
            if (!productoReal.Activo)
                return (null, $"Producto {item.ProductoId} no disponible.");
            if (item.Cantidad <= 0)
                return (null, "Las cantidades deben ser mayores a cero.");

            decimal precioUsar = item.Precio > 0 ? item.Precio : productoReal.PrecioBase;
            decimal precioConIva = precioUsar * 1.16m;

            lineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)Math.Round(precioConIva * 100m),
                    Currency = "mxn",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = productoReal.Nombre,
                    },
                },
                Quantity = item.Cantidad,
            });
        }

        if (lineItems.Count == 0)
            return (null, "El carrito está vacío o los productos no son válidos.");

        var lineasPayload = body.Items
            .Select(i => new PedidoLineaCrearDto(i.ProductoId, i.Cantidad, i.TamanoPizzaId))
            .ToList();

        var lineasJson = JsonSerializer.Serialize(lineasPayload, JsonOpts);
        if (lineasJson.Length > 450)
            return (null, "El carrito es demasiado grande para completar el pago. Reduce ítems.");

        var promocionPart = body.PromocionId?.ToString() ?? "";

        baseUrl = (baseUrl ?? "").TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
            return (null, "No se pudo determinar baseUrl para el checkout.");

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

        return (session.Url, null);
    }

    public async Task<(PedidoDetalleDto? Detalle, string? Error, int? HttpStatusCode)> ConfirmarSesionAsync(
        string sessionId,
        CancellationToken ct)
    {
        var apiKey = _configuration["Stripe:SecretKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            return (null, "Falta configuración Stripe:SecretKey.", 500);

        if (string.IsNullOrWhiteSpace(sessionId))
            return (null, "sessionId es obligatorio.", 400);

        Stripe.StripeConfiguration.ApiKey = apiKey;

        var service = new SessionService();
        var session = await service.GetAsync(sessionId, cancellationToken: ct);

        if (!string.Equals(session.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
            return (null, "El pago no está completado.", 400);

        if (session.Metadata is null
            || !session.Metadata.TryGetValue("u", out var uMeta)
            || !int.TryParse(uMeta, out var userIdPedido))
        {
            return (null, "Sesión de pago sin datos de usuario.", 400);
        }

        var existente = await _pedidoRepository.GetByStripeCheckoutSessionIdAsync(session.Id, ct);
        if (existente is not null)
        {
            if (existente.FkIdUsuario != userIdPedido)
                return (null, "Sesión ya usada por otro usuario.", 409);

            var detalleExistente = await _pedidoRepository.GetDetalleUsuarioAsync(existente.IdPedido, userIdPedido, ct);
            if (detalleExistente is null)
                return (null, "Pedido existente no encontrado.", 404);

            return (MapDetalle(detalleExistente), null, null);
        }

        session.Metadata.TryGetValue("t", out var tMeta);
        var tipoEntrega = string.IsNullOrWhiteSpace(tMeta) ? "Reparto" : tMeta.Trim();

        _ = session.Metadata.TryGetValue("d", out var dMeta);
        var tieneD = int.TryParse(dMeta, out var dVal);
        int? direccionId = null;

        var recoger = string.Equals(tipoEntrega, "Recoger", StringComparison.OrdinalIgnoreCase);
        if (recoger)
        {
            if (tieneD && dVal > 0)
                direccionId = dVal;
        }
        else
        {
            if (!tieneD || dVal <= 0)
                return (null, "Sesión sin dirección de entrega.", 400);
            direccionId = dVal;
        }

        int? promocionId = null;
        if (session.Metadata.TryGetValue("p", out var pMeta) && int.TryParse(pMeta, out var pId))
            promocionId = pId;

        if (!session.Metadata.TryGetValue("l", out var lMeta))
            return (null, "Sesión sin líneas de pedido.", 400);

        var lineas = JsonSerializer.Deserialize<IReadOnlyList<PedidoLineaCrearDto>>(lMeta, JsonOpts);
        if (lineas is null || lineas.Count == 0)
            return (null, "No se pudieron leer las líneas del pedido.", 400);

        var crearDto = new PedidoCrearDto(lineas, direccionId, promocionId, tipoEntrega);

        var (detalle, error) = await _pedidoCreacion.CrearAsync(userIdPedido, crearDto, session.Id, ct);
        if (error is not null)
        {
            // Si Stripe ya cobró pero el pedido no se puede crear por datos actuales (p. ej. producto desactivado),
            // es mejor reportarlo como conflicto para que el frontend muestre un estado de atención/reintento.
            if (error.Contains("no disponible", StringComparison.OrdinalIgnoreCase))
                return (null, error, 409);
            return (null, error, 400);
        }
        if (detalle is null)
            return (null, "No se pudo crear el pedido.", 500);

        return (detalle, null, null);
    }

    private async Task<Model.Direccion?> ResolverDireccionAsync(int userId, int? direccionId, CancellationToken ct)
    {
        if (direccionId is { } did)
            return await _direccionRepository.GetByIdForUsuarioAsync(did, userId, ct);

        var list = await _direccionRepository.GetByUsuarioAsync(userId, ct);
        return list.FirstOrDefault();
    }

    private static PedidoDetalleDto MapDetalle(Model.Pedido pedido)
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

