using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppReservasAPI.Models
{
    [Table("MetodosPago", Schema = "viajes")]
    public class MetodoPago
    {
        [Key]
        public int MetodoPagoId { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(510)]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    }
}