using AppReservasAPI.Context;
using AppReservasAPI.DTOs.Paises;
using AppReservasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppReservasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaisesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PaisesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Paises
        [HttpGet]
        public async Task<IActionResult> GetPaises(
            [FromQuery] bool? activo,
            [FromQuery] string? busqueda)
        {
            var query = _context.Paises
                .AsNoTracking()
                .AsQueryable();

            if (activo.HasValue)
            {
                query = query.Where(p => p.Activo == activo.Value);
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var texto = busqueda.Trim();
                query = query.Where(p => p.Nombre.Contains(texto));
            }

            var paises = await query
                .Select(p => new
                {
                    p.PaisId,
                    p.Nombre,
                    p.Activo,
                    CiudadesAsociadas = p.Ciudades.Count()
                })
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            return Ok(paises);
        }

        // GET: api/Paises/5
        [HttpGet("{paisId:int}")]
        public async Task<IActionResult> GetPais(int paisId)
        {
            var pais = await _context.Paises
                .AsNoTracking()
                .Where(p => p.PaisId == paisId)
                .Select(p => new
                {
                    p.PaisId,
                    p.Nombre,
                    p.Activo,
                    Ciudades = p.Ciudades
                        .OrderBy(c => c.Nombre)
                        .Select(c => new
                        {
                            c.CiudadId,
                            c.Nombre,
                            c.Activo,
                            DestinosAsociados = c.Destinos.Count()
                        })
                })
                .FirstOrDefaultAsync();

            if (pais == null)
            {
                return NotFound("El país no existe.");
            }

            return Ok(pais);
        }

        // POST: api/Paises
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CrearPais([FromBody] CrearPaisDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var nombre = dto.Nombre.Trim();

            var paisDuplicado = await _context.Paises
                .AnyAsync(p => p.Nombre == nombre);

            if (paisDuplicado)
            {
                return BadRequest("Ya existe un país con ese nombre.");
            }

            var pais = new Pais
            {
                Nombre = nombre,
                Activo = true
            };

            _context.Paises.Add(pais);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetPais),
                new { paisId = pais.PaisId },
                new
                {
                    pais.PaisId,
                    pais.Nombre,
                    pais.Activo
                }
            );
        }

        // PUT: api/Paises/5
        [HttpPut("{paisId:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ActualizarPais(int paisId, [FromBody] ActualizarPaisDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var pais = await _context.Paises
                .FirstOrDefaultAsync(p => p.PaisId == paisId);

            if (pais == null)
            {
                return NotFound("El país no existe.");
            }

            var nombre = dto.Nombre.Trim();

            var paisDuplicado = await _context.Paises
                .AnyAsync(p => p.PaisId != paisId && p.Nombre == nombre);

            if (paisDuplicado)
            {
                return BadRequest("Ya existe otro país con ese nombre.");
            }

            pais.Nombre = nombre;
            pais.Activo = dto.Activo;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                pais.PaisId,
                pais.Nombre,
                pais.Activo
            });
        }

        // PUT: api/Paises/5/activar
        [HttpPut("{paisId:int}/activar")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ActivarPais(int paisId)
        {
            var pais = await _context.Paises
                .FirstOrDefaultAsync(p => p.PaisId == paisId);

            if (pais == null)
            {
                return NotFound("El país no existe.");
            }

            pais.Activo = true;
            await _context.SaveChangesAsync();

            return Ok("País activado correctamente.");
        }

        // PUT: api/Paises/5/desactivar
        [HttpPut("{paisId:int}/desactivar")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DesactivarPais(int paisId)
        {
            var pais = await _context.Paises
                .FirstOrDefaultAsync(p => p.PaisId == paisId);

            if (pais == null)
            {
                return NotFound("El país no existe.");
            }

            pais.Activo = false;
            await _context.SaveChangesAsync();

            return Ok("País desactivado correctamente.");
        }

        // DELETE: api/Paises/5
        [HttpDelete("{paisId:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeletePais(int paisId)
        {
            var pais = await _context.Paises
                .FirstOrDefaultAsync(p => p.PaisId == paisId);

            if (pais == null)
            {
                return NotFound("El país no existe.");
            }

            var tieneCiudades = await _context.Ciudades
                .AnyAsync(c => c.PaisId == paisId);

            if (tieneCiudades)
            {
                pais.Activo = false;
                await _context.SaveChangesAsync();

                return Ok("El país tiene ciudades asociadas. No se eliminó físicamente; fue desactivado.");
            }

            _context.Paises.Remove(pais);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}