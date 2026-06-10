using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.EstadosPago
{
    public class CrearEstadoPagoDto
    {
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(510)]
        public string? Descripcion { get; set; }
    }
}