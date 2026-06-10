using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Permisos
{
    public class ActualizarPermisoDto
    {
        [Required]
        [StringLength(100)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(510)]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; }
    }
}