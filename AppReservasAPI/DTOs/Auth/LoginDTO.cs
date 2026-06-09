using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Auth
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        [StringLength(400)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;
    }
}