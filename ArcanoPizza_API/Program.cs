using ArcanoPizza_API.Data;
using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.Data.Repositories;
using ArcanoPizza_API.Extensions;
using ArcanoPizza_API.Middleware;
using ArcanoPizza_API.Model;
using ArcanoPizza_API.Options;
using ArcanoPizza_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. REGISTRO DE SERVICIOS (Configuración)
// ==========================================

builder.Services.AddData(builder.Configuration);
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateOnStart();

builder.Services.AddSingleton<IValidateOptions<JwtOptions>, JwtOptionsValidator>();
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Autenticación JWT (OWASP: Gestión de Sesiones Seguras)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((jwtBearerOptions, jwtOpt) =>
    {
        var jwt = jwtOpt.Value;
        jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configuración de CORS
var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod();
        if (corsOrigins.Length > 0)
            policy.WithOrigins(corsOrigins);
        else
            policy.SetIsOriginAllowed(_ => true); // Solo para desarrollo
    });
});

// Tus servicios de seguridad base
builder.Services.AddSecurity(builder.Configuration);

var app = builder.Build();

// ==========================================
// 2. MIDDLEWARES (EL ORDEN ES CRÍTICO)
// ==========================================

// A. Manejo de Errores y Redirección (Debe ir primero)
app.UseExceptionHandler();
if (app.Environment.IsProduction())
{
    // OWASP: Obliga al uso de HTTPS en producción (HSTS)
    app.UseHsts();
}
app.UseHttpsRedirection();

// B. CORS: DEBE IR AQUÍ (Antes de la seguridad estricta y de Auth)
// Esto permite que el navegador acepte la conexión de Angular.
app.UseCors("Frontend");

// C. Seguridad OWASP (Cabeceras CSP, X-Frame-Options, etc.)
// Al estar después de CORS, ya no bloquea las peticiones iniciales del frontend.
app.UseMiddleware<SecurityHeadersMiddleware>();

// D. Prevención de Ataques (OWASP: Mitigación de DoS/Fuerza Bruta)
app.UseRateLimiter();

// E. Entorno de Desarrollo (Swagger)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "ArcanoPizza API v1");
    });
}

// F. Autenticación y Autorización (OWASP: Control de Acceso)
app.UseAuthentication();
app.UseAuthorization();

// G. Mapeo final
app.MapControllers();

app.Run();