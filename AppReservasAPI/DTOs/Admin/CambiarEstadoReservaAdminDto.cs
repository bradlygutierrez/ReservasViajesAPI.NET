using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Admin;

public class CambiarEstadoReservaAdminDto
{
    [Required]
    public int EstadoReservaId { get; set; }

    [StringLength(300)]
    public string? Motivo { get; set; }
}
