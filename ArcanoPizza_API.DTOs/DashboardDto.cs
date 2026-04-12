using System;
using System.Collections.Generic;
using System.Text;

namespace ArcanoPizza_API.DTOs
{
    public class DashboardDto
    {
        public decimal VentasHoy { get; set; }
        public int PedidosActivos { get; set; }
        public List<ProductoVendidoDto> ProductosVendidos { get; set; } = new();
        public List<PedidosHoraDto> PedidosPorHora { get; set; } = new();
    }

    public class ProductoVendidoDto
    {
        public string Nombre { get; set; } = string.Empty;
        public int Vendidos { get; set; }
        public decimal Total { get; set; }
    }

    public class PedidosHoraDto
    {
        public string Hora { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }
}