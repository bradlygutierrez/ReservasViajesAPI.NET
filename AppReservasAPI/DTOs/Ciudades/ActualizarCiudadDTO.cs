using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Ciudades
{
    public class ActualizarCiudadDto
    {
        [Required]
        public int PaisId { get; set; }

        [Required]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        public bool Activo { get; set; }
    }
}