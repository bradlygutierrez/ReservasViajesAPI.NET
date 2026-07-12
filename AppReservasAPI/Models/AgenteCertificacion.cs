using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppReservasAPI.Models;

[Table("AgenteCertificaciones", Schema = "viajes")]
public class AgenteCertificacion
{
    [Key]
    public int AgenteCertificacionId { get; set; }

    public int UsuarioId { get; set; }

    [Required]
    [StringLength(200)]
    public string NombreLegal { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Cedula { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string NumeroLicencia { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string DocumentoCedulaUrl { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string DocumentoLicenciaUrl { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string Estado { get; set; } = "Pendiente";

    [StringLength(500)]
    public string? MotivoRechazo { get; set; }

    public DateTime FechaSolicitud { get; set; }

    public DateTime? FechaRevision { get; set; }

    public int? RevisadoPorUsuarioId { get; set; }

    public bool Activo { get; set; }

    [ForeignKey(nameof(UsuarioId))]
    public Usuario? Usuario { get; set; }

    [ForeignKey(nameof(RevisadoPorUsuarioId))]
    public Usuario? RevisadoPorUsuario { get; set; }
}