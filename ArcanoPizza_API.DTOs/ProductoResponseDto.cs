using System;
using System.Collections.Generic;
using System.Text;

namespace ArcanoPizza_API.DTOs
{
    public class ProductoResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal PrecioBase { get; set; }
        public bool Activo { get; set; }
        public int IdCategoria { get; set; }

        // Campos agregados para que la API se los devuelva a Angular
        public string? Ingredientes { get; set; }
        public string? ImagenURL { get; set; }
    }
}