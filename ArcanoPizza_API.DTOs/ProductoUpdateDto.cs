using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Text;

namespace ArcanoPizza_API.DTOs
{
    public class ProductoUpdateDto
    {
        [Required(ErrorMessage = "El nombre del producto es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        [Required]
        [Range(0.01, 10000, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal Precio { get; set; }

        public bool Activo { get; set; }

        [Required]
        public int FkIdCategoria { get; set; }

        public string? Ingredientes { get; set; }
        public string? ImagenURL { get; set; }
    }
}