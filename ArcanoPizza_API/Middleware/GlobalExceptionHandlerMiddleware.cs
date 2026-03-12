using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ArcanoPizza_API.Middleware;

/// <summary>
/// Middleware para manejar excepciones globalmente (OWASP A10: Mishandling of Exceptional Conditions).
/// Evita filtrar stack traces o información sensible al cliente.
/// </summary>
public class GlobalExceptionHandlerMiddleware : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandlerMiddleware(
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Log de seguridad (OWASP A09): registrar eventos de error sin exponer datos sensibles
        _logger.LogError(exception, "Error no controlado en {Method} {Path}. RequestId: {RequestId}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            httpContext.TraceIdentifier);

        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "Ha ocurrido un error",
            Status = (int)HttpStatusCode.InternalServerError,
            Instance = httpContext.Request.Path,
            Detail = _env.IsDevelopment()
                ? exception.Message
                : "Ha ocurrido un error interno. Por favor, inténtelo más tarde.",
            Extensions =
            {
                ["traceId"] = httpContext.TraceIdentifier,
                ["requestId"] = httpContext.TraceIdentifier
            }
        };

        // En producción NUNCA exponemos stack trace ni detalles internos
        if (_env.IsDevelopment())
        {
            problemDetails.Extensions["exception"] = exception.GetType().FullName;
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsync(
            JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            }),
            cancellationToken);

        return true;
    }
}
