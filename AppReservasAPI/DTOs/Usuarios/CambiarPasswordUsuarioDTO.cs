using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Usuarios
{
    public class CambiarPasswordUsuarioDto
    {
        [Required]
        [MinLength(6)]
        [StringLength(100)]
        public string NuevaPassword { get; set; } = string.Empty;
    }
}