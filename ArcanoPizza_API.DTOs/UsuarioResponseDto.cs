using System;
using System.Collections.Generic;
using System.Text;

namespace ArcanoPizza_API.DTOs
{
    public class UsuarioResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string? Telefono { get; set; }
        public string Tipo { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaMiembro { get; set; }
    }
}
