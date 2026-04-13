using ArcanoPizza_API.Model;

namespace ArcanoPizza_API.Helpers;

internal static class PromocionVigencia
{
    public static bool EstaVigente(Promocion p, DateTime utcNow)
    {
        if (!p.Activo)
            return false;

        return p.TipoVigencia switch
        {
            TipoVigenciaPromocion.FechaHasta => p.FechaValidaHasta is not null
                && utcNow.Date <= p.FechaValidaHasta.Value.Date,
            TipoVigenciaPromocion.DiaSemanaRecurrente => p.DiaSemanaRecurrente is not null
                && (int)utcNow.DayOfWeek == p.DiaSemanaRecurrente.Value,
            _ => false,
        };
    }
}
