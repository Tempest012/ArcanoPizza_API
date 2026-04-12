using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ArcanoPizza_API.Data;

/// <summary>
/// Solo para <c>dotnet ef</c>. No se usa en runtime.
/// Usa las mismas fuentes que la API: <c>DATABASE_URL</c> o <c>ConnectionStrings__DefaultConnection</c>.
/// </summary>
public class ArcanoPizzaDbContextFactory : IDesignTimeDbContextFactory<ArcanoPizzaDbContext>
{
    public ArcanoPizzaDbContext CreateDbContext(string[] args)
    {
        var raw =
            Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new InvalidOperationException(
                "dotnet ef necesita una cadena de conexión. En PowerShell, por ejemplo:\n" +
                "  $env:DATABASE_URL = 'postgresql://postgres:TU_CLAVE@127.0.0.1:5432/nombre_bd'\n" +
                "o bien:\n" +
                "  $env:ConnectionStrings__DefaultConnection = 'Host=127.0.0.1;Database=nombre_bd;Username=postgres;Password=TU_CLAVE'\n" +
                "Luego vuelve a ejecutar dotnet ef database update ...");
        }

        var connectionString = PostgresConnectionString.Normalize(raw);

        var optionsBuilder = new DbContextOptionsBuilder<ArcanoPizzaDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new ArcanoPizzaDbContext(optionsBuilder.Options);
    }
}
