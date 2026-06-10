using AppReservasAPI.Context;
using AppReservasAPI.DTOs.Ciudades;
using AppReservasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppReservasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CiudadesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CiudadesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Ciudades
        [HttpGet]
        public async Task<IActionResult> GetCiudades(
            [FromQuery] bool? activo,
            [FromQuery] int? paisId,
            [FromQuery] string? busqueda)
        {
            var query = _context.Ciudades
                .AsNoTracking()
                .AsQueryable();

            if (activo.HasValue)
            {
                query = query.Where(c => c.Activo == activo.Value);
            }

            if (paisId.HasValue)
            {
                query = query.Where(c => c.PaisId == paisId.Value);
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var texto = busqueda.Trim();

                query = query.Where(c =>
                    c.Nombre.Contains(texto) ||
                    (c.Pais != null && c.Pais.Nombre.Contains(texto)));
            }

            var ciudades = await query
                .Select(c => new
                {
                    c.CiudadId,
                    c.PaisId,
                    Pais = c.Pais == null ? null : c.Pais.Nombre,
                    c.Nombre,
                    c.Activo,
                    DestinosAsociados = c.Destinos.Count()
                })
                .OrderBy(c => c.Pais)
                .ThenBy(c => c.Nombre)
                .ToListAsync();

            return Ok(ciudades);
        }

        // GET: api/Ciudades/5
        [HttpGet("{ciudadId:int}")]
        public async Task<IActionResult> GetCiudad(int ciudadId)
        {
            var ciudad = await _context.Ciudades
                .AsNoTracking()
                .Where(c => c.CiudadId == ciudadId)
                .Select(c => new
                {
                    c.CiudadId,
                    c.PaisId,
                    Pais = c.Pais == null ? null : new
                    {
                        c.Pais.PaisId,
                        c.Pais.Nombre,
                        c.Pais.Activo
                    },
                    c.Nombre,
                    c.Activo,
                    Destinos = c.Destinos.Select(d => new
                    {
                        d.DestinoId,
                        d.Descripcion,
                        d.Activo
                    })
                })
                .FirstOrDefaultAsync();

            if (ciudad == null)
            {
                return NotFound("La ciudad no existe.");
            }

            return Ok(ciudad);
        }

        // GET: api/Ciudades/pais/5
        [HttpGet("pais/{paisId:int}")]
        public async Task<IActionResult> GetCiudadesPorPais(int paisId)
        {
            var paisExiste = await _context.Paises
                .AnyAsync(p => p.PaisId == paisId);

            if (!paisExiste)
            {
                return NotFound("El país no existe.");
            }

            var ciudades = await _context.Ciudades
                .AsNoTracking()
                .Where(c => c.PaisId == paisId)
                .Select(c => new
                {
                    c.CiudadId,
                    c.PaisId,
                    c.Nombre,
                    c.Activo
                })
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            return Ok(ciudades);
        }

        // POST: api/Ciudades
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CrearCiudad([FromBody] CrearCiudadDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var paisExiste = await _context.Paises
                .AnyAsync(p => p.PaisId == dto.PaisId && p.Activo);

            if (!paisExiste)
            {
                return BadRequest("El país no existe o no está activo.");
            }

            var nombre = dto.Nombre.Trim();

            var ciudadDuplicada = await _context.Ciudades
                .AnyAsync(c => c.PaisId == dto.PaisId && c.Nombre == nombre);

            if (ciudadDuplicada)
            {
                return BadRequest("Ya existe una ciudad con ese nombre en ese país.");
            }

            var ciudad = new Ciudad
            {
                PaisId = dto.PaisId,
                Nombre = nombre,
                Activo = true
            };

            _context.Ciudades.Add(ciudad);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCiudad),
                new { ciudadId = ciudad.CiudadId },
                new
                {
                    ciudad.CiudadId,
                    ciudad.PaisId,
                    ciudad.Nombre,
                    ciudad.Activo
                }
            );
        }

        // PUT: api/Ciudades/5
        [HttpPut("{ciudadId:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ActualizarCiudad(int ciudadId, [FromBody] ActualizarCiudadDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ciudad = await _context.Ciudades
                .FirstOrDefaultAsync(c => c.CiudadId == ciudadId);

            if (ciudad == null)
            {
                return NotFound("La ciudad no existe.");
            }

            var paisExiste = await _context.Paises
                .AnyAsync(p => p.PaisId == dto.PaisId && p.Activo);

            if (!paisExiste)
            {
                return BadRequest("El país no existe o no está activo.");
            }

            var nombre = dto.Nombre.Trim();

            var ciudadDuplicada = await _context.Ciudades
                .AnyAsync(c =>
                    c.CiudadId != ciudadId &&
                    c.PaisId == dto.PaisId &&
                    c.Nombre == nombre);

            if (ciudadDuplicada)
            {
                return BadRequest("Ya existe otra ciudad con ese nombre en ese país.");
            }

            ciudad.PaisId = dto.PaisId;
            ciudad.Nombre = nombre;
            ciudad.Activo = dto.Activo;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                ciudad.CiudadId,
                ciudad.PaisId,
                ciudad.Nombre,
                ciudad.Activo
            });
        }

        // PUT: api/Ciudades/5/activar
        [HttpPut("{ciudadId:int}/activar")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ActivarCiudad(int ciudadId)
        {
            var ciudad = await _context.Ciudades
                .FirstOrDefaultAsync(c => c.CiudadId == ciudadId);

            if (ciudad == null)
            {
                return NotFound("La ciudad no existe.");
            }

            ciudad.Activo = true;
            await _context.SaveChangesAsync();

            return Ok("Ciudad activada correctamente.");
        }

        // PUT: api/Ciudades/5/desactivar
        [HttpPut("{ciudadId:int}/desactivar")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DesactivarCiudad(int ciudadId)
        {
            var ciudad = await _context.Ciudades
                .FirstOrDefaultAsync(c => c.CiudadId == ciudadId);

            if (ciudad == null)
            {
                return NotFound("La ciudad no existe.");
            }

            ciudad.Activo = false;
            await _context.SaveChangesAsync();

            return Ok("Ciudad desactivada correctamente.");
        }

        // DELETE: api/Ciudades/5
        [HttpDelete("{ciudadId:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteCiudad(int ciudadId)
        {
            var ciudad = await _context.Ciudades
                .FirstOrDefaultAsync(c => c.CiudadId == ciudadId);

            if (ciudad == null)
            {
                return NotFound("La ciudad no existe.");
            }

            var tieneDestinos = await _context.Destinos
                .AnyAsync(d => d.CiudadId == ciudadId);

            if (tieneDestinos)
            {
                ciudad.Activo = false;
                await _context.SaveChangesAsync();

                return Ok("La ciudad tiene destinos asociados. No se eliminó físicamente; fue desactivada.");
            }

            _context.Ciudades.Remove(ciudad);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}