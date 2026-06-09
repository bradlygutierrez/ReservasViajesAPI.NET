using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppReservasAPI.Models
{
    [Table("Disponibilidades", Schema = "viajes")]
    public class Disponibilidad
    {
        [Key]
        public int DisponibilidadId { get; set; }

        public int ViajeId { get; set; }

        [Column(TypeName = "date")]
        public DateTime Fecha { get; set; }

        public int CuposTotales { get; set; }

        public int CuposDisponibles { get; set; }

        [Column(TypeName = "date")]
        public DateTime? FechaRetorno { get; set; }

        public bool Activo { get; set; }

        [ForeignKey(nameof(ViajeId))]
        public Viaje? Viaje { get; set; }

        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    }
}