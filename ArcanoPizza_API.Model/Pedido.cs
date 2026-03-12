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
    public int FkIdDireccion { get; set; }
    public int FkIdUsuario { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Direccion Direccion { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
    public Pago? Pago { get; set; }
    public ICollection<PedidoItem> PedidosItem { get; set; } = new List<PedidoItem>();
}
