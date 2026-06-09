using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Reservas
{
    public class CrearReservaDto
    {
        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public int DisponibilidadId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad de personas debe ser mayor que cero.")]
        public int CantidadPersonas { get; set; }

        [StringLength(1000)]
        public string? Notas { get; set; }

        public List<CrearPasajeroReservaDto> Pasajeros { get; set; } = new();
    }
}