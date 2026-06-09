using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppReservasAPI.Models
{
    [Table("Pagos", Schema = "viajes")]
    public class Pago
    {
        [Key]
        public int PagoId { get; set; }

        public int ReservaId { get; set; }

        public int EstadoPagoId { get; set; }

        public int MetodoPagoId { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Monto { get; set; }

        [StringLength(100)]
        public string? Referencia { get; set; }

        public DateTime FechaPago { get; set; }

        [ForeignKey(nameof(ReservaId))]
        public Reserva? Reserva { get; set; }

        [ForeignKey(nameof(EstadoPagoId))]
        public EstadoPago? EstadoPago { get; set; }

        [ForeignKey(nameof(MetodoPagoId))]
        public MetodoPago? MetodoPago { get; set; }
    }
}