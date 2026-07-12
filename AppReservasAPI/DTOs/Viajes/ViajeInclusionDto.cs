using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Viajes;

public class ViajeInclusionDto
{
    [Required]
    [StringLength(50)]
    public string Tipo { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Detalle { get; set; }

    public int Orden { get; set; }
}