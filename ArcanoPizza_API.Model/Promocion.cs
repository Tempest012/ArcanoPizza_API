namespace ArcanoPizza_API.Model;

public class Promocion
{
    public int IdPromocion { get; set; }
    public string Titulo { get; set; } = string.Empty;

    /// <summary>Resumen corto (card, subtítulo).</summary>
    public string? Descripcion { get; set; }

    /// <summary>Detalle del combo: qué incluye (lista). Una línea por ítem o texto libre; el front puede partir por saltos de línea.</summary>
    public string? Contenido { get; set; }

    public string? ImagenURL { get; set; }

    /// <summary>Precio de referencia (tachado en UI).</summary>
    public decimal PrecioOriginal { get; set; }

    /// <summary>Precio con promoción aplicada.</summary>
    /// <remarks>El ahorro en pesos es <c>PrecioOriginal - PrecioPromocional</c> (expuesto como <c>ahorroMonto</c> en la API).</remarks>
    public decimal PrecioPromocional { get; set; }

    public TipoVigenciaPromocion TipoVigencia { get; set; }

    /// <summary>Válida cuando <see cref="TipoVigencia"/> es <see cref="TipoVigenciaPromocion.FechaHasta"/>.</summary>
    public DateTime? FechaValidaHasta { get; set; }

    /// <summary>0–6 según <see cref="DayOfWeek"/> cuando <see cref="TipoVigencia"/> es <see cref="TipoVigenciaPromocion.DiaSemanaRecurrente"/>.</summary>
    public int? DiaSemanaRecurrente { get; set; }

    public bool Activo { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
