using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ArcanoPizza_API.Data;

/// <summary>
/// Solo para <c>dotnet ef migrations</c>. No se usa en runtime.
/// </summary>
public class ArcanoPizzaDbContextFactory : IDesignTimeDbContextFactory<ArcanoPizzaDbContext>
{
    public ArcanoPizzaDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ArcanoPizzaDbContext>();
        optionsBuilder.UseNpgsql("Host=127.0.0.1;Database=arcanopizza_design;Username=postgres;Password=postgres");
        return new ArcanoPizzaDbContext(optionsBuilder.Options);
    }
}
