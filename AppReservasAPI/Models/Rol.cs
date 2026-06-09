using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppReservasAPI.Models
{
    [Table("Roles", Schema = "viajes")]
    public class Rol
    {
        [Key]
        public int RolId { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;


        // Relación: Un rol puede pertenecer a muchos usuarios
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();

        // Relación: Un rol puede tener muchos permisos por pantalla
        public ICollection<RolPantallaPermiso> RolPantallaPermisos { get; set; } = new List<RolPantallaPermiso>();
    }
}