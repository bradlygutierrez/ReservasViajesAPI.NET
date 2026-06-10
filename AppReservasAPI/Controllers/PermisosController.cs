using AppReservasAPI.Context;
using AppReservasAPI.DTOs.Permisos;
using AppReservasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppReservasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")]
    public class PermisosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PermisosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Permisos
        [HttpGet]
        public async Task<IActionResult> GetPermisos(
            [FromQuery] bool? activo,
            [FromQuery] string? busqueda)
        {
            var query = _context.Permisos
                .AsNoTracking()
                .AsQueryable();

            if (activo.HasValue)
            {
                query = query.Where(p => p.Activo == activo.Value);
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var texto = busqueda.Trim();

                query = query.Where(p =>
                    p.Codigo.Contains(texto) ||
                    p.Nombre.Contains(texto) ||
                    (p.Descripcion != null && p.Descripcion.Contains(texto)));
            }

            var permisos = await query
                .Select(p => new
                {
                    p.PermisoId,
                    p.Codigo,
                    p.Nombre,
                    p.Descripcion,
                    p.Activo,
                    p.FechaCreacion,
                    Asignaciones = p.RolPantallaPermisos.Count()
                })
                .OrderBy(p => p.Codigo)
                .ToListAsync();

            return Ok(permisos);
        }

        // GET: api/Permisos/5
        [HttpGet("{permisoId:int}")]
        public async Task<IActionResult> GetPermiso(int permisoId)
        {
            var permiso = await _context.Permisos
                .AsNoTracking()
                .Where(p => p.PermisoId == permisoId)
                .Select(p => new
                {
                    p.PermisoId,
                    p.Codigo,
                    p.Nombre,
                    p.Descripcion,
                    p.Activo,
                    p.FechaCreacion,
                    Asignaciones = p.RolPantallaPermisos
                        .Where(rpp => rpp.Activo)
                        .Select(rpp => new
                        {
                            rpp.RolPantallaPermisoId,
                            rpp.RolId,
                            Rol = rpp.Rol == null ? null : rpp.Rol.Nombre,
                            rpp.PantallaId,
                            Pantalla = rpp.Pantalla == null ? null : rpp.Pantalla.Nombre,
                            Ruta = rpp.Pantalla == null ? null : rpp.Pantalla.Ruta,
                            rpp.Activo
                        })
                })
                .FirstOrDefaultAsync();

            if (permiso == null)
            {
                return NotFound("El permiso no existe.");
            }

            return Ok(permiso);
        }

        // POST: api/Permisos
        [HttpPost]
        public async Task<IActionResult> CrearPermiso([FromBody] CrearPermisoDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var codigo = dto.Codigo.Trim().ToUpper();
            var nombre = dto.Nombre.Trim();

            var codigoDuplicado = await _context.Permisos
                .AnyAsync(p => p.Codigo == codigo);

            if (codigoDuplicado)
            {
                return BadRequest("Ya existe un permiso con ese código.");
            }

            var nombreDuplicado = await _context.Permisos
                .AnyAsync(p => p.Nombre == nombre);

            if (nombreDuplicado)
            {
                return BadRequest("Ya existe un permiso con ese nombre.");
            }

            var permiso = new Permiso
            {
                Codigo = codigo,
                Nombre = nombre,
                Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim(),
                Activo = true,
                FechaCreacion = DateTime.Now
            };

            _context.Permisos.Add(permiso);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetPermiso),
                new { permisoId = permiso.PermisoId },
                new
                {
                    permiso.PermisoId,
                    permiso.Codigo,
                    permiso.Nombre,
                    permiso.Descripcion,
                    permiso.Activo,
                    permiso.FechaCreacion
                }
            );
        }

        // PUT: api/Permisos/5
        [HttpPut("{permisoId:int}")]
        public async Task<IActionResult> ActualizarPermiso(int permisoId, [FromBody] ActualizarPermisoDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var permiso = await _context.Permisos
                .FirstOrDefaultAsync(p => p.PermisoId == permisoId);

            if (permiso == null)
            {
                return NotFound("El permiso no existe.");
            }

            var codigo = dto.Codigo.Trim().ToUpper();
            var nombre = dto.Nombre.Trim();

            var codigoDuplicado = await _context.Permisos
                .AnyAsync(p => p.PermisoId != permisoId && p.Codigo == codigo);

            if (codigoDuplicado)
            {
                return BadRequest("Ya existe otro permiso con ese código.");
            }

            var nombreDuplicado = await _context.Permisos
                .AnyAsync(p => p.PermisoId != permisoId && p.Nombre == nombre);

            if (nombreDuplicado)
            {
                return BadRequest("Ya existe otro permiso con ese nombre.");
            }

            permiso.Codigo = codigo;
            permiso.Nombre = nombre;
            permiso.Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim();
            permiso.Activo = dto.Activo;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                permiso.PermisoId,
                permiso.Codigo,
                permiso.Nombre,
                permiso.Descripcion,
                permiso.Activo,
                permiso.FechaCreacion
            });
        }

        // PUT: api/Permisos/5/activar
        [HttpPut("{permisoId:int}/activar")]
        public async Task<IActionResult> ActivarPermiso(int permisoId)
        {
            var permiso = await _context.Permisos
                .FirstOrDefaultAsync(p => p.PermisoId == permisoId);

            if (permiso == null)
            {
                return NotFound("El permiso no existe.");
            }

            permiso.Activo = true;
            await _context.SaveChangesAsync();

            return Ok("Permiso activado correctamente.");
        }

        // PUT: api/Permisos/5/desactivar
        [HttpPut("{permisoId:int}/desactivar")]
        public async Task<IActionResult> DesactivarPermiso(int permisoId)
        {
            var permiso = await _context.Permisos
                .FirstOrDefaultAsync(p => p.PermisoId == permisoId);

            if (permiso == null)
            {
                return NotFound("El permiso no existe.");
            }

            permiso.Activo = false;
            await _context.SaveChangesAsync();

            return Ok("Permiso desactivado correctamente.");
        }

        // DELETE: api/Permisos/5
        [HttpDelete("{permisoId:int}")]
        public async Task<IActionResult> DeletePermiso(int permisoId)
        {
            var permiso = await _context.Permisos
                .FirstOrDefaultAsync(p => p.PermisoId == permisoId);

            if (permiso == null)
            {
                return NotFound("El permiso no existe.");
            }

            var tieneAsignaciones = await _context.RolPantallaPermisos
                .AnyAsync(rpp => rpp.PermisoId == permisoId);

            if (tieneAsignaciones)
            {
                permiso.Activo = false;
                await _context.SaveChangesAsync();

                return Ok("El permiso tiene asignaciones asociadas. No se eliminó físicamente; fue desactivado.");
            }

            _context.Permisos.Remove(permiso);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}