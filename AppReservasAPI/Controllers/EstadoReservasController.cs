using AppReservasAPI.Context;
using AppReservasAPI.DTOs.EstadosReserva;
using AppReservasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppReservasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EstadosReservaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EstadosReservaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/EstadosReserva
        [HttpGet]
        public async Task<IActionResult> GetEstadosReserva(
            [FromQuery] bool? activo,
            [FromQuery] string? busqueda)
        {
            var query = _context.EstadosReserva
                .AsNoTracking()
                .AsQueryable();

            if (activo.HasValue)
            {
                query = query.Where(e => e.Activo == activo.Value);
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var texto = busqueda.Trim();

                query = query.Where(e =>
                    e.Nombre.Contains(texto) ||
                    (e.Descripcion != null && e.Descripcion.Contains(texto)));
            }

            var estados = await query
                .Select(e => new
                {
                    e.EstadoReservaId,
                    e.Nombre,
                    e.Descripcion,
                    e.Activo,
                    e.FechaCreacion,
                    ReservasAsociadas = e.Reservas.Count()
                })
                .OrderBy(e => e.Nombre)
                .ToListAsync();

            return Ok(estados);
        }

        // GET: api/EstadosReserva/5
        [HttpGet("{estadoReservaId:int}")]
        public async Task<IActionResult> GetEstadoReserva(int estadoReservaId)
        {
            var estado = await _context.EstadosReserva
                .AsNoTracking()
                .Where(e => e.EstadoReservaId == estadoReservaId)
                .Select(e => new
                {
                    e.EstadoReservaId,
                    e.Nombre,
                    e.Descripcion,
                    e.Activo,
                    e.FechaCreacion,
                    Reservas = e.Reservas.Select(r => new
                    {
                        r.ReservaId,
                        r.UsuarioId,
                        Usuario = r.Usuario == null ? null : r.Usuario.Nombre,
                        r.DisponibilidadId,
                        r.FechaReserva,
                        r.CantidadPersonas,
                        r.Total
                    })
                })
                .FirstOrDefaultAsync();

            if (estado == null)
            {
                return NotFound("El estado de reserva no existe.");
            }

            return Ok(estado);
        }

        // POST: api/EstadosReserva
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CrearEstadoReserva([FromBody] CrearEstadoReservaDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var nombre = dto.Nombre.Trim();

            var nombreDuplicado = await _context.EstadosReserva
                .AnyAsync(e => e.Nombre == nombre);

            if (nombreDuplicado)
            {
                return BadRequest("Ya existe un estado de reserva con ese nombre.");
            }

            var estado = new EstadoReserva
            {
                Nombre = nombre,
                Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim(),
                Activo = true,
                FechaCreacion = DateTime.Now
            };

            _context.EstadosReserva.Add(estado);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEstadoReserva),
                new { estadoReservaId = estado.EstadoReservaId },
                new
                {
                    estado.EstadoReservaId,
                    estado.Nombre,
                    estado.Descripcion,
                    estado.Activo,
                    estado.FechaCreacion
                }
            );
        }

        // PUT: api/EstadosReserva/5
        [HttpPut("{estadoReservaId:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ActualizarEstadoReserva(int estadoReservaId, [FromBody] ActualizarEstadoReservaDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var estado = await _context.EstadosReserva
                .FirstOrDefaultAsync(e => e.EstadoReservaId == estadoReservaId);

            if (estado == null)
            {
                return NotFound("El estado de reserva no existe.");
            }

            var nombre = dto.Nombre.Trim();

            var nombreDuplicado = await _context.EstadosReserva
                .AnyAsync(e => e.EstadoReservaId != estadoReservaId && e.Nombre == nombre);

            if (nombreDuplicado)
            {
                return BadRequest("Ya existe otro estado de reserva con ese nombre.");
            }

            estado.Nombre = nombre;
            estado.Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim();
            estado.Activo = dto.Activo;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                estado.EstadoReservaId,
                estado.Nombre,
                estado.Descripcion,
                estado.Activo,
                estado.FechaCreacion
            });
        }

        // PUT: api/EstadosReserva/5/activar
        [HttpPut("{estadoReservaId:int}/activar")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ActivarEstadoReserva(int estadoReservaId)
        {
            var estado = await _context.EstadosReserva
                .FirstOrDefaultAsync(e => e.EstadoReservaId == estadoReservaId);

            if (estado == null)
            {
                return NotFound("El estado de reserva no existe.");
            }

            estado.Activo = true;
            await _context.SaveChangesAsync();

            return Ok("Estado de reserva activado correctamente.");
        }

        // PUT: api/EstadosReserva/5/desactivar
        [HttpPut("{estadoReservaId:int}/desactivar")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DesactivarEstadoReserva(int estadoReservaId)
        {
            var estado = await _context.EstadosReserva
                .FirstOrDefaultAsync(e => e.EstadoReservaId == estadoReservaId);

            if (estado == null)
            {
                return NotFound("El estado de reserva no existe.");
            }

            estado.Activo = false;
            await _context.SaveChangesAsync();

            return Ok("Estado de reserva desactivado correctamente.");
        }

        // DELETE: api/EstadosReserva/5
        [HttpDelete("{estadoReservaId:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteEstadoReserva(int estadoReservaId)
        {
            var estado = await _context.EstadosReserva
                .FirstOrDefaultAsync(e => e.EstadoReservaId == estadoReservaId);

            if (estado == null)
            {
                return NotFound("El estado de reserva no existe.");
            }

            var tieneReservas = await _context.Reservas
                .AnyAsync(r => r.EstadoReservaId == estadoReservaId);

            var tieneHistorial = await _context.HistorialEstadosReserva
                .AnyAsync(h =>
                    h.EstadoAnteriorId == estadoReservaId ||
                    h.EstadoNuevoId == estadoReservaId);

            if (tieneReservas || tieneHistorial)
            {
                estado.Activo = false;
                await _context.SaveChangesAsync();

                return Ok("El estado de reserva tiene reservas o historial asociado. No se eliminó físicamente; fue desactivado.");
            }

            _context.EstadosReserva.Remove(estado);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}