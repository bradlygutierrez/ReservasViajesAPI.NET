using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Agentes;

public class SolicitarCertificacionDto
{
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
    public IFormFile DocumentoCedula { get; set; } = default!;

    [Required]
    public IFormFile DocumentoLicencia { get; set; } = default!;
}