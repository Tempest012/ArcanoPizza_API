using System;

namespace ArcanoPizza_API.DTOs
{
    public class ProductoAdminDto
    {
        // Se inicializa para evitar el error CS8618 de valores nulos
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int FkIdCategoria { get; set; }

        public string? Ingredientes { get; set; }
        public string? ImagenURL { get; set; }
    }
}