using System;
using System.Collections.Generic;
using System.Text;

namespace ArcanoPizza_API.DTOs
{
    public class CarritoItemSeguroDto
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }

        // 🔥 NUEVOS CAMPOS: Para que el backend sepa el tamaño y el precio
        public int? TamanoPizzaId { get; set; }
        public decimal Precio { get; set; }
    }
}