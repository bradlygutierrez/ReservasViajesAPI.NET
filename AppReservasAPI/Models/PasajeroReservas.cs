using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppReservasAPI.Models
{
    [Table("PasajeroReservas", Schema = "viajes")]
    public class PasajeroReservas
    {
        [Key]
        public int PasajeroReservaId { get; set; }

        public int ReservaId { get; set; } = 0;

        public int PasajeroId { get; set; } = 0;

        public string NombreCompleto { get; set; } = string.Empty;

        public string Documento { get; set; } = string.Empty;

        [ForeignKey(nameof(ReservaId))]
        public Reservas? Reserva { get; set; }

        [ForeignKey(nameof(PasajeroId))]
        public Usuarios? Pasajero { get; set; }
    }
}
