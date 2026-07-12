using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppReservasAPI.Models
{
    [Table("Usuarios", Schema = "viajes")]
    public class Usuario
    {
        [Key]
        public int UsuarioId { get; set; }

        [Required]
        [StringLength(300)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(400)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(1024)]
        public string PasswordHash { get; set; } = string.Empty;

        public int RolId { get; set; }

        public DateTime FechaRegistro { get; set; }

        public bool Activo { get; set; }

        [StringLength(60)]
        public string? Telefono { get; set; }

        [ForeignKey(nameof(RolId))]
        public Rol? Rol { get; set; }

        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

        public ICollection<HistorialEstadoReserva> HistorialEstadoReservas { get; set; } = new List<HistorialEstadoReserva>();

        public AgenteCertificacion? AgenteCertificacion { get; set; }

        public ICollection<Notificacion> Notificaciones { get; set; }
            = new List<Notificacion>();
    }
}