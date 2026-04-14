namespace ArcanoPizza_API.DTOs;

public record RegisterRequest(
    string NombreUsuario,
    string Correo,
    string Password,
    string? Telefono);

/// <summary>
/// Login: acepta <c>correo</c> o <c>email</c> en JSON (camelCase) para clientes antiguos o proxies que renombran campos.
/// </summary>
public class LoginRequest
{
    public string? Correo { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }
}

public record RefreshRequest(string RefreshToken);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    AuthUserDto Usuario);

public record AuthUserDto(int IdUsuario, string NombreUsuario, string Correo, string Rol);
