using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Admin;

public class CambiarRolUsuarioDto
{
    [Required]
    public int RolId { get; set; }
}
