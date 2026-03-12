namespace ArcanoPizza_API.Model;

public class CategoriaProducto
{
    public int IdCategoriasProductos { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
