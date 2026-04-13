namespace ArcanoPizza_API.Model;

public class PedidoItem
{
    public int IdPedidoItem { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public int FkIdPedido { get; set; }
    public int FkIdProducto { get; set; }
    public int? FkIdTamanoPizza { get; set; }

    public Pedido Pedido { get; set; } = null!;
    public Producto Producto { get; set; } = null!;
    public TamanoPizza? TamanoPizza { get; set; }
    public ICollection<PedidoItemExtra> PedidosItemExtras { get; set; } = new List<PedidoItemExtra>();
}
