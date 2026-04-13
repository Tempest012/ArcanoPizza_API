using System;
using System.Collections.Generic;
using System.Text;

namespace ArcanoPizza_API.DTOs
{
    public class ProductoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Ingredientes { get; set; }
        public string? ImagenURL { get; set; }
        public decimal Precio { get; set; }
        public string? CategoriaNombre { get; set; }
    }
}