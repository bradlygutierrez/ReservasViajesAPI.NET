using System.ComponentModel.DataAnnotations;

namespace AppReservasAPI.DTOs.Publicaciones;

public class CambiarEstadoPublicacionDto
{
    public bool Activo { get; set; }

    [StringLength(1000)]
    public string? Motivo { get; set; }
}