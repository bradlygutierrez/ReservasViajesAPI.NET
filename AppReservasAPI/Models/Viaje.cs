using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppReservasAPI.Models
{
    [Table("Viajes", Schema = "viajes")]
    public class Viaje
    {
        [Key]
        public int ViajeId { get; set; }

        public int TipoViajeId { get; set; }

        public int DestinoId { get; set; }

        [Required]
        [StringLength(300)]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Descripcion { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Precio { get; set; }

        public int CuposTotales { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }


        [ForeignKey(nameof(TipoViajeId))]
        public TipoViaje? TipoViaje { get; set; }

        [ForeignKey(nameof(DestinoId))]
        public Destino? Destino { get; set; }


        public ICollection<Disponibilidad> Disponibilidades { get; set; } = new List<Disponibilidad>();
    }
}