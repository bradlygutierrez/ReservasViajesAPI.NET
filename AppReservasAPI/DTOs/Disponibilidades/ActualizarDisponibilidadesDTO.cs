using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Disponibilidades
{
    public class ActualizarDisponibilidadDto
    {
        [Required]
        public int ViajeId { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        public DateTime? FechaRetorno { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Los cupos totales deben ser mayores que cero.")]
        public int CuposTotales { get; set; }

        public bool Activo { get; set; }
    }
}