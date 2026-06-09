using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.RolPantallaPermisos
{
    public class CrearRolPantallaPermisoDto
    {
        [Required]
        public int RolId { get; set; }

        [Required]
        public int PantallaId { get; set; }

        [Required]
        public int PermisoId { get; set; }
    }
}