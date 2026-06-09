using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Reservas
{
    public class CambiarEstadoReservaDto
    {
        [Required]
        public int EstadoNuevoId { get; set; }

        public int? UsuarioId { get; set; }

        [StringLength(600)]
        public string? Motivo { get; set; }
    }
}