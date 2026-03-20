namespace ArcanoPizza_API.DTOs;

public record RegisterRequest(
    string NombreUsuario,
    string Correo,
    string Password,
    string? Telefono);

public record LoginRequest(string Correo, string Password);

public record RefreshRequest(string RefreshToken);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    AuthUserDto Usuario);

public record AuthUserDto(int IdUsuario, string NombreUsuario, string Correo, string Rol);
