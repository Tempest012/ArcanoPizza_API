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
        var connectionString =
            Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "No se encontró cadena de conexión. Configura 'ConnectionStrings:DefaultConnection' o la variable de entorno 'DATABASE_URL'.");

        var npgsqlConnectionString = PostgresConnectionString.Normalize(connectionString);

        services.AddDbContext<ArcanoPizzaDbContext>(options =>
            options.UseNpgsql(npgsqlConnectionString));

        services.AddScoped<IProductoRepository, ProductoRepository>();

        return services;
    }
}
