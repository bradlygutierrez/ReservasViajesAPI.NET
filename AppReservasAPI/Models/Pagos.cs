using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace AppReservasAPI.Models
{
    [Table("Pagos", Schema = "viajes")]
    public class Pagos
    {
        [Key]
        public int PagosId { get; set; }

        public int ReservaId { get; set; }

        public float Monto { get; set; }

        public string MetodoPago { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        public string Referencia {  get; set; } = string.Empty;

        public DateTime FechaPago { get; set; }

        [ForeignKey(nameof(ReservaId))]
        public Reservas? Reserva { get; set; }
    }
}
