using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppReservasAPI.Models;

[Table("ViajeInclusiones", Schema = "viajes")]
public class ViajeInclusion
{
    [Key]
    public int ViajeInclusionId { get; set; }

    public int ViajeId { get; set; }

    [Required]
    [StringLength(50)]
    public string Tipo { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Detalle { get; set; }

    public int Orden { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    [ForeignKey(nameof(ViajeId))]
    public Viaje? Viaje { get; set; }
}