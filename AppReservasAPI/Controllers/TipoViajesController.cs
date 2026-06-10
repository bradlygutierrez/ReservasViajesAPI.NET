using AppReservasAPI.Context;
using AppReservasAPI.DTOs.TipoViajes;
using AppReservasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppReservasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TipoViajesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TipoViajesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/TipoViajes
        [HttpGet]
        public async Task<IActionResult> GetTiposViaje(
            [FromQuery] bool? activo,
            [FromQuery] string? busqueda)
        {
            var query = _context.TiposViaje
                .AsNoTracking()
                .AsQueryable();

            if (activo.HasValue)
            {
                query = query.Where(t => t.Activo == activo.Value);
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var texto = busqueda.Trim();

                query = query.Where(t =>
                    t.Nombre.Contains(texto) ||
                    (t.Descripcion != null && t.Descripcion.Contains(texto)));
            }

            var tipos = await query
                .Select(t => new
                {
                    t.TipoViajeId,
                    t.Nombre,
                    t.Descripcion,
                    t.Activo,
                    ViajesAsociados = t.Viajes.Count()
                })
                .OrderBy(t => t.Nombre)
                .ToListAsync();

            return Ok(tipos);
        }

        // GET: api/TipoViajes/5
        [HttpGet("{tipoViajeId:int}")]
        public async Task<IActionResult> GetTipoViaje(int tipoViajeId)
        {
            var tipoViaje = await _context.TiposViaje
                .AsNoTracking()
                .Where(t => t.TipoViajeId == tipoViajeId)
                .Select(t => new
                {
                    t.TipoViajeId,
                    t.Nombre,
                    t.Descripcion,
                    t.Activo,
                    Viajes = t.Viajes.Select(v => new
                    {
                        v.ViajeId,
                        v.Titulo,
                        v.Precio,
                        v.CuposTotales,
                        v.Activo
                    })
                })
                .FirstOrDefaultAsync();

            if (tipoViaje == null)
            {
                return NotFound("El tipo de viaje no existe.");
            }

            return Ok(tipoViaje);
        }

        // POST: api/TipoViajes
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CrearTipoViaje([FromBody] CrearTipoViajeDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var nombre = dto.Nombre.Trim();

            var nombreDuplicado = await _context.TiposViaje
                .AnyAsync(t => t.Nombre == nombre);

            if (nombreDuplicado)
            {
                return BadRequest("Ya existe un tipo de viaje con ese nombre.");
            }

            var tipoViaje = new TipoViaje
            {
                Nombre = nombre,
                Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim(),
                Activo = true
            };

            _context.TiposViaje.Add(tipoViaje);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetTipoViaje),
                new { tipoViajeId = tipoViaje.TipoViajeId },
                new
                {
                    tipoViaje.TipoViajeId,
                    tipoViaje.Nombre,
                    tipoViaje.Descripcion,
                    tipoViaje.Activo
                }
            );
        }

        // PUT: api/TipoViajes/5
        [HttpPut("{tipoViajeId:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ActualizarTipoViaje(int tipoViajeId, [FromBody] ActualizarTipoViajeDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tipoViaje = await _context.TiposViaje
                .FirstOrDefaultAsync(t => t.TipoViajeId == tipoViajeId);

            if (tipoViaje == null)
            {
                return NotFound("El tipo de viaje no existe.");
            }

            var nombre = dto.Nombre.Trim();

            var nombreDuplicado = await _context.TiposViaje
                .AnyAsync(t => t.TipoViajeId != tipoViajeId && t.Nombre == nombre);

            if (nombreDuplicado)
            {
                return BadRequest("Ya existe otro tipo de viaje con ese nombre.");
            }

            tipoViaje.Nombre = nombre;
            tipoViaje.Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim();
            tipoViaje.Activo = dto.Activo;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                tipoViaje.TipoViajeId,
                tipoViaje.Nombre,
                tipoViaje.Descripcion,
                tipoViaje.Activo
            });
        }

        // PUT: api/TipoViajes/5/activar
        [HttpPut("{tipoViajeId:int}/activar")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ActivarTipoViaje(int tipoViajeId)
        {
            var tipoViaje = await _context.TiposViaje
                .FirstOrDefaultAsync(t => t.TipoViajeId == tipoViajeId);

            if (tipoViaje == null)
            {
                return NotFound("El tipo de viaje no existe.");
            }

            tipoViaje.Activo = true;
            await _context.SaveChangesAsync();

            return Ok("Tipo de viaje activado correctamente.");
        }

        // PUT: api/TipoViajes/5/desactivar
        [HttpPut("{tipoViajeId:int}/desactivar")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DesactivarTipoViaje(int tipoViajeId)
        {
            var tipoViaje = await _context.TiposViaje
                .FirstOrDefaultAsync(t => t.TipoViajeId == tipoViajeId);

            if (tipoViaje == null)
            {
                return NotFound("El tipo de viaje no existe.");
            }

            tipoViaje.Activo = false;
            await _context.SaveChangesAsync();

            return Ok("Tipo de viaje desactivado correctamente.");
        }

        // DELETE: api/TipoViajes/5
        [HttpDelete("{tipoViajeId:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteTipoViaje(int tipoViajeId)
        {
            var tipoViaje = await _context.TiposViaje
                .FirstOrDefaultAsync(t => t.TipoViajeId == tipoViajeId);

            if (tipoViaje == null)
            {
                return NotFound("El tipo de viaje no existe.");
            }

            var tieneViajes = await _context.Viajes
                .AnyAsync(v => v.TipoViajeId == tipoViajeId);

            if (tieneViajes)
            {
                tipoViaje.Activo = false;
                await _context.SaveChangesAsync();

                return Ok("El tipo de viaje tiene viajes asociados. No se eliminó físicamente; fue desactivado.");
            }

            _context.TiposViaje.Remove(tipoViaje);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}