using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Reservas;

public class SolicitarReembolsoDto
{
    [StringLength(1000)]
    public string? Motivo { get; set; }
}