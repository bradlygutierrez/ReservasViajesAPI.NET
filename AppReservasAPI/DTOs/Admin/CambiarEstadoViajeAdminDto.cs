using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Admin;

public class CambiarEstadoViajeAdminDto
{
    [Required]
    public bool Activo { get; set; }
}
