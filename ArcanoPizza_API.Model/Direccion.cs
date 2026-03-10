namespace ArcanoPizza_API.Model;

public class Direccion
{
    public int IdDireccion { get; set; }
    public string Calle { get; set; } = string.Empty;
    public string Colonia { get; set; } = string.Empty;
    public string CodigoPostal { get; set; } = string.Empty;
    public int FkIdUsuario { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Usuario Usuario { get; set; } = null!;
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
