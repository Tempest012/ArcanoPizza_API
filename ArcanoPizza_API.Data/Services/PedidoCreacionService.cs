using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.Data.IServices;
using ArcanoPizza_API.DTOs;
using ArcanoPizza_API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ArcanoPizza_API.Data.Services;

public class PedidoCreacionService : IPedidoCreacionService
{
    private const decimal TasaIva = 0.16m;

    private readonly IPedidoRepository _pedidoRepository;
    private readonly IDireccionRepository _direccionRepository;
    private readonly IProductoRepository _productoRepository;
    private readonly IPromocionRepository _promocionRepository;
    private readonly ArcanoPizzaDbContext _db;
    private readonly ILogger<PedidoCreacionService> _log;

    public PedidoCreacionService(
        IPedidoRepository pedidoRepository,
        IDireccionRepository direccionRepository,
        IProductoRepository productoRepository,
        IPromocionRepository promocionRepository,
        ArcanoPizzaDbContext db,
        ILogger<PedidoCreacionService> log)
    {
        _pedidoRepository = pedidoRepository;
        _direccionRepository = direccionRepository;
        _productoRepository = productoRepository;
        _promocionRepository = promocionRepository;
        _db = db;
        _log = log;
    }

    public async Task<(PedidoDetalleDto? Detalle, string? Error)> CrearAsync(
        int userId,
        PedidoCrearDto dto,
        string? stripeCheckoutSessionId,
        CancellationToken ct = default)
    {
        if (dto.Lineas is null || dto.Lineas.Count == 0)
            return (null, "El pedido debe incluir al menos una línea.");

        if (string.IsNullOrWhiteSpace(dto.TipoEntrega))
            return (null, "TipoEntrega es obligatorio.");

        var esPagoStripe = !string.IsNullOrEmpty(stripeCheckoutSessionId);
        var (metodoNorm, errMetodo) = NormalizarMetodoPago(dto.MetodoPago, esPagoStripe);
        if (errMetodo is not null)
            return (null, errMetodo);

        var tipoNorm = dto.TipoEntrega.Trim();
        var recoger = string.Equals(tipoNorm, "Recoger", StringComparison.OrdinalIgnoreCase);

        Model.Direccion? direccion = null;
        if (!recoger)
        {
            if (dto.DireccionId is not { } dirObligatoria || dirObligatoria <= 0)
                return (null, "Debés elegir una dirección para reparto a domicilio.");

            direccion = await _direccionRepository.GetByIdForUsuarioAsync(dirObligatoria, userId, ct);
            if (direccion is null)
                return (null, "Dirección no válida o no pertenece al usuario.");
        }
        else if (dto.DireccionId is { } dirOpcional && dirOpcional > 0)
        {
            direccion = await _direccionRepository.GetByIdForUsuarioAsync(dirOpcional, userId, ct);
            if (direccion is null)
                return (null, "Dirección no válida o no pertenece al usuario.");
        }

        Promocion? promocion = null;
        if (dto.PromocionId is { } pid)
        {
            promocion = await _promocionRepository.GetByIdAsync(pid, ct);
            if (promocion is null || !EstaVigente(promocion, DateTime.UtcNow))
                return (null, "La promoción no existe o no está vigente.");
        }

        decimal subtotal = 0;
        var items = new List<PedidoItem>();

        foreach (var linea in dto.Lineas)
        {
            if (linea.Cantidad <= 0)
                return (null, "Las cantidades deben ser mayores a cero.");

            var producto = await _productoRepository.GetByIdAsync(linea.ProductoId);
            if (producto is null || !producto.Activo)
                return (null, $"Producto {linea.ProductoId} no disponible.");

            decimal precioTamano = 0;
            if (linea.TamanoPizzaId is { } tid)
            {
                var tam = await _db.TamanosPizza.AsNoTracking().FirstOrDefaultAsync(t => t.IdPizza == tid, ct);
                if (tam is null)
                    return (null, $"Tamaño de pizza {tid} no válido.");
                precioTamano = tam.ModificadorPrecio;
            }

            var precioUnitario = decimal.Round(producto.PrecioBase + precioTamano, 2, MidpointRounding.AwayFromZero);
            subtotal += precioUnitario * linea.Cantidad;

            items.Add(new PedidoItem
            {
                Cantidad = linea.Cantidad,
                PrecioUnitario = precioUnitario,
                FkIdProducto = producto.IdProducto,
                FkIdTamanoPizza = linea.TamanoPizzaId,
            });
        }

        subtotal = decimal.Round(subtotal, 2, MidpointRounding.AwayFromZero);

        var descuento = 0m;
        if (promocion is not null)
        {
            var ahorroBruto = promocion.PrecioOriginal - promocion.PrecioPromocional;
            var ahorro = ahorroBruto < 0 ? 0 : ahorroBruto;
            descuento = decimal.Round(Math.Min(subtotal, ahorro), 2, MidpointRounding.AwayFromZero);
        }

        var baseImponible = subtotal - descuento;
        if (baseImponible < 0)
            baseImponible = 0;

        var impuestos = decimal.Round(baseImponible * TasaIva, 2, MidpointRounding.AwayFromZero);
        var total = PedidoTotales.CalcularTotal(subtotal, descuento, impuestos);
        total = decimal.Round(total, 2, MidpointRounding.AwayFromZero);

        var now = DateTime.UtcNow;
        var pedido = new Pedido
        {
            Estado = "Pendiente",
            TipoEntrega = tipoNorm,
            Subtotal = subtotal,
            DescuentoTotal = descuento,
            Impuestos = impuestos,
            Total = total,
            FkIdUsuario = userId,
            FkIdDireccion = direccion?.IdDireccion,
            FkIdPromocion = promocion?.IdPromocion,
            TimeStamp = now,
            CreatedAt = now,
            UpdatedAt = now,
            StripeCheckoutSessionId = stripeCheckoutSessionId,
            MetodoPago = metodoNorm,
        };

        Pedido guardado;
        try
        {
            guardado = await _pedidoRepository.CrearConItemsAsync(pedido, items, ct);
        }
        catch (DbUpdateException ex)
        {
            _log.LogError(ex, "DbUpdateException al crear pedido para usuario {UserId}", userId);
            return (null, "No se pudo guardar el pedido. Revise que la base de datos esté actualizada (migraciones).");
        }

        var detalle = await _pedidoRepository.GetDetalleUsuarioAsync(guardado.IdPedido, userId, ct);
        if (detalle is null)
            return (null, "No se pudo leer el pedido creado.");

        return (MapDetalle(detalle), null);
    }

    private static bool EstaVigente(Promocion p, DateTime utcNow)
    {
        if (!p.Activo)
            return false;

        return p.TipoVigencia switch
        {
            TipoVigenciaPromocion.FechaHasta => p.FechaValidaHasta is not null
                && utcNow.Date <= p.FechaValidaHasta.Value.Date,
            TipoVigenciaPromocion.DiaSemanaRecurrente => p.DiaSemanaRecurrente is not null
                && (int)utcNow.DayOfWeek == p.DiaSemanaRecurrente.Value,
            _ => false,
        };
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

        var direccion = pedido.Direccion is { } d
            ? new DireccionDto(d.IdDireccion, d.Calle, d.Colonia, d.CodigoPostal)
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

    private static (string? Metodo, string? Error) NormalizarMetodoPago(string? desdeDto, bool esPagoStripeOnline)
    {
        if (esPagoStripeOnline)
            return ("TarjetaOnline", null);

        if (string.IsNullOrWhiteSpace(desdeDto))
            return (null, null);

        if (string.Equals(desdeDto.Trim(), "Efectivo", StringComparison.OrdinalIgnoreCase))
            return ("Efectivo", null);

        return (null, "Método de pago no válido. Usá «Efectivo» u omití el campo.");
    }
}

