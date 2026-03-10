namespace ArcanoPizza_API.Model;

public class Extra
{
    public int IdExtra { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<PedidoItemExtra> PedidosItemExtras { get; set; } = new List<PedidoItemExtra>();
}
