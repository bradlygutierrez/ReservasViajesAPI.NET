using AppReservasAPI.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppReservasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")]
    public class HistorialEstadoReservasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HistorialEstadoReservasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/HistorialEstadoReservas
        [HttpGet]
        public async Task<IActionResult> GetHistorial(
            [FromQuery] int? reservaId,
            [FromQuery] int? usuarioId,
            [FromQuery] int? estadoNuevoId,
            [FromQuery] DateTime? fechaDesde,
            [FromQuery] DateTime? fechaHasta)
        {
            var query = _context.HistorialEstadosReserva
                .AsNoTracking()
                .AsQueryable();

            if (reservaId.HasValue)
            {
                query = query.Where(h => h.ReservaId == reservaId.Value);
            }

            if (usuarioId.HasValue)
            {
                query = query.Where(h => h.UsuarioId == usuarioId.Value);
            }

            if (estadoNuevoId.HasValue)
            {
                query = query.Where(h => h.EstadoNuevoId == estadoNuevoId.Value);
            }

            if (fechaDesde.HasValue)
            {
                query = query.Where(h => h.FechaCambio >= fechaDesde.Value);
            }

            if (fechaHasta.HasValue)
            {
                query = query.Where(h => h.FechaCambio <= fechaHasta.Value);
            }

            var historial = await query
                .Select(h => new
                {
                    h.HistorialId,
                    h.ReservaId,
                    Reserva = h.Reserva == null ? null : new
                    {
                        h.Reserva.ReservaId,
                        h.Reserva.UsuarioId,
                        UsuarioReserva = h.Reserva.Usuario == null ? null : h.Reserva.Usuario.Nombre,
                        h.Reserva.FechaReserva,
                        h.Reserva.Total
                    },
                    h.UsuarioId,
                    UsuarioCambio = h.Usuario == null ? null : new
                    {
                        h.Usuario.UsuarioId,
                        h.Usuario.Nombre,
                        h.Usuario.Email
                    },
                    h.EstadoAnteriorId,
                    EstadoAnterior = h.EstadoAnterior == null ? null : h.EstadoAnterior.Nombre,
                    h.EstadoNuevoId,
                    EstadoNuevo = h.EstadoNuevo == null ? null : h.EstadoNuevo.Nombre,
                    h.Motivo,
                    h.FechaCambio
                })
                .OrderByDescending(h => h.FechaCambio)
                .ToListAsync();

            return Ok(historial);
        }

        // GET: api/HistorialEstadoReservas/5
        [HttpGet("{historialId:long}")]
        public async Task<IActionResult> GetHistorialPorId(long historialId)
        {
            var historial = await _context.HistorialEstadosReserva
                .AsNoTracking()
                .Where(h => h.HistorialId == historialId)
                .Select(h => new
                {
                    h.HistorialId,
                    h.ReservaId,
                    Reserva = h.Reserva == null ? null : new
                    {
                        h.Reserva.ReservaId,
                        h.Reserva.UsuarioId,
                        UsuarioReserva = h.Reserva.Usuario == null ? null : h.Reserva.Usuario.Nombre,
                        h.Reserva.FechaReserva,
                        h.Reserva.CantidadPersonas,
                        h.Reserva.Total
                    },
                    h.UsuarioId,
                    UsuarioCambio = h.Usuario == null ? null : new
                    {
                        h.Usuario.UsuarioId,
                        h.Usuario.Nombre,
                        h.Usuario.Email
                    },
                    h.EstadoAnteriorId,
                    EstadoAnterior = h.EstadoAnterior == null ? null : h.EstadoAnterior.Nombre,
                    h.EstadoNuevoId,
                    EstadoNuevo = h.EstadoNuevo == null ? null : h.EstadoNuevo.Nombre,
                    h.Motivo,
                    h.FechaCambio
                })
                .FirstOrDefaultAsync();

            if (historial == null)
            {
                return NotFound("El registro de historial no existe.");
            }

            return Ok(historial);
        }

        // GET: api/HistorialEstadoReservas/reserva/5
        [HttpGet("reserva/{reservaId:int}")]
        public async Task<IActionResult> GetHistorialPorReserva(int reservaId)
        {
            var reservaExiste = await _context.Reservas
                .AnyAsync(r => r.ReservaId == reservaId);

            if (!reservaExiste)
            {
                return NotFound("La reserva no existe.");
            }

            var historial = await _context.HistorialEstadosReserva
                .AsNoTracking()
                .Where(h => h.ReservaId == reservaId)
                .Select(h => new
                {
                    h.HistorialId,
                    h.ReservaId,
                    h.EstadoAnteriorId,
                    EstadoAnterior = h.EstadoAnterior == null ? null : h.EstadoAnterior.Nombre,
                    h.EstadoNuevoId,
                    EstadoNuevo = h.EstadoNuevo == null ? null : h.EstadoNuevo.Nombre,
                    h.UsuarioId,
                    UsuarioCambio = h.Usuario == null ? null : h.Usuario.Nombre,
                    h.Motivo,
                    h.FechaCambio
                })
                .OrderByDescending(h => h.FechaCambio)
                .ToListAsync();

            return Ok(historial);
        }

        // GET: api/HistorialEstadoReservas/usuario/5
        [HttpGet("usuario/{usuarioId:int}")]
        public async Task<IActionResult> GetHistorialPorUsuario(int usuarioId)
        {
            var usuarioExiste = await _context.Usuarios
                .AnyAsync(u => u.UsuarioId == usuarioId);

            if (!usuarioExiste)
            {
                return NotFound("El usuario no existe.");
            }

            var historial = await _context.HistorialEstadosReserva
                .AsNoTracking()
                .Where(h => h.UsuarioId == usuarioId)
                .Select(h => new
                {
                    h.HistorialId,
                    h.ReservaId,
                    h.EstadoAnteriorId,
                    EstadoAnterior = h.EstadoAnterior == null ? null : h.EstadoAnterior.Nombre,
                    h.EstadoNuevoId,
                    EstadoNuevo = h.EstadoNuevo == null ? null : h.EstadoNuevo.Nombre,
                    h.Motivo,
                    h.FechaCambio
                })
                .OrderByDescending(h => h.FechaCambio)
                .ToListAsync();

            return Ok(historial);
        }
    }
}