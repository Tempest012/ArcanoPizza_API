using ArcanoPizza_API.Data.Interface;

using ArcanoPizza_API.Data.IServices;
using ArcanoPizza_API.Data.Repositories;
using ArcanoPizza_API.Data.Services;

using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Configuration;

using Microsoft.Extensions.DependencyInjection;



namespace ArcanoPizza_API.Data;



public static class PostgresConfiguration

{

    public static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)

    {

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);



        var connectionString =

            Environment.GetEnvironmentVariable("DATABASE_URL")

            ?? configuration.GetConnectionString("DefaultConnection")

            ?? throw new InvalidOperationException(

                "No se encontró cadena de conexión. Configura 'ConnectionStrings:DefaultConnection' o la variable de entorno 'DATABASE_URL'.");



        var npgsqlConnectionString = PostgresConnectionString.Normalize(connectionString);



        services.AddDbContext<ArcanoPizzaDbContext>(options =>

            options.UseNpgsql(npgsqlConnectionString));



        services.AddScoped<IExtraRepository, ExtraRepository>();
        services.AddScoped<IAdminRepository, AdminRepository>();

        services.AddScoped<IPromocionRepository, PromocionRepository>();
        services.AddScoped<IProductoRepository, ProductoRepository>();

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddScoped<IPedidoRepository, PedidoRepository>();

        services.AddScoped<IDireccionRepository, DireccionRepository>();

        services.AddScoped<ICloudinarySignatureService, CloudinarySignatureService>();
        services.AddScoped<IPromocionService, PromocionService>();
        services.AddScoped<IAuditLogsQueryService, AuditLogsQueryService>();
        services.AddScoped<IPedidoCreacionService, PedidoCreacionService>();
        services.AddScoped<IStripeCheckoutService, StripeCheckoutService>();
        services.AddScoped<IPedidosService, PedidosService>();

        services.AddScoped<IExtraService, ExtraService>();
        services.AddScoped<IProductoCatalogoService, ProductoCatalogoService>();
        services.AddScoped<IDireccionService, DireccionService>();
        services.AddScoped<IAdminService, AdminService>();



        return services;

    }

}

