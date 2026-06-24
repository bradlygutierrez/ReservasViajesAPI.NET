using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Interacciones;

public class RegistrarCompartidoDto
{
    public int? ViajeId { get; set; }

    public int? ReservaId { get; set; }

    [StringLength(50)]
    public string? Canal { get; set; }

    [StringLength(500)]
    public string? Url { get; set; }
}
