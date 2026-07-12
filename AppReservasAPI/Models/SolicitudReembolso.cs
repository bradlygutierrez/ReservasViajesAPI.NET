using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppReservasAPI.Models;

[Table("SolicitudesReembolso", Schema = "viajes")]
public class SolicitudReembolso
{
    [Key]
    public int SolicitudReembolsoId { get; set; }

    public int ReservaId { get; set; }

    public int UsuarioId { get; set; }

    [StringLength(1000)]
    public string? Motivo { get; set; }

    [Required]
    [StringLength(30)]
    public string Estado { get; set; } = "Pendiente";

    [Column(TypeName = "decimal(12,2)")]
    public decimal MontoSolicitado { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal? MontoAprobado { get; set; }

    public DateTime FechaSolicitud { get; set; }

    public DateTime? FechaResolucion { get; set; }

    public int? RevisadoPorUsuarioId { get; set; }

    [StringLength(1000)]
    public string? ObservacionResolucion { get; set; }

    public Reserva? Reserva { get; set; }

    public Usuario? Usuario { get; set; }

    public Usuario? RevisadoPorUsuario { get; set; }
}