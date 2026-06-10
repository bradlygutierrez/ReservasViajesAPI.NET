using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.TipoViajes
{
    public class CrearTipoViajeDto
    {
        [Required]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(600)]
        public string? Descripcion { get; set; }
    }
}