namespace ArcanoPizza_API.Model;

public class Mesa
{
    public int IdMesa { get; set; }
    public int Numero { get; set; }
    /// <summary>Disponible | Ocupada | Reservada</summary>
    public string Estado { get; set; } = "Disponible";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
