namespace ArcanoPizza_API.DTOs;

public record ProductoResponseDto(
    int IdProducto,
    string Nombre,
    string? Descripcion,
    decimal PrecioBase,
    bool Activo,
    int FkIdCategoria);

public record ProductoCreateDto(
    string Nombre,
    string? Descripcion,
    decimal PrecioBase,
    bool Activo = true,
    int FkIdCategoria = 1);

public record ProductoUpdateDto(
    string? Nombre,
    string? Descripcion,
    decimal? PrecioBase,
    bool? Activo,
    int? FkIdCategoria);
