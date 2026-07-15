using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.Data.IServices;
using ArcanoPizza_API.DTOs;
using ArcanoPizza_API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ArcanoPizza_API.Data.Services;

public class SalonService : ISalonService
{
    private const decimal TasaIva = 0.16m;

    private readonly ArcanoPizzaDbContext _db;
    private readonly IMesaRepository _mesas;
    private readonly IProductoRepository _productos;
    private readonly ILogger<SalonService> _log;

    public SalonService(
        ArcanoPizzaDbContext db,
        IMesaRepository mesas,
        IProductoRepository productos,
        ILogger<SalonService> log)
    {
        _db = db;
        _mesas = mesas;
        _productos = productos;
        _log = log;
    }

    public async Task<(OrdenSalonDetalleDto? Detalle, string? Error, int Status)> CrearOrdenAsync(
        int operadorId,
        OrdenSalonCrearDto dto,
        CancellationToken ct = default)
    {
        if (dto.Lineas is null || dto.Lineas.Count == 0)
            return (null, "La orden debe incluir al menos una línea.", 400);

        var mesa = await _mesas.GetByIdAsync(dto.MesaId, ct);
        if (mesa is null)
            return (null, "Mesa no encontrada.", 404);

        if (string.Equals(mesa.Estado, SalonEstados.MesaOcupada, StringComparison.OrdinalIgnoreCase) == false
            && string.Equals(mesa.Estado, SalonEstados.MesaDisponible, StringComparison.OrdinalIgnoreCase) == false
            && string.Equals(mesa.Estado, SalonEstados.MesaReservada, StringComparison.OrdinalIgnoreCase) == false)
            return (null, "Estado de mesa no válido para ordenar.", 400);

        decimal subtotal = 0;
        var items = new List<PedidoItem>();

        foreach (var linea in dto.Lineas)
        {
            if (linea.Cantidad <= 0)
                return (null, "Las cantidades deben ser mayores a cero.", 400);

            var producto = await _productos.GetByIdAsync(linea.ProductoId);
            if (producto is null || !producto.Activo)
                return (null, $"Producto {linea.ProductoId} no disponible.", 400);

            decimal precioTamano = 0;
            if (linea.TamanoPizzaId is { } tid)
            {
                var tam = await _db.TamanosPizza.AsNoTracking().FirstOrDefaultAsync(t => t.IdPizza == tid, ct);
                if (tam is null)
                    return (null, $"Tamaño de pizza {tid} no válido.", 400);
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
        var impuestos = decimal.Round(subtotal * TasaIva, 2, MidpointRounding.AwayFromZero);
        var total = PedidoTotales.CalcularTotal(subtotal, 0, impuestos);
        var now = DateTime.UtcNow;

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var pedido = new Pedido
            {
                Estado = SalonEstados.Pendiente,
                TipoEntrega = SalonEstados.TipoEntregaSalon,
                Subtotal = subtotal,
                DescuentoTotal = 0,
                Impuestos = impuestos,
                Total = total,
                FkIdUsuario = operadorId,
                FkIdOperador = operadorId,
                FkIdMesa = mesa.IdMesa,
                TimeStamp = now,
                CreatedAt = now,
                UpdatedAt = now,
            };

            _db.Pedidos.Add(pedido);
            await _db.SaveChangesAsync(ct);

            foreach (var item in items)
            {
                item.FkIdPedido = pedido.IdPedido;
                item.IdPedidoItem = 0;
            }

            _db.PedidosItem.AddRange(items);

            if (!string.Equals(mesa.Estado, SalonEstados.MesaOcupada, StringComparison.OrdinalIgnoreCase))
            {
                mesa.Estado = SalonEstados.MesaOcupada;
                mesa.UpdatedAt = now;
            }

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            var detalle = await CargarDetalleAsync(pedido.IdPedido, ct);
            return (detalle, null, 201);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            _log.LogError(ex, "Error al crear orden de salón mesa {MesaId}", dto.MesaId);
            return (null, "No se pudo crear la orden de salón.", 500);
        }
    }

    public async Task<IReadOnlyList<OrdenSalonListaDto>> ListarOrdenesAsync(
        string? rol,
        int userId,
        string? estado,
        int? mesaId,
        CancellationToken ct = default)
    {
        var q = _db.Pedidos
            .AsNoTracking()
            .Include(p => p.Mesa)
            .Include(p => p.Operador)
            .Where(p => p.TipoEntrega == SalonEstados.TipoEntregaSalon);

        if (string.Equals(rol, SalonEstados.RolOperador, StringComparison.OrdinalIgnoreCase)
            && !SalonEstados.EsDespachadorOAdmin(rol))
        {
            q = q.Where(p => p.FkIdOperador == userId);
        }

        if (!string.IsNullOrWhiteSpace(estado))
            q = q.Where(p => p.Estado == estado.Trim());

        if (mesaId is { } mid && mid > 0)
            q = q.Where(p => p.FkIdMesa == mid);

        var lista = await q.OrderByDescending(p => p.CreatedAt).ToListAsync(ct);
        return lista.Select(MapLista).ToList();
    }

    public async Task<(OrdenSalonDetalleDto? Detalle, string? Error, int Status)> ObtenerOrdenAsync(
        int idPedido,
        string? rol,
        int userId,
        CancellationToken ct = default)
    {
        var pedido = await _db.Pedidos.AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdPedido == idPedido && p.TipoEntrega == SalonEstados.TipoEntregaSalon, ct);
        if (pedido is null)
            return (null, "Orden no encontrada.", 404);

        if (string.Equals(rol, SalonEstados.RolOperador, StringComparison.OrdinalIgnoreCase)
            && !SalonEstados.EsDespachadorOAdmin(rol)
            && pedido.FkIdOperador != userId)
            return (null, "No autorizado para ver esta orden.", 403);

        var detalle = await CargarDetalleAsync(idPedido, ct);
        return detalle is null ? (null, "Orden no encontrada.", 404) : (detalle, null, 200);
    }

    public async Task<(bool Ok, string? Error, int Status)> ActualizarEstadoAsync(
        int idPedido,
        string nuevoEstado,
        int userId,
        string? rol,
        CancellationToken ct = default)
    {
        var pedido = await _db.Pedidos
            .Include(p => p.Mesa)
            .FirstOrDefaultAsync(p => p.IdPedido == idPedido && p.TipoEntrega == SalonEstados.TipoEntregaSalon, ct);
        if (pedido is null)
            return (false, "Orden no encontrada.", 404);

        var esOperador = pedido.FkIdOperador == userId;
        var (ok, error) = SalonEstados.ValidarTransicion(pedido.Estado, nuevoEstado, rol, esOperador);
        if (!ok)
            return (false, error, string.Equals(error, "Solo el Operador asignado puede confirmar recolección o entrega.", StringComparison.Ordinal)
                || string.Equals(error, "Solo Despachador o Administrador pueden cancelar.", StringComparison.Ordinal)
                || (error?.Contains("Solo Despachador") == true)
                ? 403
                : 400);

        var destino = nuevoEstado.Trim();
        if (string.Equals(destino, SalonEstados.Cancelado, StringComparison.OrdinalIgnoreCase))
            destino = SalonEstados.Cancelado;
        else if (string.Equals(destino, SalonEstados.EnPreparacion, StringComparison.OrdinalIgnoreCase))
            destino = SalonEstados.EnPreparacion;
        else if (string.Equals(destino, SalonEstados.Listo, StringComparison.OrdinalIgnoreCase))
            destino = SalonEstados.Listo;
        else if (string.Equals(destino, SalonEstados.Recogida, StringComparison.OrdinalIgnoreCase))
            destino = SalonEstados.Recogida;
        else if (string.Equals(destino, SalonEstados.Entregado, StringComparison.OrdinalIgnoreCase))
            destino = SalonEstados.Entregado;

        pedido.Estado = destino;
        pedido.UpdatedAt = DateTime.UtcNow;

        if (string.Equals(destino, SalonEstados.Listo, StringComparison.OrdinalIgnoreCase)
            && pedido.FkIdOperador is { } opId)
        {
            var mesaTxt = pedido.Mesa?.Numero.ToString() ?? "?";
            _db.Notificaciones.Add(new Notificacion
            {
                FkIdUsuario = opId,
                FkIdPedido = pedido.IdPedido,
                Mensaje = $"Orden #{pedido.IdPedido} lista para mesa {mesaTxt}",
                Fecha = DateTime.UtcNow,
                Leida = false,
            });
        }

        if (string.Equals(destino, SalonEstados.Cancelado, StringComparison.OrdinalIgnoreCase)
            && pedido.FkIdMesa is { } mesaId)
        {
            await LiberarMesaSiSinActivasAsync(mesaId, pedido.IdPedido, ct);
        }

        await _db.SaveChangesAsync(ct);
        return (true, null, 200);
    }

    public async Task<IReadOnlyList<OrdenSalonListaDto>> WatchPendientesAsync(int operadorId, CancellationToken ct = default)
    {
        var lista = await _db.Pedidos
            .AsNoTracking()
            .Include(p => p.Mesa)
            .Include(p => p.Operador)
            .Where(p => p.TipoEntrega == SalonEstados.TipoEntregaSalon
                        && p.FkIdOperador == operadorId
                        && (p.Estado == SalonEstados.Listo || p.Estado == SalonEstados.Recogida))
            .OrderBy(p => p.UpdatedAt)
            .ToListAsync(ct);

        return lista.Select(MapLista).ToList();
    }

    public Task<(bool Ok, string? Error, int Status)> WatchRecogerAsync(int idPedido, int operadorId, CancellationToken ct = default) =>
        ActualizarEstadoAsync(idPedido, SalonEstados.Recogida, operadorId, SalonEstados.RolOperador, ct);

    public Task<(bool Ok, string? Error, int Status)> WatchEntregarAsync(int idPedido, int operadorId, CancellationToken ct = default) =>
        ActualizarEstadoAsync(idPedido, SalonEstados.Entregado, operadorId, SalonEstados.RolOperador, ct);

    public async Task<(CuentaMesaDto? Cuenta, string? Error, int Status)> ObtenerCuentaAsync(int mesaId, CancellationToken ct = default)
    {
        var mesa = await _mesas.GetByIdAsync(mesaId, ct);
        if (mesa is null)
            return (null, "Mesa no encontrada.", 404);

        var pedidos = await _db.Pedidos
            .AsNoTracking()
            .Include(p => p.PedidosItem).ThenInclude(i => i.Producto)
            .Where(p => p.FkIdMesa == mesaId
                        && p.TipoEntrega == SalonEstados.TipoEntregaSalon
                        && p.Estado == SalonEstados.Entregado)
            .ToListAsync(ct);

        var lineas = new List<CuentaMesaLineaDto>();
        foreach (var p in pedidos)
        {
            foreach (var i in p.PedidosItem)
            {
                lineas.Add(new CuentaMesaLineaDto(
                    p.IdPedido,
                    i.Producto?.Nombre ?? "(producto)",
                    i.Cantidad,
                    i.PrecioUnitario,
                    decimal.Round(i.Cantidad * i.PrecioUnitario, 2, MidpointRounding.AwayFromZero)));
            }
        }

        var total = pedidos.Sum(p => p.Total);
        return (new CuentaMesaDto(
            mesa.IdMesa,
            mesa.Numero,
            mesa.Estado,
            pedidos.Select(p => p.IdPedido).ToList(),
            lineas,
            total), null, 200);
    }

    public async Task<(bool Ok, string? Error, int Status)> CerrarMesaAsync(
        int mesaId,
        int operadorId,
        string metodoPago,
        CancellationToken ct = default)
    {
        var metodo = NormalizarMetodoPagoSalon(metodoPago);
        if (metodo is null)
            return (false, "Método de pago inválido. Use Efectivo o Tarjeta.", 400);

        var mesa = await _mesas.GetByIdAsync(mesaId, ct);
        if (mesa is null)
            return (false, "Mesa no encontrada.", 404);

        var pedidos = await _db.Pedidos
            .Include(p => p.Pago)
            .Where(p => p.FkIdMesa == mesaId
                        && p.TipoEntrega == SalonEstados.TipoEntregaSalon
                        && p.Estado == SalonEstados.Entregado)
            .ToListAsync(ct);

        if (pedidos.Count == 0)
            return (false, "No hay órdenes entregadas pendientes de cobro en esta mesa.", 400);

        var now = DateTime.UtcNow;
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            foreach (var pedido in pedidos)
            {
                pedido.Estado = SalonEstados.Pagado;
                pedido.MetodoPago = metodo;
                pedido.UpdatedAt = now;

                if (pedido.Pago is null)
                {
                    _db.Pagos.Add(new Pago
                    {
                        Proveedor = "Salon",
                        ProveedorPagoId = null,
                        Monto = pedido.Total,
                        Estado = "Completado",
                        MetodoPago = metodo,
                        TimeStamp = now,
                        FkIdPedido = pedido.IdPedido,
                    });
                }
            }

            mesa.Estado = SalonEstados.MesaDisponible;
            mesa.UpdatedAt = now;

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return (true, null, 200);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            _log.LogError(ex, "Error al cerrar mesa {MesaId}", mesaId);
            return (false, "No se pudo cerrar la mesa.", 500);
        }
    }

    public async Task<IReadOnlyList<NotificacionDto>> NotificacionesAsync(int userId, CancellationToken ct = default)
    {
        var lista = await _db.Notificaciones
            .AsNoTracking()
            .Where(n => n.FkIdUsuario == userId)
            .OrderByDescending(n => n.Fecha)
            .Take(50)
            .ToListAsync(ct);

        return lista.Select(n => new NotificacionDto(
            n.IdNotificacion,
            n.FkIdPedido,
            n.Mensaje,
            n.Fecha,
            n.Leida)).ToList();
    }

    private async Task LiberarMesaSiSinActivasAsync(int mesaId, int pedidoExcluidoId, CancellationToken ct)
    {
        var hayOtras = await _db.Pedidos.AnyAsync(
            p => p.FkIdMesa == mesaId
                 && p.IdPedido != pedidoExcluidoId
                 && p.TipoEntrega == SalonEstados.TipoEntregaSalon
                 && p.Estado != SalonEstados.Pagado
                 && p.Estado != SalonEstados.Cancelado,
            ct);

        if (hayOtras) return;

        var mesa = await _db.Mesas.FirstOrDefaultAsync(m => m.IdMesa == mesaId, ct);
        if (mesa is null) return;
        mesa.Estado = SalonEstados.MesaDisponible;
        mesa.UpdatedAt = DateTime.UtcNow;
    }

    private async Task<OrdenSalonDetalleDto?> CargarDetalleAsync(int idPedido, CancellationToken ct)
    {
        var pedido = await _db.Pedidos
            .AsNoTracking()
            .Include(p => p.Mesa)
            .Include(p => p.Operador)
            .Include(p => p.PedidosItem).ThenInclude(i => i.Producto)
            .Include(p => p.PedidosItem).ThenInclude(i => i.TamanoPizza)
            .FirstOrDefaultAsync(p => p.IdPedido == idPedido, ct);

        return pedido is null ? null : MapDetalle(pedido);
    }

    private static OrdenSalonListaDto MapLista(Pedido p) => new(
        p.IdPedido,
        p.Estado,
        p.Total,
        p.CreatedAt,
        p.FkIdMesa,
        p.Mesa?.Numero,
        p.FkIdOperador,
        p.Operador?.NombreUsuario);

    private static OrdenSalonDetalleDto MapDetalle(Pedido p) => new(
        p.IdPedido,
        p.Estado,
        p.Subtotal,
        p.Impuestos,
        p.Total,
        p.CreatedAt,
        p.FkIdMesa,
        p.Mesa?.Numero,
        p.FkIdOperador,
        p.Operador?.NombreUsuario,
        p.MetodoPago,
        p.PedidosItem.Select(i => new OrdenSalonLineaDto(
            i.IdPedidoItem,
            i.Producto?.Nombre ?? "(producto)",
            i.Cantidad,
            i.PrecioUnitario,
            i.TamanoPizza?.Nombre)).ToList());

    private static string? NormalizarMetodoPagoSalon(string? metodo)
    {
        if (string.IsNullOrWhiteSpace(metodo)) return null;
        if (string.Equals(metodo.Trim(), "Efectivo", StringComparison.OrdinalIgnoreCase)) return "Efectivo";
        if (string.Equals(metodo.Trim(), "Tarjeta", StringComparison.OrdinalIgnoreCase)) return "Tarjeta";
        return null;
    }
}
