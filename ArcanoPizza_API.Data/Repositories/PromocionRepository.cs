using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.Model;

namespace ArcanoPizza_API.Data.Repositories;

public class PromocionRepository : Repository<Promocion>, IPromocionRepository
{
    public PromocionRepository(ArcanoPizzaDbContext context) : base(context) { }
}
