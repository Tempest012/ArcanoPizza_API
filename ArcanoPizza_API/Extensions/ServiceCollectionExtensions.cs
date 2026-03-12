using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using ArcanoPizza_API.Middleware;

namespace ArcanoPizza_API.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configura las medidas de seguridad para cubrir OWASP Top 10.
    /// </summary>
    public static IServiceCollection AddSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        // A09: Security Logging - Exception handler con logging estructurado
        services.AddExceptionHandler<GlobalExceptionHandlerMiddleware>();

        // A02: Security Misconfiguration
        services.AddAntiforgery();
        services.AddHsts(options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromDays(365);
        });

        // A05, A07: Rate Limiting - Protección contra abusos e inyección por fuerza bruta
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

        return services;
    }

    /// <summary>
    /// Configura JWT para A01 Broken Access Control y A07 Authentication Failures.
    /// Solo se activa si Jwt:Key está configurado (User Secrets o variable JWT_KEY).
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"] ?? Environment.GetEnvironmentVariable("JWT_KEY");
        if (string.IsNullOrEmpty(jwtKey))
            return services; // Sin clave = sin auth hasta que lo configures

        var key = Encoding.UTF8.GetBytes(jwtKey);
        if (key.Length < 32)
            throw new InvalidOperationException("Jwt:Key debe tener al menos 32 caracteres para HS256.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"] ?? "ArcanoPizza",
                    ValidAudience = configuration["Jwt:Audience"] ?? "ArcanoPizza",
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        return services;
    }
}
