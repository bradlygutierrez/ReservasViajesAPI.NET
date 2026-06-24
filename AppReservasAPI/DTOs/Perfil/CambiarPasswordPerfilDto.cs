using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Perfil;

public class CambiarPasswordPerfilDto
{
    [Required(ErrorMessage = "La contraseña actual es obligatoria.")]
    public string PasswordActual { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
    [MinLength(6, ErrorMessage = "La nueva contraseña debe tener al menos 6 caracteres.")]
    public string PasswordNueva { get; set; } = string.Empty;
}
