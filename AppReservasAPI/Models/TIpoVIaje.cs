using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppReservasAPI.Models;

[Table("TiposViaje", Schema = "viajes")]
public class TipoViaje
{
    [Key]
    public int TipoViajeId { get; set; }

    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public ICollection<Viaje> Viajes { get; set; } = new List<Viaje>();
}
