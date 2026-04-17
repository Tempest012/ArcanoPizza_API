using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using ArcanoPizza_API.Middleware;
using Microsoft.Extensions.Hosting;

namespace ArcanoPizza_API.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configura las medidas de seguridad para cubrir OWASP Top 10.
    /// </summary>
    /// <remarks>
    /// En Development no se registra rate limiting para no bloquear herramientas DAST (p. ej. OWASP ZAP)
    /// ni pruebas rápidas; en el resto de entornos se aplican los límites globales.
    /// </remarks>
    public static IServiceCollection AddSecurity(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // A09: Security Logging - Exception handler con logging estructurado
        services.AddExceptionHandler<GlobalExceptionHandlerMiddleware>();
        services.AddProblemDetails();

        // A02: Security Misconfiguration
        services.AddAntiforgery();
        services.AddHsts(options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromDays(365);
        });

        // A05, A07: Rate Limiting - Protección contra abusos e inyección por fuerza bruta (no en Development)
        if (!environment.IsDevelopment())
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = 429;
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(_ =>
                    RateLimitPartition.GetFixedWindowLimiter("global", _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            Window = TimeSpan.FromMinutes(1),
                            PermitLimit = 100
                        }));

                options.AddFixedWindowLimiter("api", config =>
                {
                    config.Window = TimeSpan.FromMinutes(1);
                    config.PermitLimit = 100;
                });

                options.AddFixedWindowLimiter("auth", config =>
                {
                    config.Window = TimeSpan.FromMinutes(1);
                    config.PermitLimit = 10;
                });
            });
        }

        return services;
    }
}
