using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppReservasAPI.Models
{
    [Table("PasajerosReserva", Schema = "viajes")]
    public class PasajeroReserva
    {
        [Key]
        public int PasajeroId { get; set; }

        public int ReservaId { get; set; }

        [Required]
        [StringLength(300)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Documento { get; set; } = string.Empty;

        [Column(TypeName = "date")]
        public DateTime? FechaNacimiento { get; set; }

        [ForeignKey(nameof(ReservaId))]
        public Reserva? Reserva { get; set; }
    }
}