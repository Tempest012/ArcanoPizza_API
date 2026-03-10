namespace ArcanoPizza_API.Model;

public class TamanoPizza
{
    public int IdPizza { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal ModificadorPrecio { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<PedidoItem> PedidosItem { get; set; } = new List<PedidoItem>();
}
