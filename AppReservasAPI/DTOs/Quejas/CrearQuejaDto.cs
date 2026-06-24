using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Quejas;

public class CrearQuejaDto
{
    public int? ReservaId { get; set; }

    public int? ViajeId { get; set; }

    [Required]
    [StringLength(50)]
    public string Tipo { get; set; } = "General";

    [Required]
    [StringLength(150)]
    public string Asunto { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Descripcion { get; set; } = string.Empty;
}
