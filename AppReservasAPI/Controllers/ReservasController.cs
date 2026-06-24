using AppReservasAPI.Context;
using AppReservasAPI.DTOs.Reservas;
using AppReservasAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;


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

        // DELETE: api/Reservas/5
        // No se recomienda borrar reservas físicamente.
        [HttpDelete("{reservaId:int}")]
        public IActionResult DeleteReserva(int reservaId)
        {
            return BadRequest("No se recomienda eliminar reservas físicamente. Usá PUT /api/Reservas/{reservaId}/estado para cambiarla a Cancelada.");
        }
    }
}