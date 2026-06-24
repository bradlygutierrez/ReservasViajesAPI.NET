using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppReservasAPI.Models;

[Table("ViajeLikes", Schema = "viajes")]
public class ViajeLike
{
    [Key]
    public int ViajeLikeId { get; set; }

    public int UsuarioId { get; set; }

    public int ViajeId { get; set; }

    public DateTime FechaCreacion { get; set; }

    [ForeignKey(nameof(UsuarioId))]
    public Usuario? Usuario { get; set; }

    [ForeignKey(nameof(ViajeId))]
    public Viaje? Viaje { get; set; }
}
