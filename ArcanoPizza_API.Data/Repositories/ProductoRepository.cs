using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.Model;

namespace ArcanoPizza_API.Data.Repositories;

public class ProductoRepository : Repository<Producto>, IProductoRepository
{
    public ProductoRepository(ArcanoPizzaDbContext context) : base(context) { }
}
