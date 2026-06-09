using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppReservasAPI.Models
{
    [Table("Paises", Schema = "viajes")]
    public class Pais
    {
        [Key]
        public int PaisId { get; set; }

        [Required]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        public bool Activo { get; set; }


        // Relación: Un país puede tener muchas ciudades
        public ICollection<Ciudad> Ciudades { get; set; } = new List<Ciudad>();
    }
}