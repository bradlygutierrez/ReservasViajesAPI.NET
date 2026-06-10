using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Viajes
{
    public class CrearViajeDto
    {
        [Required]
        public int TipoViajeId { get; set; }

        [Required]
        public int DestinoId { get; set; }

        [Required]
        [StringLength(300)]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Descripcion { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor que cero.")]
        public decimal Precio { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Los cupos totales deben ser mayores que cero.")]
        public int CuposTotales { get; set; }
    }
}