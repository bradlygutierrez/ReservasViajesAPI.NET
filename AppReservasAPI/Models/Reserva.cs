using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppReservasAPI.Models
{
    [Table("Reservas", Schema = "viajes")]
    public class Reserva
    {
        [Key]
        public int ReservaId { get; set; }

        public int UsuarioId { get; set; }

        public int DisponibilidadId { get; set; }

        public int EstadoReservaId { get; set; }

        public DateTime FechaReserva { get; set; }

        public int CantidadPersonas { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioUnitario { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal? Total { get; set; }

        public DateTime FechaActualizacion { get; set; }

        [StringLength(1000)]
        public string? Notas { get; set; }

        [ForeignKey(nameof(UsuarioId))]
        public Usuario? Usuario { get; set; }

        [ForeignKey(nameof(DisponibilidadId))]
        public Disponibilidad? Disponibilidad { get; set; }

        [ForeignKey(nameof(EstadoReservaId))]
        public EstadoReserva? EstadoReserva { get; set; }

        public ICollection<PasajeroReserva> PasajerosReserva { get; set; } = new List<PasajeroReserva>();

        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();

        public ICollection<HistorialEstadoReserva> HistorialEstadosReserva { get; set; } = new List<HistorialEstadoReserva>();
    }
}