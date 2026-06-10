using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Paises
{
    public class ActualizarPaisDto
    {
        [Required]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        public bool Activo { get; set; }
    }
}