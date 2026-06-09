using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.RolPantallaPermisos
{
    public class AsignarPermisosPantallaRolDto
    {
        [Required]
        public int RolId { get; set; }

        [Required]
        public int PantallaId { get; set; }

        [Required]
        public List<int> PermisoIds { get; set; } = new();
    }
}