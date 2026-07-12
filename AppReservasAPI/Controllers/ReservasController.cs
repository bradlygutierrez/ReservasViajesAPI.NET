using AppReservasAPI.Context;
using AppReservasAPI.DTOs.Reservas;
using AppReservasAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AppReservasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReservasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReservasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Reservas
        [HttpGet]
        public async Task<IActionResult> GetReservas()
        {
            var reservas = await _context.Reservas
                .AsNoTracking()
                .Select(r => new
                {
                    r.ReservaId,
                    r.UsuarioId,
                    Usuario = r.Usuario == null ? null : new
                    {
                        r.Usuario.UsuarioId,
                        r.Usuario.Nombre,
                        r.Usuario.Email
                    },
                    r.DisponibilidadId,
                    Disponibilidad = r.Disponibilidad == null ? null : new
                    {
                        r.Disponibilidad.DisponibilidadId,
                        r.Disponibilidad.Fecha,
                        r.Disponibilidad.FechaRetorno,
                        r.Disponibilidad.CuposTotales,
                        r.Disponibilidad.CuposDisponibles,
                        Viaje = r.Disponibilidad.Viaje == null ? null : new
                        {
                            r.Disponibilidad.Viaje.ViajeId,
                            r.Disponibilidad.Viaje.Titulo,
                            r.Disponibilidad.Viaje.Precio
                        }
                    },
                    r.EstadoReservaId,
                    Estado = r.EstadoReserva == null ? null : r.EstadoReserva.Nombre,
                    r.FechaReserva,
                    r.CantidadPersonas,
                    r.PrecioUnitario,
                    r.Total,
                    r.FechaActualizacion,
                    r.Notas
                })
                .ToListAsync();

            return Ok(reservas);
        }

        // GET: api/Reservas/5
        [HttpGet("{reservaId:int}")]
        public async Task<IActionResult> GetReserva(int reservaId)
        {
            var reserva = await _context.Reservas
                .AsNoTracking()
                .Where(r => r.ReservaId == reservaId)
                .Select(r => new
                {
                    r.ReservaId,
                    r.UsuarioId,
                    Usuario = r.Usuario == null ? null : new
                    {
                        r.Usuario.UsuarioId,
                        r.Usuario.Nombre,
                        r.Usuario.Email,
                        r.Usuario.Telefono
                    },
                    r.DisponibilidadId,
                    Disponibilidad = r.Disponibilidad == null ? null : new
                    {
                        r.Disponibilidad.DisponibilidadId,
                        r.Disponibilidad.Fecha,
                        r.Disponibilidad.FechaRetorno,
                        r.Disponibilidad.CuposTotales,
                        r.Disponibilidad.CuposDisponibles,
                        Viaje = r.Disponibilidad.Viaje == null ? null : new
                        {
                            r.Disponibilidad.Viaje.ViajeId,
                            r.Disponibilidad.Viaje.Titulo,
                            r.Disponibilidad.Viaje.Precio
                        }
                    },
                    r.EstadoReservaId,
                    Estado = r.EstadoReserva == null ? null : r.EstadoReserva.Nombre,
                    r.FechaReserva,
                    r.CantidadPersonas,
                    r.PrecioUnitario,
                    r.Total,
                    r.FechaActualizacion,
                    r.Notas,
                    Pasajeros = r.PasajerosReserva.Select(p => new
                    {
                        p.PasajeroId,
                        p.NombreCompleto,
                        p.Documento,
                        p.FechaNacimiento
                    }),
                    Pagos = r.Pagos.Select(p => new
                    {
                        p.PagoId,
                        p.Monto,
                        p.Referencia,
                        p.FechaPago,
                        EstadoPago = p.EstadoPago == null ? null : p.EstadoPago.Nombre,
                        MetodoPago = p.MetodoPago == null ? null : p.MetodoPago.Nombre
                    }),
                    Historial = r.HistorialEstadosReserva.Select(h => new
                    {
                        h.HistorialId,
                        h.EstadoAnteriorId,
                        EstadoAnterior = h.EstadoAnterior == null ? null : h.EstadoAnterior.Nombre,
                        h.EstadoNuevoId,
                        EstadoNuevo = h.EstadoNuevo == null ? null : h.EstadoNuevo.Nombre,
                        h.UsuarioId,
                        Usuario = h.Usuario == null ? null : h.Usuario.Nombre,
                        h.Motivo,
                        h.FechaCambio
                    })
                })
                .FirstOrDefaultAsync();

            if (reserva == null)
            {
                return NotFound("La reserva no existe.");
            }

            return Ok(reserva);
        }

        // POST: api/Reservas
        [HttpPost]
        public async Task<IActionResult> CrearReserva([FromBody] CrearReservaDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (dto.CantidadPersonas <= 0)
            {
                return BadRequest("La cantidad de personas debe ser mayor que cero.");
            }

            if (dto.Pasajeros.Count != dto.CantidadPersonas)
            {
                return BadRequest("La cantidad de pasajeros debe coincidir con la cantidad de personas de la reserva.");
            }

            var usuarioExiste = await _context.Usuarios
                .AnyAsync(u => u.UsuarioId == dto.UsuarioId && u.Activo);

            if (!usuarioExiste)
            {
                return BadRequest("El usuario no existe o no está activo.");
            }

            var disponibilidad = await _context.Disponibilidades
                .Include(d => d.Viaje)
                .FirstOrDefaultAsync(d => d.DisponibilidadId == dto.DisponibilidadId);

            if (disponibilidad == null)
            {
                return NotFound("La disponibilidad no existe.");
            }

            if (!disponibilidad.Activo)
            {
                return BadRequest("La disponibilidad no está activa.");
            }

            if (disponibilidad.Viaje == null || !disponibilidad.Viaje.Activo)
            {
                return BadRequest("El viaje asociado no existe o no está activo.");
            }

            if (disponibilidad.Fecha.Date < DateTime.Today)
            {
                return BadRequest("No se puede reservar un viaje cuya fecha de salida ya pasó.");
            }

            var yaTieneReservaActiva = await _context.Reservas
                .Include(r => r.EstadoReserva)
                .AnyAsync(r =>
                    r.UsuarioId == dto.UsuarioId &&
                    r.DisponibilidadId == dto.DisponibilidadId &&
                    r.EstadoReserva != null &&
                    !r.EstadoReserva.Nombre.Equals("Cancelada"));

            if (yaTieneReservaActiva)
            {
                return BadRequest("Ya tenés una reserva activa para esta fecha de viaje.");
            }

            if (disponibilidad.CuposDisponibles < dto.CantidadPersonas)
            {
                return BadRequest($"No hay cupos suficientes. Cupos disponibles: {disponibilidad.CuposDisponibles}.");
            }

            var estadoPendiente = await _context.EstadosReserva
                .FirstOrDefaultAsync(e => e.Nombre == "Pendiente" && e.Activo);

            if (estadoPendiente == null)
            {
                return StatusCode(500, "No existe el estado de reserva 'Pendiente'.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var precioUnitario = disponibilidad.Viaje.Precio;
                var total = precioUnitario * dto.CantidadPersonas;

                var reserva = new Reserva
                {
                    UsuarioId = dto.UsuarioId,
                    DisponibilidadId = dto.DisponibilidadId,
                    EstadoReservaId = estadoPendiente.EstadoReservaId,
                    FechaReserva = DateTime.Now,
                    CantidadPersonas = dto.CantidadPersonas,
                    PrecioUnitario = precioUnitario,
                    FechaActualizacion = DateTime.Now,
                    Notas = dto.Notas
                };

                _context.Reservas.Add(reserva);

                disponibilidad.CuposDisponibles -= dto.CantidadPersonas;

                await _context.SaveChangesAsync();

                foreach (var pasajeroDto in dto.Pasajeros)
                {
                    var pasajero = new PasajeroReserva
                    {
                        ReservaId = reserva.ReservaId,
                        NombreCompleto = pasajeroDto.NombreCompleto,
                        Documento = pasajeroDto.Documento,
                        FechaNacimiento = pasajeroDto.FechaNacimiento
                    };

                    _context.PasajerosReserva.Add(pasajero);
                }

                var historial = new HistorialEstadoReserva
                {
                    ReservaId = reserva.ReservaId,
                    UsuarioId = dto.UsuarioId,
                    EstadoAnteriorId = null,
                    EstadoNuevoId = estadoPendiente.EstadoReservaId,
                    Motivo = "Reserva creada.",
                    FechaCambio = DateTime.Now
                };

                _context.HistorialEstadosReserva.Add(historial);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(
                    nameof(GetReserva),
                    new { reservaId = reserva.ReservaId },
                    new
                    {
                        reserva.ReservaId,
                        reserva.UsuarioId,
                        reserva.DisponibilidadId,
                        reserva.EstadoReservaId,
                        Estado = estadoPendiente.Nombre,
                        reserva.CantidadPersonas,
                        reserva.PrecioUnitario,
                        reserva.Total,
                        reserva.FechaReserva
                    }
                );
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // PUT: api/Reservas/5/estado
        [HttpPut("{reservaId:int}/estado")]
        public async Task<IActionResult> CambiarEstadoReserva(int reservaId, [FromBody] CambiarEstadoReservaDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var reserva = await _context.Reservas
                .Include(r => r.Disponibilidad)
                .Include(r => r.EstadoReserva)
                .FirstOrDefaultAsync(r => r.ReservaId == reservaId);

            if (reserva == null)
            {
                return NotFound("La reserva no existe.");
            }

            var estadoNuevo = await _context.EstadosReserva
                .FirstOrDefaultAsync(e => e.EstadoReservaId == dto.EstadoNuevoId && e.Activo);

            if (estadoNuevo == null)
            {
                return BadRequest("El estado nuevo no existe o no está activo.");
            }

            if (reserva.EstadoReservaId == dto.EstadoNuevoId)
            {
                return BadRequest("La reserva ya tiene ese estado.");
            }

            if (dto.UsuarioId.HasValue)
            {
                var usuarioExiste = await _context.Usuarios
                    .AnyAsync(u => u.UsuarioId == dto.UsuarioId.Value && u.Activo);

                if (!usuarioExiste)
                {
                    return BadRequest("El usuario que realiza el cambio no existe o no está activo.");
                }
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var estadoAnteriorId = reserva.EstadoReservaId;
                var estadoAnteriorNombre = reserva.EstadoReserva?.Nombre ?? string.Empty;
                var estadoNuevoNombre = estadoNuevo.Nombre;

                var anteriorEraCancelada = estadoAnteriorNombre.Equals("Cancelada", StringComparison.OrdinalIgnoreCase);
                var nuevoEsCancelada = estadoNuevoNombre.Equals("Cancelada", StringComparison.OrdinalIgnoreCase);

                if (nuevoEsCancelada && !anteriorEraCancelada)
                {
                    if (reserva.Disponibilidad != null)
                    {
                        reserva.Disponibilidad.CuposDisponibles += reserva.CantidadPersonas;
                    }
                }

                if (anteriorEraCancelada && !nuevoEsCancelada)
                {
                    if (reserva.Disponibilidad == null)
                    {
                        return BadRequest("La reserva no tiene disponibilidad asociada.");
                    }

                    if (reserva.Disponibilidad.CuposDisponibles < reserva.CantidadPersonas)
                    {
                        return BadRequest("No hay cupos suficientes para reactivar esta reserva.");
                    }

                    reserva.Disponibilidad.CuposDisponibles -= reserva.CantidadPersonas;
                }

                reserva.EstadoReservaId = estadoNuevo.EstadoReservaId;
                reserva.FechaActualizacion = DateTime.Now;

                var historial = new HistorialEstadoReserva
                {
                    ReservaId = reserva.ReservaId,
                    UsuarioId = dto.UsuarioId,
                    EstadoAnteriorId = estadoAnteriorId,
                    EstadoNuevoId = estadoNuevo.EstadoReservaId,
                    Motivo = dto.Motivo,
                    FechaCambio = DateTime.Now
                };

                _context.HistorialEstadosReserva.Add(historial);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    reserva.ReservaId,
                    EstadoAnteriorId = estadoAnteriorId,
                    EstadoNuevoId = estadoNuevo.EstadoReservaId,
                    EstadoNuevo = estadoNuevo.Nombre,
                    reserva.FechaActualizacion
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // POST: api/Reservas/6/reagendar
        [HttpPost("{reservaId:int}/reagendar")]
        public async Task<IActionResult> ReagendarReserva(
            int reservaId,
            [FromBody] ReagendarReservaDto dto)
        {
            var usuarioAutenticadoId = ObtenerUsuarioId();

            if (usuarioAutenticadoId == null)
            {
                return Unauthorized("Token inválido.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var reserva = await _context.Reservas
                .Include(r => r.Disponibilidad)
                    .ThenInclude(d => d!.Viaje)
                .Include(r => r.EstadoReserva)
                .FirstOrDefaultAsync(r =>
                    r.ReservaId == reservaId);

            if (reserva == null)
            {
                return NotFound("La reserva no existe.");
            }

            /*
             * Solo el propietario de la reserva o un administrador
             * puede realizar el reagendamiento.
             */
            if (!EsAdministrador() &&
                reserva.UsuarioId != usuarioAutenticadoId.Value)
            {
                return Forbid();
            }

            if (reserva.EstadoReserva == null ||
                !reserva.EstadoReserva.Nombre.Equals(
                    "Pausada",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message =
                        "Solo se pueden reagendar reservas que estén pausadas.",
                    codigo = "RESERVA_NO_PAUSADA"
                });
            }

            if (reserva.Disponibilidad == null)
            {
                return BadRequest(
                    "La reserva no tiene una disponibilidad asociada.");
            }

            if (reserva.DisponibilidadId ==
                dto.NuevaDisponibilidadId)
            {
                return BadRequest(new
                {
                    message =
                        "La nueva disponibilidad debe ser diferente de la actual.",
                    codigo = "MISMA_DISPONIBILIDAD"
                });
            }

            var nuevaDisponibilidad =
                await _context.Disponibilidades
                    .Include(d => d.Viaje)
                    .FirstOrDefaultAsync(d =>
                        d.DisponibilidadId ==
                        dto.NuevaDisponibilidadId);

            if (nuevaDisponibilidad == null)
            {
                return NotFound(
                    "La nueva disponibilidad no existe.");
            }

            if (!nuevaDisponibilidad.Activo)
            {
                return BadRequest(new
                {
                    message =
                        "La nueva disponibilidad no está activa.",
                    codigo = "DISPONIBILIDAD_INACTIVA"
                });
            }

            if (nuevaDisponibilidad.Viaje == null ||
                !nuevaDisponibilidad.Viaje.Activo)
            {
                return BadRequest(new
                {
                    message =
                        "El viaje de la nueva disponibilidad no está activo.",
                    codigo = "VIAJE_NUEVO_INACTIVO"
                });
            }

            /*
             * Por seguridad, la nueva fecha debe pertenecer
             * al mismo viaje.
             */
            if (nuevaDisponibilidad.ViajeId !=
                reserva.Disponibilidad.ViajeId)
            {
                return BadRequest(new
                {
                    message =
                        "La nueva disponibilidad debe pertenecer al mismo viaje.",
                    codigo = "VIAJE_DIFERENTE"
                });
            }

            if (nuevaDisponibilidad.Fecha.Date <
                DateTime.Today)
            {
                return BadRequest(new
                {
                    message =
                        "No se puede reagendar para una fecha pasada.",
                    codigo = "FECHA_REAGENDAMIENTO_PASADA"
                });
            }

            if (nuevaDisponibilidad.CuposDisponibles <
                reserva.CantidadPersonas)
            {
                return BadRequest(new
                {
                    message =
                        $"No hay cupos suficientes. Cupos disponibles: " +
                        $"{nuevaDisponibilidad.CuposDisponibles}.",
                    codigo = "CUPOS_INSUFICIENTES"
                });
            }

            var estadoReagendada =
                await _context.EstadosReserva
                    .FirstOrDefaultAsync(e =>
                        e.Nombre == "Reagendada" &&
                        e.Activo);

            if (estadoReagendada == null)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message =
                            "No existe el estado de reserva 'Reagendada'.",
                        codigo =
                            "ESTADO_REAGENDADA_NO_CONFIGURADO"
                    });
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var ahora = DateTime.Now;

                var disponibilidadAnterior =
                    reserva.Disponibilidad;

                var disponibilidadAnteriorId =
                    reserva.DisponibilidadId;

                var estadoAnteriorId =
                    reserva.EstadoReservaId;

                /*
                 * La reserva pausada conservó sus cupos en la
                 * disponibilidad original.
                 *
                 * Por eso, al moverla:
                 * 1. Se devuelven los cupos a la fecha anterior.
                 * 2. Se descuentan de la nueva fecha.
                 */
                disponibilidadAnterior.CuposDisponibles +=
                    reserva.CantidadPersonas;

                /*
                 * Protección para que no supere el total,
                 * en caso de datos antiguos inconsistentes.
                 */
                if (disponibilidadAnterior.CuposDisponibles >
                    disponibilidadAnterior.CuposTotales)
                {
                    disponibilidadAnterior.CuposDisponibles =
                        disponibilidadAnterior.CuposTotales;
                }

                nuevaDisponibilidad.CuposDisponibles -=
                    reserva.CantidadPersonas;

                reserva.DisponibilidadId =
                    nuevaDisponibilidad.DisponibilidadId;

                reserva.EstadoReservaId =
                    estadoReagendada.EstadoReservaId;

                reserva.FechaActualizacion = ahora;

                /*
                 * Registrar el movimiento de disponibilidad.
                 */
                var reagendamiento =
                    new ReagendamientoReserva
                    {
                        ReservaId = reserva.ReservaId,
                        DisponibilidadAnteriorId =
                            disponibilidadAnteriorId,
                        DisponibilidadNuevaId =
                            nuevaDisponibilidad.DisponibilidadId,
                        SolicitadoPorUsuarioId =
                            usuarioAutenticadoId.Value,
                        Motivo = string.IsNullOrWhiteSpace(
                            dto.Motivo)
                            ? "Reagendamiento solicitado por el cliente."
                            : dto.Motivo.Trim(),
                        FechaReagendamiento = ahora
                    };

                _context.ReagendamientosReserva.Add(
                    reagendamiento);

                /*
                 * Registrar el cambio de estado.
                 */
                var historial =
                    new HistorialEstadoReserva
                    {
                        ReservaId = reserva.ReservaId,
                        UsuarioId =
                            usuarioAutenticadoId.Value,
                        EstadoAnteriorId =
                            estadoAnteriorId,
                        EstadoNuevoId =
                            estadoReagendada.EstadoReservaId,
                        Motivo = string.IsNullOrWhiteSpace(
                            dto.Motivo)
                            ? "Reserva reagendada."
                            : dto.Motivo.Trim(),
                        FechaCambio = ahora
                    };

                _context.HistorialEstadosReserva.Add(
                    historial);

                /*
                 * Crear una notificación para el cliente.
                 */
                var notificacion =
                    new Notificacion
                    {
                        UsuarioId = reserva.UsuarioId,
                        ViajeId =
                            nuevaDisponibilidad.ViajeId,
                        ReservaId = reserva.ReservaId,
                        Titulo =
                            "Tu reserva fue reagendada",
                        Mensaje =
                            $"Tu reserva fue trasladada a la fecha " +
                            $"{nuevaDisponibilidad.Fecha:dd/MM/yyyy}" +
                            (nuevaDisponibilidad.FechaRetorno.HasValue
                                ? $" con retorno el " +
                                  $"{nuevaDisponibilidad.FechaRetorno.Value:dd/MM/yyyy}."
                                : "."),
                        Tipo = "ReservaReagendada",
                        Leida = false,
                        FechaCreacion = ahora
                    };

                _context.Notificaciones.Add(
                    notificacion);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message =
                        "La reserva fue reagendada correctamente.",
                    reserva.ReservaId,
                    DisponibilidadAnteriorId =
                        disponibilidadAnteriorId,
                    NuevaDisponibilidadId =
                        nuevaDisponibilidad.DisponibilidadId,
                    NuevaFecha =
                        nuevaDisponibilidad.Fecha,
                    NuevaFechaRetorno =
                        nuevaDisponibilidad.FechaRetorno,
                    EstadoAnteriorId =
                        estadoAnteriorId,
                    EstadoNuevoId =
                        estadoReagendada.EstadoReservaId,
                    EstadoNuevo =
                        estadoReagendada.Nombre,
                    reserva.FechaActualizacion
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // POST: api/Reservas/6/reembolso
        [HttpPost("{reservaId:int}/reembolso")]
        public async Task<IActionResult> SolicitarReembolso(
            int reservaId,
            [FromBody] SolicitarReembolsoDto dto)
        {
            var usuarioAutenticadoId = ObtenerUsuarioId();

            if (usuarioAutenticadoId == null)
            {
                return Unauthorized("Token inválido.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var reserva = await _context.Reservas
                .Include(r => r.Disponibilidad)
                    .ThenInclude(d => d!.Viaje)
                .Include(r => r.EstadoReserva)
                .Include(r => r.Pagos)
                    .ThenInclude(p => p.EstadoPago)
                .FirstOrDefaultAsync(r =>
                    r.ReservaId == reservaId);

            if (reserva == null)
            {
                return NotFound("La reserva no existe.");
            }

            /*
             * Solo el propietario o un administrador puede
             * solicitar el reembolso.
             */
            if (!EsAdministrador() &&
                reserva.UsuarioId != usuarioAutenticadoId.Value)
            {
                return Forbid();
            }

            /*
             * El reembolso se habilita cuando la reserva
             * está pausada.
             */
            if (reserva.EstadoReserva == null ||
(
            !reserva.EstadoReserva.Nombre.Equals(
                 "Pausada",
             StringComparison.OrdinalIgnoreCase)
                &&
            !reserva.EstadoReserva.Nombre.Equals(
                 "Reagendada",
             StringComparison.OrdinalIgnoreCase)
))
            {
                return BadRequest(new
                {
                    message = "Solo se puede solicitar un reembolso cuando la reserva esté pausada o reagendada.",
                    codigo = "RESERVA_NO_VALIDA"
                });
            }

            var solicitudExistente =
                await _context.SolicitudesReembolso
                    .AnyAsync(s =>
                        s.ReservaId == reservaId &&
                        (
                            s.Estado == "Pendiente" ||
                            s.Estado == "Aprobada" ||
                            s.Estado == "Procesada"
                        ));

            if (solicitudExistente)
            {
                return BadRequest(new
                {
                    message =
                        "La reserva ya tiene una solicitud de reembolso activa.",
                    codigo = "REEMBOLSO_DUPLICADO"
                });
            }

            /*
             * Busca cuánto se ha pagado realmente.
             *
             * Ajusta los nombres de estados si tu tabla
             * EstadosPago usa otros valores.
             */
            var montoPagado = reserva.Pagos
                .Where(p =>
                    p.EstadoPago != null &&
                    (
                        p.EstadoPago.Nombre == "Pagado" ||
                        p.EstadoPago.Nombre == "Aprobado" ||
                        p.EstadoPago.Nombre == "Completado"
                    ))
                .Sum(p => p.Monto);

            /*
             * Durante pruebas puede que la reserva no tenga pagos.
             * Para producción, puedes exigir montoPagado > 0.
             */
            var montoSolicitado = montoPagado > 0
                ? montoPagado
                : reserva.Total;

            if (montoSolicitado <= 0)
            {
                return BadRequest(new
                {
                    message =
                        "No existe un monto válido para solicitar el reembolso.",
                    codigo = "MONTO_REEMBOLSO_INVALIDO"
                });
            }

            var estadoReembolsoSolicitado =
                await _context.EstadosReserva
                    .FirstOrDefaultAsync(e =>
                        e.Nombre == "Reembolso solicitado" &&
                        e.Activo);

            if (estadoReembolsoSolicitado == null)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message =
                            "No existe el estado de reserva 'Reembolso solicitado'.",
                        codigo =
                            "ESTADO_REEMBOLSO_NO_CONFIGURADO"
                    });
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var ahora = DateTime.Now;
                var estadoAnteriorId =
                    reserva.EstadoReservaId;

                var solicitud = new SolicitudReembolso
                {
                    ReservaId = reserva.ReservaId,
                    UsuarioId = reserva.UsuarioId,
                    Motivo = string.IsNullOrWhiteSpace(dto.Motivo)
                        ? "Solicitud de reembolso por viaje pausado."
                        : dto.Motivo.Trim(),
                    Estado = "Pendiente",
                    MontoSolicitado = montoSolicitado,
                    MontoAprobado = null,
                    FechaSolicitud = ahora,
                    FechaResolucion = null,
                    RevisadoPorUsuarioId = null,
                    ObservacionResolucion = null
                };

                _context.SolicitudesReembolso.Add(
                    solicitud);

                reserva.EstadoReservaId =
                    estadoReembolsoSolicitado.EstadoReservaId;

                reserva.FechaActualizacion = ahora;

                var historial =
                    new HistorialEstadoReserva
                    {
                        ReservaId = reserva.ReservaId,
                        UsuarioId =
                            usuarioAutenticadoId.Value,
                        EstadoAnteriorId =
                            estadoAnteriorId,
                        EstadoNuevoId =
                            estadoReembolsoSolicitado.EstadoReservaId,
                        Motivo = solicitud.Motivo,
                        FechaCambio = ahora
                    };

                _context.HistorialEstadosReserva.Add(
                    historial);

                var notificacion =
                    new Notificacion
                    {
                        UsuarioId = reserva.UsuarioId,
                        ViajeId =
                            reserva.Disponibilidad?.ViajeId,
                        ReservaId = reserva.ReservaId,
                        Titulo =
                            "Solicitud de reembolso enviada",
                        Mensaje =
                            $"Tu solicitud de reembolso por " +
                            $"{montoSolicitado:N2} fue enviada y está pendiente de revisión.",
                        Tipo = "ReembolsoSolicitado",
                        Leida = false,
                        FechaCreacion = ahora
                    };

                _context.Notificaciones.Add(
                    notificacion);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message =
                        "La solicitud de reembolso fue enviada correctamente.",
                    solicitud.SolicitudReembolsoId,
                    solicitud.ReservaId,
                    solicitud.Estado,
                    solicitud.MontoSolicitado,
                    solicitud.FechaSolicitud
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // DELETE: api/Reservas/5
        // No se recomienda borrar reservas físicamente.
        [HttpDelete("{reservaId:int}")]
        public IActionResult DeleteReserva(int reservaId)
        {
            return BadRequest("No se recomienda eliminar reservas físicamente. Usá PUT /api/Reservas/{reservaId}/estado para cambiarla a Cancelada.");
        }

        private int? ObtenerUsuarioId()
        {
            var valor = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            return int.TryParse(valor, out var usuarioId)
                ? usuarioId
                : null;
        }

        private bool EsAdministrador()
        {
            return User.IsInRole("Admin");
        }
    }
}