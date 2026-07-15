namespace ArcanoPizza_API.DTOs;

public record MesaDto(int IdMesa, int Numero, string Estado);

public record MesaCrearDto(int Numero, string? Estado = null);

public record MesaEstadoDto(string Estado);

public record OrdenSalonLineaCrearDto(int ProductoId, int Cantidad, int? TamanoPizzaId = null);

public record OrdenSalonCrearDto(int MesaId, IReadOnlyList<OrdenSalonLineaCrearDto> Lineas);

public record OrdenSalonLineaDto(
    int IdPedidoItem,
    string ProductoNombre,
    int Cantidad,
    decimal PrecioUnitario,
    string? TamanoNombre);

public record OrdenSalonListaDto(
    int IdPedido,
    string Estado,
    decimal Total,
    DateTime Creado,
    int? MesaId,
    int? NumeroMesa,
    int? OperadorId,
    string? OperadorNombre);

public record OrdenSalonDetalleDto(
    int IdPedido,
    string Estado,
    decimal Subtotal,
    decimal Impuestos,
    decimal Total,
    DateTime Creado,
    int? MesaId,
    int? NumeroMesa,
    int? OperadorId,
    string? OperadorNombre,
    string? MetodoPago,
    IReadOnlyList<OrdenSalonLineaDto> Lineas);

public record OrdenSalonEstadoDto(string Estado);

public record CuentaMesaLineaDto(
    int IdPedido,
    string ProductoNombre,
    int Cantidad,
    decimal PrecioUnitario,
    decimal Subtotal);

public record CuentaMesaDto(
    int MesaId,
    int NumeroMesa,
    string EstadoMesa,
    IReadOnlyList<int> PedidoIds,
    IReadOnlyList<CuentaMesaLineaDto> Lineas,
    decimal Total);

public record CerrarMesaDto(string MetodoPago);

public record NotificacionDto(
    int IdNotificacion,
    int IdPedido,
    string Mensaje,
    DateTime Fecha,
    bool Leida);
