namespace ArcanoPizza_API.Model;

/// <summary>
/// Cálculo de totales cuando un pedido aplica <see cref="Pedido.DescuentoTotal"/> por promoción.
/// </summary>
public static class PedidoTotales
{
    /// <summary>
    /// Total cobrado: subtotal de ítems menos descuento más impuestos (el descuento no debe exceder el subtotal).
    /// </summary>
    public static decimal CalcularTotal(decimal subtotal, decimal descuentoTotal, decimal impuestos)
    {
        var neto = subtotal - descuentoTotal;
        if (neto < 0)
            neto = 0;
        return neto + impuestos;
    }
}
