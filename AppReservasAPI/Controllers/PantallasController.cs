using AppReservasAPI.Context;
using AppReservasAPI.DTOs.Pantallas;
using AppReservasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppReservasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")]
    public class PantallasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PantallasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Pantallas
        [HttpGet]
        public async Task<IActionResult> GetPantallas(
            [FromQuery] bool? activo,
            [FromQuery] string? modulo,
            [FromQuery] string? busqueda)
        {
            var query = _context.Pantallas
                .AsNoTracking()
                .AsQueryable();

            if (activo.HasValue)
            {
                query = query.Where(p => p.Activo == activo.Value);
            }

            if (!string.IsNullOrWhiteSpace(modulo))
            {
                var moduloTexto = modulo.Trim();
                query = query.Where(p => p.Modulo != null && p.Modulo.Contains(moduloTexto));
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var texto = busqueda.Trim();

                query = query.Where(p =>
                    p.Nombre.Contains(texto) ||
                    p.Ruta.Contains(texto) ||
                    (p.Modulo != null && p.Modulo.Contains(texto)));
            }

            var pantallas = await query
                .Select(p => new
                {
                    p.PantallaId,
                    p.Nombre,
                    p.Ruta,
                    p.Modulo,
                    p.Icono,
                    p.Orden,
                    p.Activo,
                    p.FechaCreacion,
                    PermisosAsociados = p.RolPantallaPermisos.Count()
                })
                .OrderBy(p => p.Orden)
                .ThenBy(p => p.Nombre)
                .ToListAsync();

            return Ok(pantallas);
        }

        // GET: api/Pantallas/5
        [HttpGet("{pantallaId:int}")]
        public async Task<IActionResult> GetPantalla(int pantallaId)
        {
            var pantalla = await _context.Pantallas
                .AsNoTracking()
                .Where(p => p.PantallaId == pantallaId)
                .Select(p => new
                {
                    p.PantallaId,
                    p.Nombre,
                    p.Ruta,
                    p.Modulo,
                    p.Icono,
                    p.Orden,
                    p.Activo,
                    p.FechaCreacion,
                    PermisosPorRol = p.RolPantallaPermisos
                        .Where(rpp => rpp.Activo)
                        .Select(rpp => new
                        {
                            rpp.RolPantallaPermisoId,
                            rpp.RolId,
                            Rol = rpp.Rol == null ? null : rpp.Rol.Nombre,
                            rpp.PermisoId,
                            Permiso = rpp.Permiso == null ? null : rpp.Permiso.Nombre,
                            CodigoPermiso = rpp.Permiso == null ? null : rpp.Permiso.Codigo,
                            rpp.Activo
                        })
                })
                .FirstOrDefaultAsync();

            if (pantalla == null)
            {
                return NotFound("La pantalla no existe.");
            }

            return Ok(pantalla);
        }

        // POST: api/Pantallas
        [HttpPost]
        public async Task<IActionResult> CrearPantalla([FromBody] CrearPantallaDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var nombre = dto.Nombre.Trim();
            var ruta = dto.Ruta.Trim();

            if (!ruta.StartsWith("/"))
            {
                ruta = "/" + ruta;
            }

            var nombreDuplicado = await _context.Pantallas
                .AnyAsync(p => p.Nombre == nombre);

            if (nombreDuplicado)
            {
                return BadRequest("Ya existe una pantalla con ese nombre.");
            }

            var rutaDuplicada = await _context.Pantallas
                .AnyAsync(p => p.Ruta == ruta);

            if (rutaDuplicada)
            {
                return BadRequest("Ya existe una pantalla con esa ruta.");
            }

            var pantalla = new Pantalla
            {
                Nombre = nombre,
                Ruta = ruta,
                Modulo = string.IsNullOrWhiteSpace(dto.Modulo) ? null : dto.Modulo.Trim(),
                Icono = string.IsNullOrWhiteSpace(dto.Icono) ? null : dto.Icono.Trim(),
                Orden = dto.Orden,
                Activo = true,
                FechaCreacion = DateTime.Now
            };

            _context.Pantallas.Add(pantalla);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetPantalla),
                new { pantallaId = pantalla.PantallaId },
                new
                {
                    pantalla.PantallaId,
                    pantalla.Nombre,
                    pantalla.Ruta,
                    pantalla.Modulo,
                    pantalla.Icono,
                    pantalla.Orden,
                    pantalla.Activo,
                    pantalla.FechaCreacion
                }
            );
        }

        // PUT: api/Pantallas/5
        [HttpPut("{pantallaId:int}")]
        public async Task<IActionResult> ActualizarPantalla(int pantallaId, [FromBody] ActualizarPantallaDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var pantalla = await _context.Pantallas
                .FirstOrDefaultAsync(p => p.PantallaId == pantallaId);

            if (pantalla == null)
            {
                return NotFound("La pantalla no existe.");
            }

            var nombre = dto.Nombre.Trim();
            var ruta = dto.Ruta.Trim();

            if (!ruta.StartsWith("/"))
            {
                ruta = "/" + ruta;
            }

            var nombreDuplicado = await _context.Pantallas
                .AnyAsync(p => p.PantallaId != pantallaId && p.Nombre == nombre);

            if (nombreDuplicado)
            {
                return BadRequest("Ya existe otra pantalla con ese nombre.");
            }

            var rutaDuplicada = await _context.Pantallas
                .AnyAsync(p => p.PantallaId != pantallaId && p.Ruta == ruta);

            if (rutaDuplicada)
            {
                return BadRequest("Ya existe otra pantalla con esa ruta.");
            }

            pantalla.Nombre = nombre;
            pantalla.Ruta = ruta;
            pantalla.Modulo = string.IsNullOrWhiteSpace(dto.Modulo) ? null : dto.Modulo.Trim();
            pantalla.Icono = string.IsNullOrWhiteSpace(dto.Icono) ? null : dto.Icono.Trim();
            pantalla.Orden = dto.Orden;
            pantalla.Activo = dto.Activo;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                pantalla.PantallaId,
                pantalla.Nombre,
                pantalla.Ruta,
                pantalla.Modulo,
                pantalla.Icono,
                pantalla.Orden,
                pantalla.Activo,
                pantalla.FechaCreacion
            });
        }

        // PUT: api/Pantallas/5/activar
        [HttpPut("{pantallaId:int}/activar")]
        public async Task<IActionResult> ActivarPantalla(int pantallaId)
        {
            var pantalla = await _context.Pantallas
                .FirstOrDefaultAsync(p => p.PantallaId == pantallaId);

            if (pantalla == null)
            {
                return NotFound("La pantalla no existe.");
            }

            pantalla.Activo = true;
            await _context.SaveChangesAsync();

            return Ok("Pantalla activada correctamente.");
        }

        // PUT: api/Pantallas/5/desactivar
        [HttpPut("{pantallaId:int}/desactivar")]
        public async Task<IActionResult> DesactivarPantalla(int pantallaId)
        {
            var pantalla = await _context.Pantallas
                .FirstOrDefaultAsync(p => p.PantallaId == pantallaId);

            if (pantalla == null)
            {
                return NotFound("La pantalla no existe.");
            }

            pantalla.Activo = false;
            await _context.SaveChangesAsync();

            return Ok("Pantalla desactivada correctamente.");
        }

        // DELETE: api/Pantallas/5
        [HttpDelete("{pantallaId:int}")]
        public async Task<IActionResult> DeletePantalla(int pantallaId)
        {
            var pantalla = await _context.Pantallas
                .FirstOrDefaultAsync(p => p.PantallaId == pantallaId);

            if (pantalla == null)
            {
                return NotFound("La pantalla no existe.");
            }

            var tienePermisos = await _context.RolPantallaPermisos
                .AnyAsync(rpp => rpp.PantallaId == pantallaId);

            if (tienePermisos)
            {
                pantalla.Activo = false;
                await _context.SaveChangesAsync();

                return Ok("La pantalla tiene permisos asociados. No se eliminó físicamente; fue desactivada.");
            }

            _context.Pantallas.Remove(pantalla);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}