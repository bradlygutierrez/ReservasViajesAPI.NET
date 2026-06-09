using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppReservasAPI.Models
{
    [Table("HistorialEstadoReserva", Schema = "viajes")]
    public class HistorialEstadoReserva
    {
        [Key]
        public long HistorialId { get; set; }

        public int ReservaId { get; set; }

        public int? UsuarioId { get; set; }

        [StringLength(600)]
        public string? Motivo { get; set; }

        public DateTime FechaCambio { get; set; }

        public int? EstadoAnteriorId { get; set; }

        public int EstadoNuevoId { get; set; }

        [ForeignKey(nameof(ReservaId))]
        public Reserva? Reserva { get; set; }

        [ForeignKey(nameof(UsuarioId))]
        public Usuario? Usuario { get; set; }

        [ForeignKey(nameof(EstadoAnteriorId))]
        public EstadoReserva? EstadoAnterior { get; set; }

        [ForeignKey(nameof(EstadoNuevoId))]
        public EstadoReserva? EstadoNuevo { get; set; }
    }
}