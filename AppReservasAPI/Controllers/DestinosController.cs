using AppReservasAPI.Context;
using AppReservasAPI.DTOs.Destinos;
using AppReservasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppReservasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DestinosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DestinosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Destinos
        [HttpGet]
        public async Task<IActionResult> GetDestinos(
            [FromQuery] bool? activo,
            [FromQuery] int? ciudadId,
            [FromQuery] int? paisId,
            [FromQuery] string? busqueda)
        {
            var query = _context.Destinos
                .AsNoTracking()
                .AsQueryable();

            if (activo.HasValue)
            {
                query = query.Where(d => d.Activo == activo.Value);
            }

            if (ciudadId.HasValue)
            {
                query = query.Where(d => d.CiudadId == ciudadId.Value);
            }

            if (paisId.HasValue)
            {
                query = query.Where(d => d.Ciudad != null && d.Ciudad.PaisId == paisId.Value);
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var texto = busqueda.Trim();

                query = query.Where(d =>
                    (d.Descripcion != null && d.Descripcion.Contains(texto)) ||
                    (d.Ciudad != null && d.Ciudad.Nombre.Contains(texto)) ||
                    (d.Ciudad != null && d.Ciudad.Pais != null && d.Ciudad.Pais.Nombre.Contains(texto)));
            }

            var destinos = await query
                .Select(d => new
                {
                    d.DestinoId,
                    d.Descripcion,
                    d.Activo,
                    d.CiudadId,
                    Ciudad = d.Ciudad == null ? null : new
                    {
                        d.Ciudad.CiudadId,
                        d.Ciudad.Nombre,
                        d.Ciudad.PaisId,
                        Pais = d.Ciudad.Pais == null ? null : d.Ciudad.Pais.Nombre
                    },
                    ViajesAsociados = d.Viajes.Count()
                })
                .OrderBy(d => d.Ciudad == null ? "" : d.Ciudad.Pais)
                .ThenBy(d => d.Ciudad == null ? "" : d.Ciudad.Nombre)
                .ToListAsync();

            return Ok(destinos);
        }

        // GET: api/Destinos/5
        [HttpGet("{destinoId:int}")]
        public async Task<IActionResult> GetDestino(int destinoId)
        {
            var destino = await _context.Destinos
                .AsNoTracking()
                .Where(d => d.DestinoId == destinoId)
                .Select(d => new
                {
                    d.DestinoId,
                    d.Descripcion,
                    d.Activo,
                    d.CiudadId,
                    Ciudad = d.Ciudad == null ? null : new
                    {
                        d.Ciudad.CiudadId,
                        d.Ciudad.Nombre,
                        d.Ciudad.PaisId,
                        Pais = d.Ciudad.Pais == null ? null : new
                        {
                            d.Ciudad.Pais.PaisId,
                            d.Ciudad.Pais.Nombre
                        }
                    },
                    Viajes = d.Viajes.Select(v => new
                    {
                        v.ViajeId,
                        v.Titulo,
                        v.Precio,
                        v.CuposTotales,
                        v.Activo
                    })
                })
                .FirstOrDefaultAsync();

            if (destino == null)
            {
                return NotFound("El destino no existe.");
            }

            return Ok(destino);
        }

        // POST: api/Destinos
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CrearDestino([FromBody] CrearDestinoDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ciudadExiste = await _context.Ciudades
                .AnyAsync(c => c.CiudadId == dto.CiudadId && c.Activo);

            if (!ciudadExiste)
            {
                return BadRequest("La ciudad no existe o no está activa.");
            }

            var descripcion = string.IsNullOrWhiteSpace(dto.Descripcion)
                ? null
                : dto.Descripcion.Trim();

            var destinoDuplicado = await _context.Destinos
                .AnyAsync(d =>
                    d.CiudadId == dto.CiudadId &&
                    d.Descripcion == descripcion);

            if (destinoDuplicado)
            {
                return BadRequest("Ya existe un destino con esa ciudad y descripción.");
            }

            var destino = new Destino
            {
                CiudadId = dto.CiudadId,
                Descripcion = descripcion,
                Activo = true
            };

            _context.Destinos.Add(destino);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetDestino),
                new { destinoId = destino.DestinoId },
                new
                {
                    destino.DestinoId,
                    destino.CiudadId,
                    destino.Descripcion,
                    destino.Activo
                }
            );
        }

        // PUT: api/Destinos/5
        [HttpPut("{destinoId:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ActualizarDestino(int destinoId, [FromBody] ActualizarDestinoDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var destino = await _context.Destinos
                .FirstOrDefaultAsync(d => d.DestinoId == destinoId);

            if (destino == null)
            {
                return NotFound("El destino no existe.");
            }

            var ciudadExiste = await _context.Ciudades
                .AnyAsync(c => c.CiudadId == dto.CiudadId && c.Activo);

            if (!ciudadExiste)
            {
                return BadRequest("La ciudad no existe o no está activa.");
            }

            var descripcion = string.IsNullOrWhiteSpace(dto.Descripcion)
                ? null
                : dto.Descripcion.Trim();

            var destinoDuplicado = await _context.Destinos
                .AnyAsync(d =>
                    d.DestinoId != destinoId &&
                    d.CiudadId == dto.CiudadId &&
                    d.Descripcion == descripcion);

            if (destinoDuplicado)
            {
                return BadRequest("Ya existe otro destino con esa ciudad y descripción.");
            }

            destino.CiudadId = dto.CiudadId;
            destino.Descripcion = descripcion;
            destino.Activo = dto.Activo;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                destino.DestinoId,
                destino.CiudadId,
                destino.Descripcion,
                destino.Activo
            });
        }

        // PUT: api/Destinos/5/activar
        [HttpPut("{destinoId:int}/activar")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ActivarDestino(int destinoId)
        {
            var destino = await _context.Destinos
                .FirstOrDefaultAsync(d => d.DestinoId == destinoId);

            if (destino == null)
            {
                return NotFound("El destino no existe.");
            }

            destino.Activo = true;
            await _context.SaveChangesAsync();

            return Ok("Destino activado correctamente.");
        }

        // PUT: api/Destinos/5/desactivar
        [HttpPut("{destinoId:int}/desactivar")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DesactivarDestino(int destinoId)
        {
            var destino = await _context.Destinos
                .FirstOrDefaultAsync(d => d.DestinoId == destinoId);

            if (destino == null)
            {
                return NotFound("El destino no existe.");
            }

            destino.Activo = false;
            await _context.SaveChangesAsync();

            return Ok("Destino desactivado correctamente.");
        }

        // DELETE: api/Destinos/5
        [HttpDelete("{destinoId:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteDestino(int destinoId)
        {
            var destino = await _context.Destinos
                .FirstOrDefaultAsync(d => d.DestinoId == destinoId);

            if (destino == null)
            {
                return NotFound("El destino no existe.");
            }

            var tieneViajes = await _context.Viajes
                .AnyAsync(v => v.DestinoId == destinoId);

            if (tieneViajes)
            {
                destino.Activo = false;
                await _context.SaveChangesAsync();

                return Ok("El destino tiene viajes asociados. No se eliminó físicamente; fue desactivado.");
            }

            _context.Destinos.Remove(destino);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}