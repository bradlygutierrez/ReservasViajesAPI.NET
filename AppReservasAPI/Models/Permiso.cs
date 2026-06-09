using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppReservasAPI.Models
{
    [Table("Permisos", Schema = "viajes")]
    public class Permiso
    {
        [Key]
        public int PermisoId { get; set; }

        [Required]
        [StringLength(100)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(510)]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        public ICollection<RolPantallaPermiso> RolPantallaPermisos { get; set; } = new List<RolPantallaPermiso>();
    }
}