using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Pantallas
{
    public class CrearPantallaDto
    {
        [Required]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(400)]
        public string Ruta { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Modulo { get; set; }

        [StringLength(200)]
        public string? Icono { get; set; }

        public int Orden { get; set; }
    }
}