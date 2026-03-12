using ArcanoPizza_API.Data;
using ArcanoPizza_API.Extensions;
using ArcanoPizza_API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddData(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// OWASP Top 10: seguridad
builder.Services.AddSecurity(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

// OWASP A02: Cabeceras de seguridad
app.UseMiddleware<SecurityHeadersMiddleware>();

if (app.Environment.IsProduction())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseExceptionHandler(_ => { }); // Usa GlobalExceptionHandlerMiddleware

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "ArcanoPizza API v1");
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
