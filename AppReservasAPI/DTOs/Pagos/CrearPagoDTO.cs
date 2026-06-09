using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Pagos
{
    public class CrearPagoDto
    {
        [Required]
        public int ReservaId { get; set; }

        [Required]
        public int MetodoPagoId { get; set; }

        [Required]
        public int EstadoPagoId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero.")]
        public decimal Monto { get; set; }

        [StringLength(100)]
        public string? Referencia { get; set; }

        public int? UsuarioId { get; set; }
    }
}