using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Publicaciones;

public class CrearPublicacionViajeDto
{
    [Required]
    [StringLength(150)]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Descripcion { get; set; }

    [Required]
    public int TipoViajeId { get; set; }

    [Required]
    [StringLength(100)]
    public string Pais { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Ciudad { get; set; } = string.Empty;

    [StringLength(500)]
    public string? DestinoDescripcion { get; set; }

    [Range(0.01, 9999999)]
    public decimal Precio { get; set; }

    [Range(1, 10000)]
    public int CuposTotales { get; set; }

    [Required]
    public DateTime FechaSalida { get; set; }

    public DateTime? FechaRetorno { get; set; }

    public IFormFile? Imagen { get; set; }
}
