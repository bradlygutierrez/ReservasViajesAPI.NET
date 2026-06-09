using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Usuarios
{
    public class CrearUsuarioDto
    {
        [Required]
        [StringLength(300)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(400)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public int RolId { get; set; }

        [StringLength(60)]
        public string? Telefono { get; set; }
    }
}