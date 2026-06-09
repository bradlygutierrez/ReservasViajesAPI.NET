using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppReservasAPI.Models
{
    [Table("Pantallas", Schema = "viajes")]
    public class Pantalla
    {
        [Key]
        public int PantallaId { get; set; }

        [Required]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(400)]
        public string Ruta { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Modulo { get; set; }

        [StringLength(200)]
        public string? Icono { get; set; }

        public int Orden { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        public ICollection<RolPantallaPermiso> RolPantallaPermisos { get; set; } = new List<RolPantallaPermiso>();
    }
}