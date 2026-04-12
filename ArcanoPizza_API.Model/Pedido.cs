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

    /// <summary>Id de Checkout Session de Stripe; permite idempotencia al confirmar el pago.</summary>
    public string? StripeCheckoutSessionId { get; set; }

    /// <summary>Efectivo, TarjetaOnline, etc. Pedidos sin pago online suelen usar Efectivo.</summary>
    public string? MetodoPago { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Promocion? Promocion { get; set; }

    public Direccion? Direccion { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public Pago? Pago { get; set; }
    public ICollection<PedidoItem> PedidosItem { get; set; } = new List<PedidoItem>();
}
