using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Pagos
{
    public class CambiarEstadoPagoDto
    {
        [Required]
        public int EstadoPagoId { get; set; }

        public int? UsuarioId { get; set; }

        [StringLength(600)]
        public string? Motivo { get; set; }
    }
}