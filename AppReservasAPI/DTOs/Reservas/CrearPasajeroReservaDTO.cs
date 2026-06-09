using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Reservas
{
    public class CrearPasajeroReservaDto
    {
        [Required]
        [StringLength(300)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Documento { get; set; } = string.Empty;

        public DateTime? FechaNacimiento { get; set; }
    }
}