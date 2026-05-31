using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppReservasAPI.Models
{
    [Table("Roles", Schema = "viajes")]
    public class Roles
    {
        [Key]
        public int RolId { get; set; }

        public string Nombre { get; set; } = string.Empty;
    }
}