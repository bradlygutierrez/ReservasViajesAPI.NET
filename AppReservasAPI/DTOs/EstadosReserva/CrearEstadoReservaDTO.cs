using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.EstadosReserva
{
    public class CrearEstadoReservaDto
    {
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(510)]
        public string? Descripcion { get; set; }
    }
}