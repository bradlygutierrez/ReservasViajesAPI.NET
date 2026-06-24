using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Perfil;

public class ActualizarPerfilDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede exceder 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(200, ErrorMessage = "El correo no puede exceder 200 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [StringLength(30, ErrorMessage = "El teléfono no puede exceder 30 caracteres.")]
    public string? Telefono { get; set; }
}
