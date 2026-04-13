namespace ArcanoPizza_API.Model;

public class PedidoItemExtra
{
    public int IdPedidoItemExtra { get; set; }
    public int FkIdPedidoItem { get; set; }
    public int FkIdExtra { get; set; }
    public decimal PrecioExtra { get; set; }

    public PedidoItem PedidoItem { get; set; } = null!;
    public Extra Extra { get; set; } = null!;
}
