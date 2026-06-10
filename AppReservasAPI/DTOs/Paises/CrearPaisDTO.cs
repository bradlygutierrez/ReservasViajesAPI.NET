using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Paises
{
    public class CrearPaisDto
    {
        [Required]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;
    }
}