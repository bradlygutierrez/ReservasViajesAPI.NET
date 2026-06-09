using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppReservasAPI.Models
{
    [Table("EstadosReserva", Schema = "viajes")]
    public class EstadoReserva
    {
        [Key]
        public int EstadoReservaId { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(510)]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    }
}