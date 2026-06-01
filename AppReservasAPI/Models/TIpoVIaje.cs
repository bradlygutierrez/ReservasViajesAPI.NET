using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppReservasAPI.Models
{
    [Table("TiposViaje", Schema = "viajes")]

    public class TIpoVIaje
    {
        [Key]
        public int TipoViajeId { get; set; }

        public string Nombre { get; set; } = string.Empty;

    }
}
