using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppReservasAPI.Models
{
    [Table("Usuarios", Schema = "viajes")]
    public class Usuarios
    {
        [Key]
        public int UsuarioId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public int RolId { get; set; }

        public DateTime FechaRegistro { get; set; }

        public bool Activo { get; set; }

        [ForeignKey(nameof(RolId))]
        public Roles? Rol { get; set; }
    }
}