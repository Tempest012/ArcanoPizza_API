namespace ArcanoPizza_API.DTOs;

public record PedidoLineaCrearDto(int ProductoId, int Cantidad, int? TamanoPizzaId = null);

public record PedidoCrearDto(
    IReadOnlyList<PedidoLineaCrearDto> Lineas,
    int? DireccionId,
    int? PromocionId,
    string TipoEntrega,
    string? MetodoPago = null);

public record PedidoListaDto(
    int IdPedido,
    string Estado,
    decimal Total,
    DateTime Creado,
    string TipoEntrega,
    string? PromocionTitulo,
    string? MetodoPago);

public record PedidoLineaDetalleDto(
    int IdPedidoItem,
    string ProductoNombre,
    int Cantidad,
    decimal PrecioUnitario,
    string? TamanoNombre);

public record PedidoDetalleDto(
    int IdPedido,
    string Estado,
    decimal Subtotal,
    decimal DescuentoTotal,
    decimal Impuestos,
    decimal Total,
    string TipoEntrega,
    DateTime? TimeStamp,
    int? PromocionId,
    string? PromocionTitulo,
    string? MetodoPago,
    DireccionDto Direccion,
    IReadOnlyList<PedidoLineaDetalleDto> Lineas);

public record DireccionDto(int IdDireccion, string Calle, string Colonia, string CodigoPostal);

public record DireccionCrearDto(string Calle, string Colonia, string CodigoPostal);

// 1. El DTO principal para la vista del empleado
public record PedidoDashboardDto(
    string Id,
    string Estado,
    bool Urgente,
    string HoraRecibido,
    string HoraEntrega,
    ClienteResumenDto Cliente,
    IReadOnlyList<ProductoResumenDto> Productos,
    decimal Total
);

// 2. DTOs de apoyo solo con la info necesaria para las tarjetas
public record ClienteResumenDto(
    string Nombre,
    string Telefono,
    string Direccion
);

public record ProductoResumenDto(
    int Cantidad,
    string Nombre,
    string? Nota
);