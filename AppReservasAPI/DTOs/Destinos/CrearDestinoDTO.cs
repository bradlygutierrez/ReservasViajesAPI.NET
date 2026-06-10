using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Destinos
{
    public class CrearDestinoDto
    {
        [Required]
        public int CiudadId { get; set; }

        [StringLength(1000)]
        public string? Descripcion { get; set; }
    }
}