using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Reservas;

public class ReagendarReservaDto
{
    [Range(1, int.MaxValue)]
    public int NuevaDisponibilidadId { get; set; }

    [StringLength(500)]
    public string? Motivo { get; set; }
}