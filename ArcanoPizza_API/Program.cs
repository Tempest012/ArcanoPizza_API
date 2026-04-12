using ArcanoPizza_API.Data;
using ArcanoPizza_API.Extensions;
using ArcanoPizza_API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. CONFIGURACIÓN DE SERVICIOS (Contenedor)
// ==========================================
builder.Services.AddData(builder.Configuration);
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

// Seguridad y Autenticación (OWASP Top 10)
builder.Services.AddSecurity(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

// ==========================================
// 2. TUBERÍA DE PETICIONES (Middleware Pipeline)
// ==========================================

// 2.1. Manejo de Excepciones (Lo más arriba posible)
app.UseExceptionHandler(_ => { }); // Usa GlobalExceptionHandlerMiddleware

// 2.2. Cabeceras de seguridad y redirección
app.UseMiddleware<SecurityHeadersMiddleware>();
if (app.Environment.IsProduction())
{
    app.UseHsts();
}
if (!app.Environment.IsDevelopment())
{
    // Solo fuerza HTTPS cuando el sistema esté en producción
    app.UseHttpsRedirection();
}

// 2.3. 🔥 CORS: Darle luz verde a Angular (Debe ir ANTES de Auth)
app.UseCors("Frontend");

// 2.4. Limitador de peticiones
app.UseRateLimiter();

// 2.5. Swagger (Solo en desarrollo)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "ArcanoPizza API v1");
    });
}

// 2.6. Seguridad (Quién eres y qué puedes hacer)
app.UseAuthentication();
app.UseAuthorization();

// 2.7. Mapeo final
app.MapControllers();

app.Run();