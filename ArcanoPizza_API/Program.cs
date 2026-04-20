using ArcanoPizza_API.Extensions;
using ArcanoPizza_API.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddArcanoPizzaCore(builder.Configuration);
builder.Services.AddSecurity(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseForwardedHeaders();

if (app.Environment.IsProduction())
{
    app.UseHsts();
}
else
{
    app.UseHttpsRedirection();
}

app.UseExceptionHandler();
app.UseHttpLogging();
app.UseRequestTraceLogging();
app.UseCors("Frontend");

app.UseMiddleware<SecurityHeadersMiddleware>();
if (!app.Environment.IsDevelopment())
{
    app.UseRateLimiter();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "ArcanoPizza API v1");
    });
}

app.UseAuthentication();
app.UseMiddleware<AuditLogMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();
