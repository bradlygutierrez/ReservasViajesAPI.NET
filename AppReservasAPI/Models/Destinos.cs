using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace AppReservasAPI.Models
{
    [Table("Destinos", Schema = "viajes")]
    public class Destinos
    {
        [Key]
        public int DestinoId { get; set; }

        public string Ciudad { get; set; } = string.Empty;

        public String Pais { get; set; } = string.Empty;

        public String Descripcion { get; set; } = string.Empty;
    }
}
