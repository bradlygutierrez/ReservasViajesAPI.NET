using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Roles
{
    public class ActualizarRolDto
    {
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;
    }
}