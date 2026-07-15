namespace ArcanoPizza_API.Model;

/// <summary>Constantes y máquina de estados para pedidos de salón (TipoEntrega = Salon).</summary>
public static class SalonEstados
{
    public const string TipoEntregaSalon = "Salon";

    public const string Pendiente = "Pendiente";
    public const string EnPreparacion = "En preparacion";
    public const string Listo = "Listo";
    public const string Recogida = "Recogida";
    public const string Entregado = "Entregado";
    public const string Pagado = "Pagado";
    public const string Cancelado = "Cancelado";

    public const string MesaDisponible = "Disponible";
    public const string MesaOcupada = "Ocupada";
    public const string MesaReservada = "Reservada";

    public const string RolAdministrador = "Administrador";
    public const string RolDespachador = "Despachador";
    public const string RolOperador = "Operador";

    private static readonly HashSet<string> EstadosTerminales = new(StringComparer.OrdinalIgnoreCase)
    {
        Pagado, Cancelado
    };

    private static readonly Dictionary<string, string> SiguienteNormal = new(StringComparer.OrdinalIgnoreCase)
    {
        [Pendiente] = EnPreparacion,
        [EnPreparacion] = Listo,
        [Listo] = Recogida,
        [Recogida] = Entregado,
        // Entregado → Pagado solo vía cerrar mesa
    };

    public static bool EsTerminal(string estado) =>
        EstadosTerminales.Contains(estado);

    public static bool PuedeCancelar(string estadoActual) =>
        !string.Equals(estadoActual, Pagado, StringComparison.OrdinalIgnoreCase)
        && !string.Equals(estadoActual, Cancelado, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Valida transición de estado (no incluye Pagado; ese va por cierre de mesa).
    /// </summary>
    public static (bool Ok, string? Error) ValidarTransicion(string actual, string nuevo, string? rol, bool esOperadorDelPedido)
    {
        if (string.IsNullOrWhiteSpace(nuevo))
            return (false, "El estado no puede estar vacío.");

        var destino = nuevo.Trim();

        if (string.Equals(destino, Pagado, StringComparison.OrdinalIgnoreCase))
            return (false, "El estado Pagado solo se asigna al cerrar la mesa.");

        if (string.Equals(destino, Cancelado, StringComparison.OrdinalIgnoreCase))
        {
            if (!PuedeCancelar(actual))
                return (false, "No se puede cancelar un pedido en este estado.");
            if (!EsDespachadorOAdmin(rol))
                return (false, "Solo Despachador o Administrador pueden cancelar.");
            return (true, null);
        }

        if (!SiguienteNormal.TryGetValue(actual, out var esperado)
            || !string.Equals(destino, esperado, StringComparison.OrdinalIgnoreCase))
            return (false, $"Transición inválida: {actual} → {destino}.");

        // Despachador: Pendiente → En preparacion → Listo
        if (string.Equals(destino, EnPreparacion, StringComparison.OrdinalIgnoreCase)
            || string.Equals(destino, Listo, StringComparison.OrdinalIgnoreCase))
        {
            if (!EsDespachadorOAdmin(rol))
                return (false, "Solo Despachador o Administrador pueden avanzar hasta Listo.");
            return (true, null);
        }

        // Operador: Listo → Recogida → Entregado
        if (string.Equals(destino, Recogida, StringComparison.OrdinalIgnoreCase)
            || string.Equals(destino, Entregado, StringComparison.OrdinalIgnoreCase))
        {
            if (EsDespachadorOAdmin(rol))
                return (true, null);
            if (string.Equals(rol, RolOperador, StringComparison.OrdinalIgnoreCase) && esOperadorDelPedido)
                return (true, null);
            return (false, "Solo el Operador asignado puede confirmar recolección o entrega.");
        }

        return (false, "Transición no permitida.");
    }

    public static bool EsDespachadorOAdmin(string? rol) =>
        string.Equals(rol, RolDespachador, StringComparison.OrdinalIgnoreCase)
        || string.Equals(rol, RolAdministrador, StringComparison.OrdinalIgnoreCase)
        || string.Equals(rol, "Tecnico", StringComparison.OrdinalIgnoreCase);

    public static string NormalizarEstadoMesa(string? estado)
    {
        if (string.IsNullOrWhiteSpace(estado))
            return MesaDisponible;
        var e = estado.Trim();
        if (string.Equals(e, MesaDisponible, StringComparison.OrdinalIgnoreCase)) return MesaDisponible;
        if (string.Equals(e, MesaOcupada, StringComparison.OrdinalIgnoreCase)) return MesaOcupada;
        if (string.Equals(e, MesaReservada, StringComparison.OrdinalIgnoreCase)) return MesaReservada;
        throw new ArgumentException("Estado de mesa inválido. Use Disponible, Ocupada o Reservada.");
    }
}
