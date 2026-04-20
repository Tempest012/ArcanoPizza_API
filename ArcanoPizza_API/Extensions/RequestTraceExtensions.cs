using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ArcanoPizza_API.Extensions;

public static class RequestTraceExtensions
{
    /// <summary>Trazas por request; en POST /api/Auth/login solo registra correo/email (sin contraseña).</summary>
    public static IApplicationBuilder UseRequestTraceLogging(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("RequestTrace");
            var sw = Stopwatch.StartNew();
            try
            {
                if (HttpMethods.IsPost(context.Request.Method)
                    && context.Request.Path.Equals("/api/Auth/login", StringComparison.OrdinalIgnoreCase))
                {
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
    }
}
