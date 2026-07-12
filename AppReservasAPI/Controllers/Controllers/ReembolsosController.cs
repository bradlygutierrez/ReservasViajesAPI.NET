using AppReservasAPI.Context;
using AppReservasAPI.DTOs.Reembolsos;
using AppReservasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AppReservasAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class ReembolsosController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReembolsosController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Reembolsos
    [HttpGet]
    public async Task<IActionResult> ObtenerSolicitudes(
        [FromQuery] string? estado)
    {
        var query = _context.SolicitudesReembolso
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
        {
            var estadoBuscado = estado.Trim();

            query = query.Where(s =>
                s.Estado == estadoBuscado);
        }

        var solicitudes = await query
            .OrderByDescending(s => s.FechaSolicitud)
            .Select(s => new
            {
                s.SolicitudReembolsoId,
                s.ReservaId,
                s.UsuarioId,

                Usuario = s.Usuario == null
                    ? null
                    : new
                    {
                        s.Usuario.UsuarioId,
                        s.Usuario.Nombre,
                        s.Usuario.Email
                    },

                Reserva = s.Reserva == null
                    ? null
                    : new
                    {
                        s.Reserva.ReservaId,
                        s.Reserva.CantidadPersonas,
                        s.Reserva.PrecioUnitario,
                        s.Reserva.Total,

                        Estado = s.Reserva.EstadoReserva == null
                            ? null
                            : s.Reserva.EstadoReserva.Nombre,

                        Disponibilidad =
                            s.Reserva.Disponibilidad == null
                                ? null
                                : new
                                {
                                    s.Reserva
                                        .Disponibilidad
                                        .DisponibilidadId,

                                    s.Reserva
                                        .Disponibilidad
                                        .Fecha,

                                    s.Reserva
                                        .Disponibilidad
                                        .FechaRetorno,

                                    Viaje =
                                        s.Reserva
                                            .Disponibilidad
                                            .Viaje == null
                                            ? null
                                            : new
                                            {
                                                s.Reserva
                                                    .Disponibilidad
                                                    .Viaje
                                                    .ViajeId,

                                                s.Reserva
                                                    .Disponibilidad
                                                    .Viaje
                                                    .Titulo
                                            }
                                }
                    },

                s.Motivo,
                s.Estado,
                s.MontoSolicitado,
                s.MontoAprobado,
                s.FechaSolicitud,
                s.FechaResolucion,
                s.RevisadoPorUsuarioId,
                s.ObservacionResolucion
            })
            .ToListAsync();

        return Ok(solicitudes);
    }

    // GET: api/Reembolsos/1
    [HttpGet("{solicitudId:int}")]
    public async Task<IActionResult> ObtenerSolicitud(
        int solicitudId)
    {
        var solicitud = await _context.SolicitudesReembolso
            .AsNoTracking()
            .Where(s =>
                s.SolicitudReembolsoId == solicitudId)
            .Select(s => new
            {
                s.SolicitudReembolsoId,
                s.ReservaId,
                s.UsuarioId,

                Usuario = s.Usuario == null
                    ? null
                    : new
                    {
                        s.Usuario.UsuarioId,
                        s.Usuario.Nombre,
                        s.Usuario.Email,
                        s.Usuario.Telefono
                    },

                Reserva = s.Reserva == null
                    ? null
                    : new
                    {
                        s.Reserva.ReservaId,
                        s.Reserva.CantidadPersonas,
                        s.Reserva.PrecioUnitario,
                        s.Reserva.Total,

                        Estado = s.Reserva.EstadoReserva == null
                            ? null
                            : s.Reserva.EstadoReserva.Nombre,

                        Disponibilidad =
                            s.Reserva.Disponibilidad == null
                                ? null
                                : new
                                {
                                    s.Reserva
                                        .Disponibilidad
                                        .DisponibilidadId,

                                    s.Reserva
                                        .Disponibilidad
                                        .Fecha,

                                    s.Reserva
                                        .Disponibilidad
                                        .FechaRetorno,

                                    Viaje =
                                        s.Reserva
                                            .Disponibilidad
                                            .Viaje == null
                                            ? null
                                            : new
                                            {
                                                s.Reserva
                                                    .Disponibilidad
                                                    .Viaje
                                                    .ViajeId,

                                                s.Reserva
                                                    .Disponibilidad
                                                    .Viaje
                                                    .Titulo
                                            }
                                }
                    },

                s.Motivo,
                s.Estado,
                s.MontoSolicitado,
                s.MontoAprobado,
                s.FechaSolicitud,
                s.FechaResolucion,
                s.RevisadoPorUsuarioId,

                RevisadoPor =
                    s.RevisadoPorUsuario == null
                        ? null
                        : new
                        {
                            s.RevisadoPorUsuario.UsuarioId,
                            s.RevisadoPorUsuario.Nombre
                        },

                s.ObservacionResolucion
            })
            .FirstOrDefaultAsync();

        if (solicitud == null)
        {
            return NotFound(
                "La solicitud de reembolso no existe.");
        }

        return Ok(solicitud);
    }

    // PUT: api/Reembolsos/1/resolver
    [HttpPut("{solicitudId:int}/resolver")]
    public async Task<IActionResult> ResolverSolicitud(
        int solicitudId,
        [FromBody] ResolverReembolsoDto dto)
    {
        var administradorId = ObtenerUsuarioId();

        if (administradorId == null)
        {
            return Unauthorized("Token inválido.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var estadosPermitidos = new[]
        {
            "Aprobada",
            "Rechazada",
            "Procesada"
        };

        var estadoNormalizado =
            estadosPermitidos.FirstOrDefault(e =>
                e.Equals(
                    dto.Estado.Trim(),
                    StringComparison.OrdinalIgnoreCase));

        if (estadoNormalizado == null)
        {
            return BadRequest(new
            {
                message =
                    "El estado debe ser Aprobada, Rechazada o Procesada.",
                codigo = "ESTADO_REEMBOLSO_INVALIDO"
            });
        }

        var solicitud = await _context
            .SolicitudesReembolso
            .Include(s => s.Reserva)
                .ThenInclude(r => r!.Disponibilidad)
            .Include(s => s.Reserva)
                .ThenInclude(r => r!.EstadoReserva)
            .FirstOrDefaultAsync(s =>
                s.SolicitudReembolsoId == solicitudId);

        if (solicitud == null)
        {
            return NotFound(
                "La solicitud de reembolso no existe.");
        }

        if (solicitud.Reserva == null)
        {
            return BadRequest(
                "La solicitud no tiene una reserva asociada.");
        }

        if (solicitud.Estado.Equals(
                "Procesada",
                StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message =
                    "La solicitud ya fue procesada.",
                codigo = "REEMBOLSO_YA_PROCESADO"
            });
        }

        if (estadoNormalizado == "Aprobada")
        {
            if (!solicitud.Estado.Equals(
                    "Pendiente",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(
                    "Solo una solicitud pendiente puede aprobarse.");
            }
        }

        if (estadoNormalizado == "Rechazada")
        {
            if (!solicitud.Estado.Equals(
                    "Pendiente",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(
                    "Solo una solicitud pendiente puede rechazarse.");
            }
        }

        if (estadoNormalizado == "Procesada")
        {
            if (!solicitud.Estado.Equals(
                    "Aprobada",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(
                    "La solicitud debe estar aprobada antes de procesarse.");
            }
        }

        if (estadoNormalizado is "Aprobada" or "Procesada")
        {
            if (!dto.MontoAprobado.HasValue ||
                dto.MontoAprobado.Value <= 0)
            {
                return BadRequest(
                    "Debe indicar un monto aprobado válido.");
            }

            if (dto.MontoAprobado.Value >
                solicitud.MontoSolicitado)
            {
                return BadRequest(
                    "El monto aprobado no puede superar el monto solicitado.");
            }
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var ahora = DateTime.Now;
            var reserva = solicitud.Reserva;

            if (estadoNormalizado == "Aprobada")
            {
                solicitud.Estado = "Aprobada";
                solicitud.MontoAprobado =
                    dto.MontoAprobado;
                solicitud.FechaResolucion = ahora;
                solicitud.RevisadoPorUsuarioId =
                    administradorId.Value;
                solicitud.ObservacionResolucion =
                    LimpiarTexto(dto.Observacion);

                _context.Notificaciones.Add(
                    new Notificacion
                    {
                        UsuarioId = solicitud.UsuarioId,
                        ViajeId =
                            reserva.Disponibilidad?.ViajeId,
                        ReservaId = reserva.ReservaId,
                        Titulo = "Reembolso aprobado",
                        Mensaje =
                            $"Tu solicitud de reembolso fue aprobada por " +
                            $"{solicitud.MontoAprobado:N2}. " +
                            "La devolución está pendiente de procesamiento.",
                        Tipo = "ReembolsoAprobado",
                        Leida = false,
                        FechaCreacion = ahora
                    });
            }

            if (estadoNormalizado == "Rechazada")
            {
                var estadoPausada = await _context
                    .EstadosReserva
                    .FirstOrDefaultAsync(e =>
                        e.Nombre == "Pausada" &&
                        e.Activo);

                if (estadoPausada == null)
                {
                    return StatusCode(
                        StatusCodes
                            .Status500InternalServerError,
                        "No existe el estado de reserva 'Pausada'.");
                }

                var estadoAnteriorId =
                    reserva.EstadoReservaId;

                solicitud.Estado = "Rechazada";
                solicitud.MontoAprobado = null;
                solicitud.FechaResolucion = ahora;
                solicitud.RevisadoPorUsuarioId =
                    administradorId.Value;
                solicitud.ObservacionResolucion =
                    LimpiarTexto(dto.Observacion);

                reserva.EstadoReservaId =
                    estadoPausada.EstadoReservaId;
                reserva.FechaActualizacion = ahora;

                _context.HistorialEstadosReserva.Add(
                    new HistorialEstadoReserva
                    {
                        ReservaId = reserva.ReservaId,
                        UsuarioId =
                            administradorId.Value,
                        EstadoAnteriorId =
                            estadoAnteriorId,
                        EstadoNuevoId =
                            estadoPausada.EstadoReservaId,
                        Motivo =
                            string.IsNullOrWhiteSpace(
                                dto.Observacion)
                                ? "Solicitud de reembolso rechazada."
                                : dto.Observacion.Trim(),
                        FechaCambio = ahora
                    });

                _context.Notificaciones.Add(
                    new Notificacion
                    {
                        UsuarioId = solicitud.UsuarioId,
                        ViajeId =
                            reserva.Disponibilidad?.ViajeId,
                        ReservaId = reserva.ReservaId,
                        Titulo =
                            "Solicitud de reembolso rechazada",
                        Mensaje =
                            string.IsNullOrWhiteSpace(
                                dto.Observacion)
                                ? "Tu solicitud de reembolso fue rechazada."
                                : $"Tu solicitud de reembolso fue rechazada. Motivo: {dto.Observacion.Trim()}",
                        Tipo = "ReembolsoRechazado",
                        Leida = false,
                        FechaCreacion = ahora
                    });
            }

            if (estadoNormalizado == "Procesada")
            {
                var estadoReembolsada = await _context
                    .EstadosReserva
                    .FirstOrDefaultAsync(e =>
                        e.Nombre == "Reembolsada" &&
                        e.Activo);

                if (estadoReembolsada == null)
                {
                    return StatusCode(
                        StatusCodes
                            .Status500InternalServerError,
                        "No existe el estado de reserva 'Reembolsada'.");
                }

                var estadoAnteriorId =
                    reserva.EstadoReservaId;

                solicitud.Estado = "Procesada";
                solicitud.MontoAprobado =
                    dto.MontoAprobado;
                solicitud.FechaResolucion = ahora;
                solicitud.RevisadoPorUsuarioId =
                    administradorId.Value;
                solicitud.ObservacionResolucion =
                    LimpiarTexto(dto.Observacion);

                reserva.EstadoReservaId =
                    estadoReembolsada.EstadoReservaId;
                reserva.FechaActualizacion = ahora;

                var disponibilidad =
                    reserva.Disponibilidad;

                if (disponibilidad != null)
                {
                    disponibilidad.CuposDisponibles +=
                        reserva.CantidadPersonas;

                    if (disponibilidad.CuposDisponibles >
                        disponibilidad.CuposTotales)
                    {
                        disponibilidad.CuposDisponibles =
                            disponibilidad.CuposTotales;
                    }
                }

                _context.HistorialEstadosReserva.Add(
                    new HistorialEstadoReserva
                    {
                        ReservaId = reserva.ReservaId,
                        UsuarioId =
                            administradorId.Value,
                        EstadoAnteriorId =
                            estadoAnteriorId,
                        EstadoNuevoId =
                            estadoReembolsada.EstadoReservaId,
                        Motivo =
                            string.IsNullOrWhiteSpace(
                                dto.Observacion)
                                ? "Reembolso procesado."
                                : dto.Observacion.Trim(),
                        FechaCambio = ahora
                    });

                _context.Notificaciones.Add(
                    new Notificacion
                    {
                        UsuarioId = solicitud.UsuarioId,
                        ViajeId =
                            disponibilidad?.ViajeId,
                        ReservaId = reserva.ReservaId,
                        Titulo = "Reembolso procesado",
                        Mensaje =
                            $"Tu reembolso por " +
                            $"{solicitud.MontoAprobado:N2} " +
                            "fue procesado correctamente.",
                        Tipo = "ReembolsoProcesado",
                        Leida = false,
                        FechaCreacion = ahora
                    });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                message =
                    $"La solicitud fue marcada como {solicitud.Estado}.",
                solicitud.SolicitudReembolsoId,
                solicitud.ReservaId,
                solicitud.Estado,
                solicitud.MontoSolicitado,
                solicitud.MontoAprobado,
                solicitud.FechaResolucion,
                solicitud.RevisadoPorUsuarioId,
                solicitud.ObservacionResolucion
            });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private int? ObtenerUsuarioId()
    {
        var valor = User.FindFirst(
            ClaimTypes.NameIdentifier)?.Value;

        return int.TryParse(
            valor,
            out var usuarioId)
            ? usuarioId
            : null;
    }

    private static string? LimpiarTexto(
        string? texto)
    {
        return string.IsNullOrWhiteSpace(texto)
            ? null
            : texto.Trim();
    }
}