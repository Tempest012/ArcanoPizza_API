using ArcanoPizza_API.Data;
using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.Data.Repositories;
using ArcanoPizza_API.Extensions;
using ArcanoPizza_API.Middleware;
using ArcanoPizza_API.Model;
using ArcanoPizza_API.Options;
using ArcanoPizza_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddData(builder.Configuration);
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IPedidoCreacionService, PedidoCreacionService>();

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateOnStart();

builder.Services.AddSingleton<IValidateOptions<JwtOptions>, JwtOptionsValidator>();
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddOptions<AuditLogRetentionOptions>()
    .Bind(builder.Configuration.GetSection(AuditLogRetentionOptions.SectionName));
builder.Services.AddHostedService<AuditLogRetentionService>();

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

// Logging de HTTP (útil para diagnosticar en Azure)
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields =
        HttpLoggingFields.RequestMethod
        | HttpLoggingFields.RequestPath
        | HttpLoggingFields.RequestQuery
        | HttpLoggingFields.RequestHeaders
        | HttpLoggingFields.ResponseStatusCode
        | HttpLoggingFields.Duration;
    options.RequestHeaders.Add("Origin");
    options.RequestHeaders.Add("Referer");
    options.RequestHeaders.Add("User-Agent");
});

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

// Soporte para reverse proxies (IP real / esquema) cuando aplica
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Importante: en Azure/App Services/NGINX suele ser necesario limpiar restricciones por redes desconocidas.
    // Se asume despliegue detrás de proxy administrado; ajusta si necesitas allowlist.
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});


builder.Services.AddSecurity(builder.Configuration, builder.Environment);

var app = builder.Build();

// Forwarded headers lo más temprano posible
app.UseForwardedHeaders();

if (app.Environment.IsProduction())
{
    // OWASP: Obliga al uso de HTTPS en producción (HSTS)
    app.UseHsts();
}
else
{
    app.UseHttpsRedirection();
}

// Excepciones primero (middleware global)
app.UseExceptionHandler();

// Logging de request/response metadata
app.UseHttpLogging();

// Logging de trazas por request (y body redactado SOLO para /api/Auth/login)
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("RequestTrace");
    var sw = Stopwatch.StartNew();
    try
    {
        if (HttpMethods.IsPost(context.Request.Method)
            && context.Request.Path.Equals("/api/Auth/login", StringComparison.OrdinalIgnoreCase))
        {
            // No loguear contraseñas. Solo registrar shape y correo/email.
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            try
            {
                using var doc = JsonDocument.Parse(body);
                string? correo = null;
                if (doc.RootElement.TryGetProperty("correo", out var c) && c.ValueKind == JsonValueKind.String)
                    correo = c.GetString();
                else if (doc.RootElement.TryGetProperty("email", out var e) && e.ValueKind == JsonValueKind.String)
                    correo = e.GetString();

                logger.LogInformation(
                    "LOGIN request: origin={Origin} ip={IP} correo/email={Correo} traceId={TraceId}",
                    context.Request.Headers.Origin.ToString(),
                    context.Connection.RemoteIpAddress?.ToString(),
                    correo,
                    context.TraceIdentifier);
            }
            catch
            {
                logger.LogInformation(
                    "LOGIN request: body invalid json origin={Origin} ip={IP} traceId={TraceId}",
                    context.Request.Headers.Origin.ToString(),
                    context.Connection.RemoteIpAddress?.ToString(),
                    context.TraceIdentifier);
            }
        }

        await next();
    }
    finally
    {
        sw.Stop();
        logger.LogInformation(
            "HTTP {Method} {Path} -> {StatusCode} ({ElapsedMs}ms) traceId={TraceId}",
            context.Request.Method,
            context.Request.Path.Value,
            context.Response.StatusCode,
            sw.ElapsedMilliseconds,
            context.TraceIdentifier);
    }
});

// CORS debe ir antes de auth
app.UseCors("Frontend");

// Seguridad y protección
app.UseMiddleware<SecurityHeadersMiddleware>();
if (!app.Environment.IsDevelopment())
{
    app.UseRateLimiter();
}

// E. Entorno de Desarrollo (Swagger)

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "ArcanoPizza API v1");
    });
}

// Autenticación y autorización
app.UseAuthentication();
app.UseMiddleware<AuditLogMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();