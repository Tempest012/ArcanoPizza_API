namespace ArcanoPizza_API.Model;

public class Pago
{
    public int IdPago { get; set; }
    public string Proveedor { get; set; } = string.Empty;
    public string? ProveedorPagoId { get; set; }
    public decimal Monto { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string MetodoPago { get; set; } = string.Empty;
    public DateTime? TimeStamp { get; set; }
    public int FkIdPedido { get; set; }

    public Pedido Pedido { get; set; } = null!;
}
