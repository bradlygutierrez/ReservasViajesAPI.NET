using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppReservasAPI.Models
{
    [Table("Destinos", Schema = "viajes")]
    public class Destino
    {
        [Key]
        public int DestinoId { get; set; }

        [StringLength(1000)]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; }

        public int CiudadId { get; set; }

        [ForeignKey(nameof(CiudadId))]
        public Ciudad? Ciudad { get; set; }

        public ICollection<Viaje> Viajes { get; set; } = new List<Viaje>();
    }
}