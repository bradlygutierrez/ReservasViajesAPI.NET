using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Ciudades
{
    public class CrearCiudadDto
    {
        [Required]
        public int PaisId { get; set; }

        [Required]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;
    }
}