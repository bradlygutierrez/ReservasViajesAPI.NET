using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppReservasAPI.Models;

[Table("Notificaciones", Schema = "viajes")]
public class Notificacion
{
    [Key]
    public long NotificacionId { get; set; }

    public int UsuarioId { get; set; }

    [Required]
    [StringLength(150)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    [StringLength(1500)]
    public string Mensaje { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Tipo { get; set; }

    public int? ViajeId { get; set; }

    public int? ReservaId { get; set; }

    public bool Leida { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaLectura { get; set; }

    public Usuario? Usuario { get; set; }

    public Viaje? Viaje { get; set; }

    public Reserva? Reserva { get; set; }
}