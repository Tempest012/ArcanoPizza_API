using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArcanoPizza_API.Data;

public static class SqlServerConfiguration
{
    public static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ArcanoPizzaDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IProductoRepository, ProductoRepository>();

        return services;
    }
}
