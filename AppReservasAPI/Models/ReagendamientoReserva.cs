using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppReservasAPI.Models;

[Table("ReagendamientosReserva", Schema = "viajes")]
public class ReagendamientoReserva
{
    [Key]
    public int ReagendamientoReservaId { get; set; }

    public int ReservaId { get; set; }

    public int DisponibilidadAnteriorId { get; set; }

    public int DisponibilidadNuevaId { get; set; }

    public int SolicitadoPorUsuarioId { get; set; }

    [StringLength(500)]
    public string? Motivo { get; set; }

    public DateTime FechaReagendamiento { get; set; }

    public Reserva? Reserva { get; set; }

    public Disponibilidad? DisponibilidadAnterior { get; set; }

    public Disponibilidad? DisponibilidadNueva { get; set; }

    public Usuario? SolicitadoPorUsuario { get; set; }
}