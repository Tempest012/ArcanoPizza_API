using ArcanoPizza_API.DTOs;

namespace ArcanoPizza_API.Data.IServices;

/// <summary>Resultado de operaciones de autenticación para mapear respuestas HTTP en el controller.</summary>
public sealed class AuthOutcome
{
    public AuthResponse? Response { get; init; }
    public int StatusCode { get; init; }
    public object? Body { get; init; }
}
