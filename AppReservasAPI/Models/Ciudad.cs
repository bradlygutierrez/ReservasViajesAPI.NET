using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppReservasAPI.Models
{
    [Table("Ciudades", Schema = "viajes")]
    public class Ciudad
    {
        [Key]
        public int CiudadId { get; set; }

        public int PaisId { get; set; }

        [Required]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        public bool Activo { get; set; }


        // Relación: Una ciudad pertenece a un país
        [ForeignKey(nameof(PaisId))]
        public Pais? Pais { get; set; }


        // Relación: Una ciudad puede tener muchos destinos
        public ICollection<Destino> Destinos { get; set; } = new List<Destino>();
    }
}