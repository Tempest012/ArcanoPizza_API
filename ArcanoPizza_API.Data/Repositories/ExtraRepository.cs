using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.Model;

namespace ArcanoPizza_API.Data.Repositories;

public class ExtraRepository : Repository<Extra>, IExtraRepository
{
    public ExtraRepository(ArcanoPizzaDbContext context) : base(context) { }
}
