using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Agentes;

public class RevisarCertificacionDto
{
    [Required]
    public string Estado { get; set; } = string.Empty;

    [StringLength(500)]
    public string? MotivoRechazo { get; set; }
}