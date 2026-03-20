namespace ArcanoPizza_API.Model;

public class Producto
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    public string? Ingredientes { get; set; }

    public string? ImagenURL { get; set; }
    public decimal PrecioBase { get; set; }
    public bool Activo { get; set; } = true;
    public int FkIdCategoria { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public CategoriaProducto Categoria { get; set; } = null!;
    public ICollection<PedidoItem> PedidosItem { get; set; } = new List<PedidoItem>();
}
