using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Reembolsos;

public class ResolverReembolsoDto
{
    [Required]
    public string Estado { get; set; } = string.Empty;

    [Range(0.01, 999999999)]
    public decimal? MontoAprobado { get; set; }

    [StringLength(1000)]
    public string? Observacion { get; set; }
}