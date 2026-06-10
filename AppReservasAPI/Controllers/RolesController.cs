using AppReservasAPI.Context;
using AppReservasAPI.DTOs.Roles;
using AppReservasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppReservasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")]
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RolesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Roles
        [HttpGet]
        public async Task<IActionResult> GetRoles([FromQuery] string? busqueda)
        {
            var query = _context.Roles
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var texto = busqueda.Trim();
                query = query.Where(r => r.Nombre.Contains(texto));
            }

            var roles = await query
                .Select(r => new
                {
                    r.RolId,
                    r.Nombre,
                    UsuariosAsociados = r.Usuarios.Count(),
                    PermisosAsociados = r.RolPantallaPermisos.Count()
                })
                .OrderBy(r => r.Nombre)
                .ToListAsync();

            return Ok(roles);
        }

        // GET: api/Roles/5
        [HttpGet("{rolId:int}")]
        public async Task<IActionResult> GetRol(int rolId)
        {
            var rol = await _context.Roles
                .AsNoTracking()
                .Where(r => r.RolId == rolId)
                .Select(r => new
                {
                    r.RolId,
                    r.Nombre,
                    Usuarios = r.Usuarios.Select(u => new
                    {
                        u.UsuarioId,
                        u.Nombre,
                        u.Email,
                        u.Activo
                    }),
                    Permisos = r.RolPantallaPermisos
                        .Where(rpp => rpp.Activo)
                        .Select(rpp => new
                        {
                            rpp.RolPantallaPermisoId,
                            rpp.PantallaId,
                            Pantalla = rpp.Pantalla == null ? null : rpp.Pantalla.Nombre,
                            Ruta = rpp.Pantalla == null ? null : rpp.Pantalla.Ruta,
                            rpp.PermisoId,
                            Permiso = rpp.Permiso == null ? null : rpp.Permiso.Nombre,
                            CodigoPermiso = rpp.Permiso == null ? null : rpp.Permiso.Codigo
                        })
                })
                .FirstOrDefaultAsync();

            if (rol == null)
            {
                return NotFound("El rol no existe.");
            }

            return Ok(rol);
        }

        // POST: api/Roles
        [HttpPost]
        public async Task<IActionResult> CrearRol([FromBody] CrearRolDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var nombre = dto.Nombre.Trim();

            var existeRol = await _context.Roles
                .AnyAsync(r => r.Nombre == nombre);

            if (existeRol)
            {
                return BadRequest("Ya existe un rol con ese nombre.");
            }

            var rol = new Rol
            {
                Nombre = nombre
            };

            _context.Roles.Add(rol);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetRol),
                new { rolId = rol.RolId },
                new
                {
                    rol.RolId,
                    rol.Nombre
                }
            );
        }

        // PUT: api/Roles/5
        [HttpPut("{rolId:int}")]
        public async Task<IActionResult> ActualizarRol(int rolId, [FromBody] ActualizarRolDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var rol = await _context.Roles
                .FirstOrDefaultAsync(r => r.RolId == rolId);

            if (rol == null)
            {
                return NotFound("El rol no existe.");
            }

            var nombre = dto.Nombre.Trim();

            var nombreDuplicado = await _context.Roles
                .AnyAsync(r => r.RolId != rolId && r.Nombre == nombre);

            if (nombreDuplicado)
            {
                return BadRequest("Ya existe otro rol con ese nombre.");
            }

            rol.Nombre = nombre;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                rol.RolId,
                rol.Nombre
            });
        }

        // DELETE: api/Roles/5
        [HttpDelete("{rolId:int}")]
        public async Task<IActionResult> DeleteRol(int rolId)
        {
            var rol = await _context.Roles
                .FirstOrDefaultAsync(r => r.RolId == rolId);

            if (rol == null)
            {
                return NotFound("El rol no existe.");
            }

            var tieneUsuarios = await _context.Usuarios
                .AnyAsync(u => u.RolId == rolId);

            if (tieneUsuarios)
            {
                return BadRequest("No se puede eliminar el rol porque tiene usuarios asociados.");
            }

            var tienePermisos = await _context.RolPantallaPermisos
                .AnyAsync(rpp => rpp.RolId == rolId);

            if (tienePermisos)
            {
                return BadRequest("No se puede eliminar el rol porque tiene permisos asociados.");
            }

            _context.Roles.Remove(rol);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}