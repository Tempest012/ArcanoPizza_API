using System.ComponentModel.DataAnnotations;

namespace ArcanoPizza_API.DTOs;

public record ExtraResponseDto(
    int IdExtra,
    string Nombre,
    decimal Precio,
    bool Activo);

public record ExtraCreateDto(
    [Required(ErrorMessage = "El nombre es requerido")]
    [MaxLength(100)]
    string Nombre,
    [Range(0, 99999.99, ErrorMessage = "El precio debe ser mayor o igual a 0")]
    decimal Precio,
    bool Activo = true);

public record ExtraUpdateDto(
    [MaxLength(100)]
    string? Nombre,
    [Range(0, 99999.99)]
    decimal? Precio,
    bool? Activo);
