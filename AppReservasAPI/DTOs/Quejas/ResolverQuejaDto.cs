using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Quejas;

public class ResolverQuejaDto
{
    [Required]
    [StringLength(30)]
    public string Estado { get; set; } = "Resuelta";

    [StringLength(1000)]
    public string? RespuestaAdmin { get; set; }
}
