using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppReservasAPI.Models
{
    [Table("RolPantallaPermisos", Schema = "viajes")]
    public class RolPantallaPermiso
    {
        [Key]
        public int RolPantallaPermisoId { get; set; }

        public int RolId { get; set; }

        public int PantallaId { get; set; }

        public int PermisoId { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        [ForeignKey(nameof(RolId))]
        public Rol? Rol { get; set; }

        [ForeignKey(nameof(PantallaId))]
        public Pantalla? Pantalla { get; set; }

        [ForeignKey(nameof(PermisoId))]
        public Permiso? Permiso { get; set; }
    }
}