using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Viajes;

public class ViajeItinerarioDto
{
    [Range(1, int.MaxValue)]
    public int Dia { get; set; }

    [Required]
    [StringLength(150)]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(1500)]
    public string? Descripcion { get; set; }

    [StringLength(1000)]
    public string? Sitios { get; set; }

    public int Orden { get; set; }
}