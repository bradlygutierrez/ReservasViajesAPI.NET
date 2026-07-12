using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppReservasAPI.Models;

[Table("ViajeItinerarios", Schema = "viajes")]
public class ViajeItinerario
{
    [Key]
    public int ViajeItinerarioId { get; set; }

    public int ViajeId { get; set; }

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

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    [ForeignKey(nameof(ViajeId))]
    public Viaje? Viaje { get; set; }
}