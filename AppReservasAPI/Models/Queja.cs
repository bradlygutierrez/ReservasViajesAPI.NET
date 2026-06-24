using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppReservasAPI.Models;

[Table("Quejas", Schema = "viajes")]
public class Queja
{
    [Key]
    public int QuejaId { get; set; }

    public int UsuarioId { get; set; }

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

    [Required]
    [StringLength(30)]
    public string Estado { get; set; } = "Pendiente";

    [StringLength(1000)]
    public string? RespuestaAdmin { get; set; }

    public int? AtendidoPorUsuarioId { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    [ForeignKey(nameof(UsuarioId))]
    public Usuario? Usuario { get; set; }

    [ForeignKey(nameof(ReservaId))]
    public Reserva? Reserva { get; set; }

    [ForeignKey(nameof(ViajeId))]
    public Viaje? Viaje { get; set; }

    [ForeignKey(nameof(AtendidoPorUsuarioId))]
    public Usuario? AtendidoPorUsuario { get; set; }
}
