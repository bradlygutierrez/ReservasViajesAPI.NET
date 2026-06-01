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

        public string Titulo { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public float Precio { get; set; }

        public int CuposTotales { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        [ForeignKey(nameof(ViajeId))]

        public TIpoVIaje? TipoViaje { get; set; }

        [ForeignKey(nameof(DestinoId))]

        public Destinos? Destino { get; set; }

    }
}