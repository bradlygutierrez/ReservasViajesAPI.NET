using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.EstadosReserva
{
    public class ActualizarEstadoReservaDto
    {
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(510)]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; }
    }
}