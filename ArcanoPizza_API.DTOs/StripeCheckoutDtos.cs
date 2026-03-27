namespace ArcanoPizza_API.DTOs;

/// <summary>Cuerpo de <c>POST /api/pagos/crear-sesion</c> (requiere JWT).</summary>
public class CrearSesionStripeDto
{
    public List<CarritoItemSeguroDto> Items { get; set; } = [];

    public int? DireccionId { get; set; }

    public string? TipoEntrega { get; set; }

    public int? PromocionId { get; set; }
}

public record ConfirmarSesionStripeDto(string SessionId);
