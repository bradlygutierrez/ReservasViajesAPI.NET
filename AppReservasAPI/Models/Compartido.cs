using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppReservasAPI.Models;

[Table("Compartidos", Schema = "viajes")]
public class Compartido
{
    [Key]
    public int CompartidoId { get; set; }

    public int? UsuarioId { get; set; }

    public int? ViajeId { get; set; }

    public int? ReservaId { get; set; }

    [StringLength(50)]
    public string? Canal { get; set; }

    [StringLength(500)]
    public string? Url { get; set; }

    public DateTime FechaCreacion { get; set; }

    [ForeignKey(nameof(UsuarioId))]
    public Usuario? Usuario { get; set; }

    [ForeignKey(nameof(ViajeId))]
    public Viaje? Viaje { get; set; }

    [ForeignKey(nameof(ReservaId))]
    public Reserva? Reserva { get; set; }
}
