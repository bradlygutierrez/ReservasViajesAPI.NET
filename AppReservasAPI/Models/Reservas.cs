using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppReservasAPI.Models
{
    [Table("Reservas", Schema = "viajes")]
    public class Reservas
    {
        [Key]
        public int ReservaId { get; set; }

        public int UsuarioId { get; set; }
        
        public int DisponibilidadId { get; set; }

        public DateTime FechaReserva { get; set; }

        public int CantidadPersonas { get; set; }

        public float PrecioUnitario { get; set; }

        public float Total { get; set; }
        
        public string Estado { get; set; } = string.Empty;

        [ForeignKey(nameof(DisponibilidadId))]
        public Disponibilidades? Disponibilidad { get; set; } = null;

        [ForeignKey(nameof(UsuarioId))]

        public Usuarios? Usuario { get; set; } = null;


    }
}
