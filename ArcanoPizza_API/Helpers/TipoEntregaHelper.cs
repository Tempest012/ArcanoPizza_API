namespace ArcanoPizza_API.Helpers;

public static class TipoEntregaHelper
{
    /// <summary>Coincide con el valor del frontend para «Recoger en local».</summary>
    public static bool EsRecogerEnLocal(string? tipoEntrega) =>
        string.Equals(tipoEntrega?.Trim(), "Recoger", StringComparison.OrdinalIgnoreCase);
}
