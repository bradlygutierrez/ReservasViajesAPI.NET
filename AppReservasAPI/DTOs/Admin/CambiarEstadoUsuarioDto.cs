using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Admin;

public class CambiarEstadoUsuarioDto
{
    [Required]
    public bool Activo { get; set; }
}
