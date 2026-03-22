using System.ComponentModel.DataAnnotations;

namespace ArcanoPizza_API.DTOs;

public record PromocionResponseDto(
    int IdPromocion,
    string Titulo,
    string? Descripcion,
    string? Contenido,
    string? ImagenURL,
    decimal PrecioOriginal,
    decimal PrecioPromocional,
    decimal? PorcentajeDescuento,
    decimal AhorroMonto,
    int TipoVigencia,
    DateTime? FechaValidaHasta,
    int? DiaSemanaRecurrente,
    bool Activo);

public record PromocionCreateDto(
    [Required, MaxLength(200)] string Titulo,
    [MaxLength(1000)] string? Descripcion,
    [MaxLength(4000)] string? Contenido,
    [MaxLength(2048)] string? ImagenURL,
    [Range(0, 999999.99)] decimal PrecioOriginal,
    [Range(0, 999999.99)] decimal PrecioPromocional,
    [Range(0, 100)] decimal? PorcentajeDescuento,
    [Required] int TipoVigencia,
    DateTime? FechaValidaHasta,
    [Range(0, 6)] int? DiaSemanaRecurrente,
    bool Activo = true);

public record PromocionUpdateDto(
    [MaxLength(200)] string? Titulo,
    [MaxLength(1000)] string? Descripcion,
    [MaxLength(4000)] string? Contenido,
    [MaxLength(2048)] string? ImagenURL,
    [Range(0, 999999.99)] decimal? PrecioOriginal,
    [Range(0, 999999.99)] decimal? PrecioPromocional,
    [Range(0, 100)] decimal? PorcentajeDescuento,
    int? TipoVigencia,
    DateTime? FechaValidaHasta,
    [Range(0, 6)] int? DiaSemanaRecurrente,
    bool? Activo);
