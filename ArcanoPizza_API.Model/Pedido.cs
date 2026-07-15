namespace ArcanoPizza_API.Model;


public class Pedido
{
    public int IdPedido { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime? TimeStamp { get; set; }
    public string TipoEntrega { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal Impuestos { get; set; }

    /// <summary>Monto descontado del subtotal antes de impuestos (si aplica promoción al pedido). Ver <see cref="PedidoTotales.CalcularTotal"/>.</summary>
    public decimal DescuentoTotal { get; set; }

    public int? FkIdPromocion { get; set; }

    public int? FkIdDireccion { get; set; }
    public int FkIdUsuario { get; set; }

    /// <summary>Usuario con rol repartidor/empleado asignado a la entrega (opcional).</summary>
    public int? FkIdRepartidor { get; set; }

    /// <summary>Mesa de salón (pedidos TipoEntrega = Salon).</summary>
    public int? FkIdMesa { get; set; }

    /// <summary>Operador/mesero responsable de la entrega en salón.</summary>
    public int? FkIdOperador { get; set; }

    /// <summary>Id de Checkout Session de Stripe; permite idempotencia al confirmar el pago.</summary>
    public string? StripeCheckoutSessionId { get; set; }

    /// <summary>Efectivo, TarjetaOnline, etc. Pedidos sin pago online suelen usar Efectivo.</summary>
    public string? MetodoPago { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Promocion? Promocion { get; set; }

    public Direccion? Direccion { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public Usuario? Repartidor { get; set; }
    public Mesa? Mesa { get; set; }
    public Usuario? Operador { get; set; }
    public Pago? Pago { get; set; }
    public ICollection<PedidoItem> PedidosItem { get; set; } = new List<PedidoItem>();
    public ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();
}