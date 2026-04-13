using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ArcanoPizza_API.DTOs
{
    public class UsuarioAdminDto
    {
        [Required]
        public string Nombre { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string? Telefono { get; set; }

        [Required]
        public string Tipo { get; set; }

    }
}