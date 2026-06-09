using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Auth
{
    public class RegistroDto
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

        [StringLength(60)]
        public string? Telefono { get; set; }
    }
}