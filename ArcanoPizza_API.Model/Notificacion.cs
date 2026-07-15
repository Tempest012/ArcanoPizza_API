namespace ArcanoPizza_API.Model;

public class Notificacion
{
    public int IdNotificacion { get; set; }
    public int FkIdUsuario { get; set; }
    public int FkIdPedido { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public bool Leida { get; set; }

    public Usuario Usuario { get; set; } = null!;
    public Pedido Pedido { get; set; } = null!;
}
