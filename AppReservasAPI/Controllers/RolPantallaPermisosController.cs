using AppReservasAPI.Context;
using AppReservasAPI.DTOs.RolPantallaPermisos;
using AppReservasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppReservasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")]
    public class RolPantallaPermisosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RolPantallaPermisosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/RolPantallaPermisos
        [HttpGet]
        public async Task<IActionResult> GetRolPantallaPermisos()
        {
            var datos = await _context.RolPantallaPermisos
                .AsNoTracking()
                .Select(rpp => new
                {
                    rpp.RolPantallaPermisoId,
                    rpp.RolId,
                    Rol = rpp.Rol == null ? null : rpp.Rol.Nombre,
                    rpp.PantallaId,
                    Pantalla = rpp.Pantalla == null ? null : rpp.Pantalla.Nombre,
                    Ruta = rpp.Pantalla == null ? null : rpp.Pantalla.Ruta,
                    Modulo = rpp.Pantalla == null ? null : rpp.Pantalla.Modulo,
                    rpp.PermisoId,
                    CodigoPermiso = rpp.Permiso == null ? null : rpp.Permiso.Codigo,
                    Permiso = rpp.Permiso == null ? null : rpp.Permiso.Nombre,
                    rpp.Activo,
                    rpp.FechaCreacion
                })
                .OrderBy(x => x.Rol)
                .ThenBy(x => x.Modulo)
                .ThenBy(x => x.Pantalla)
                .ThenBy(x => x.Permiso)
                .ToListAsync();

            return Ok(datos);
        }

        // GET: api/RolPantallaPermisos/5
        [HttpGet("{rolPantallaPermisoId:int}")]
        public async Task<IActionResult> GetRolPantallaPermiso(int rolPantallaPermisoId)
        {
            var dato = await _context.RolPantallaPermisos
                .AsNoTracking()
                .Where(rpp => rpp.RolPantallaPermisoId == rolPantallaPermisoId)
                .Select(rpp => new
                {
                    rpp.RolPantallaPermisoId,
                    rpp.RolId,
                    Rol = rpp.Rol == null ? null : rpp.Rol.Nombre,
                    rpp.PantallaId,
                    Pantalla = rpp.Pantalla == null ? null : rpp.Pantalla.Nombre,
                    Ruta = rpp.Pantalla == null ? null : rpp.Pantalla.Ruta,
                    Modulo = rpp.Pantalla == null ? null : rpp.Pantalla.Modulo,
                    rpp.PermisoId,
                    CodigoPermiso = rpp.Permiso == null ? null : rpp.Permiso.Codigo,
                    Permiso = rpp.Permiso == null ? null : rpp.Permiso.Nombre,
                    rpp.Activo,
                    rpp.FechaCreacion
                })
                .FirstOrDefaultAsync();

            if (dato == null)
            {
                return NotFound("La asignación de permiso no existe.");
            }

            return Ok(dato);
        }

        // GET: api/RolPantallaPermisos/rol/1
        [HttpGet("rol/{rolId:int}")]
        public async Task<IActionResult> GetPermisosPorRol(int rolId)
        {
            var rolExiste = await _context.Roles.AnyAsync(r => r.RolId == rolId);

            if (!rolExiste)
            {
                return NotFound("El rol no existe.");
            }

            var permisos = await _context.RolPantallaPermisos
                .AsNoTracking()
                .Where(rpp => rpp.RolId == rolId && rpp.Activo)
                .Select(rpp => new
                {
                    rpp.RolPantallaPermisoId,
                    rpp.PantallaId,
                    Pantalla = rpp.Pantalla == null ? null : rpp.Pantalla.Nombre,
                    Ruta = rpp.Pantalla == null ? null : rpp.Pantalla.Ruta,
                    Modulo = rpp.Pantalla == null ? null : rpp.Pantalla.Modulo,
                    Icono = rpp.Pantalla == null ? null : rpp.Pantalla.Icono,
                    Orden = rpp.Pantalla == null ? 0 : rpp.Pantalla.Orden,
                    rpp.PermisoId,
                    CodigoPermiso = rpp.Permiso == null ? null : rpp.Permiso.Codigo,
                    Permiso = rpp.Permiso == null ? null : rpp.Permiso.Nombre
                })
                .OrderBy(x => x.Orden)
                .ThenBy(x => x.Pantalla)
                .ThenBy(x => x.Permiso)
                .ToListAsync();

            return Ok(permisos);
        }

        // GET: api/RolPantallaPermisos/rol/1/agrupado
        [HttpGet("rol/{rolId:int}/agrupado")]
        public async Task<IActionResult> GetPermisosPorRolAgrupado(int rolId)
        {
            var rol = await _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RolId == rolId);

            if (rol == null)
            {
                return NotFound("El rol no existe.");
            }

            var permisosPlano = await _context.RolPantallaPermisos
                .AsNoTracking()
                .Where(rpp => rpp.RolId == rolId && rpp.Activo)
                .Select(rpp => new
                {
                    rpp.PantallaId,
                    Pantalla = rpp.Pantalla == null ? null : rpp.Pantalla.Nombre,
                    Ruta = rpp.Pantalla == null ? null : rpp.Pantalla.Ruta,
                    Modulo = rpp.Pantalla == null ? null : rpp.Pantalla.Modulo,
                    Icono = rpp.Pantalla == null ? null : rpp.Pantalla.Icono,
                    Orden = rpp.Pantalla == null ? 0 : rpp.Pantalla.Orden,
                    rpp.PermisoId,
                    CodigoPermiso = rpp.Permiso == null ? null : rpp.Permiso.Codigo,
                    Permiso = rpp.Permiso == null ? null : rpp.Permiso.Nombre
                })
                .OrderBy(x => x.Orden)
                .ThenBy(x => x.Pantalla)
                .ToListAsync();

            var agrupado = permisosPlano
                .GroupBy(x => new
                {
                    x.PantallaId,
                    x.Pantalla,
                    x.Ruta,
                    x.Modulo,
                    x.Icono,
                    x.Orden
                })
                .Select(g => new
                {
                    g.Key.PantallaId,
                    g.Key.Pantalla,
                    g.Key.Ruta,
                    g.Key.Modulo,
                    g.Key.Icono,
                    g.Key.Orden,
                    Permisos = g.Select(p => new
                    {
                        p.PermisoId,
                        p.CodigoPermiso,
                        p.Permiso
                    }).ToList()
                })
                .ToList();

            return Ok(new
            {
                rol.RolId,
                Rol = rol.Nombre,
                Pantallas = agrupado
            });
        }

        // POST: api/RolPantallaPermisos
        [HttpPost]
        public async Task<IActionResult> CrearRolPantallaPermiso([FromBody] CrearRolPantallaPermisoDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var rolExiste = await _context.Roles.AnyAsync(r => r.RolId == dto.RolId);
            if (!rolExiste)
            {
                return BadRequest("El rol no existe.");
            }

            var pantallaExiste = await _context.Pantallas.AnyAsync(p => p.PantallaId == dto.PantallaId && p.Activo);
            if (!pantallaExiste)
            {
                return BadRequest("La pantalla no existe o no está activa.");
            }

            var permisoExiste = await _context.Permisos.AnyAsync(p => p.PermisoId == dto.PermisoId && p.Activo);
            if (!permisoExiste)
            {
                return BadRequest("El permiso no existe o no está activo.");
            }

            var existente = await _context.RolPantallaPermisos
                .FirstOrDefaultAsync(rpp =>
                    rpp.RolId == dto.RolId &&
                    rpp.PantallaId == dto.PantallaId &&
                    rpp.PermisoId == dto.PermisoId);

            if (existente != null)
            {
                if (existente.Activo)
                {
                    return BadRequest("Este permiso ya está asignado a ese rol y pantalla.");
                }

                existente.Activo = true;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    mensaje = "Permiso reactivado correctamente.",
                    existente.RolPantallaPermisoId,
                    existente.RolId,
                    existente.PantallaId,
                    existente.PermisoId,
                    existente.Activo
                });
            }

            var asignacion = new RolPantallaPermiso
            {
                RolId = dto.RolId,
                PantallaId = dto.PantallaId,
                PermisoId = dto.PermisoId,
                Activo = true,
                FechaCreacion = DateTime.Now
            };

            _context.RolPantallaPermisos.Add(asignacion);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetRolPantallaPermiso),
                new { rolPantallaPermisoId = asignacion.RolPantallaPermisoId },
                new
                {
                    asignacion.RolPantallaPermisoId,
                    asignacion.RolId,
                    asignacion.PantallaId,
                    asignacion.PermisoId,
                    asignacion.Activo,
                    asignacion.FechaCreacion
                }
            );
        }

        // POST: api/RolPantallaPermisos/asignar
        [HttpPost("asignar")]
        public async Task<IActionResult> AsignarPermisosPantallaRol([FromBody] AsignarPermisosPantallaRolDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (dto.PermisoIds == null || dto.PermisoIds.Count == 0)
            {
                return BadRequest("Debe seleccionar al menos un permiso.");
            }

            dto.PermisoIds = dto.PermisoIds.Distinct().ToList();

            var rolExiste = await _context.Roles.AnyAsync(r => r.RolId == dto.RolId);
            if (!rolExiste)
            {
                return BadRequest("El rol no existe.");
            }

            var pantallaExiste = await _context.Pantallas.AnyAsync(p => p.PantallaId == dto.PantallaId && p.Activo);
            if (!pantallaExiste)
            {
                return BadRequest("La pantalla no existe o no está activa.");
            }

            var permisosValidos = await _context.Permisos
                .Where(p => dto.PermisoIds.Contains(p.PermisoId) && p.Activo)
                .Select(p => p.PermisoId)
                .ToListAsync();

            if (permisosValidos.Count != dto.PermisoIds.Count)
            {
                return BadRequest("Uno o más permisos no existen o no están activos.");
            }

            var asignacionesExistentes = await _context.RolPantallaPermisos
                .Where(rpp => rpp.RolId == dto.RolId && rpp.PantallaId == dto.PantallaId)
                .ToListAsync();

            foreach (var asignacion in asignacionesExistentes)
            {
                asignacion.Activo = dto.PermisoIds.Contains(asignacion.PermisoId);
            }

            var permisosYaExistentes = asignacionesExistentes
                .Select(a => a.PermisoId)
                .ToList();

            var permisosNuevos = dto.PermisoIds
                .Where(id => !permisosYaExistentes.Contains(id))
                .ToList();

            foreach (var permisoId in permisosNuevos)
            {
                _context.RolPantallaPermisos.Add(new RolPantallaPermiso
                {
                    RolId = dto.RolId,
                    PantallaId = dto.PantallaId,
                    PermisoId = permisoId,
                    Activo = true,
                    FechaCreacion = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Permisos actualizados correctamente.",
                dto.RolId,
                dto.PantallaId,
                PermisoIds = dto.PermisoIds
            });
        }

        // PUT: api/RolPantallaPermisos/5/estado
        [HttpPut("{rolPantallaPermisoId:int}/estado")]
        public async Task<IActionResult> CambiarEstado(int rolPantallaPermisoId, [FromBody] CambiarEstadoRolPantallaPermisoDto dto)
        {
            var asignacion = await _context.RolPantallaPermisos
                .FirstOrDefaultAsync(rpp => rpp.RolPantallaPermisoId == rolPantallaPermisoId);

            if (asignacion == null)
            {
                return NotFound("La asignación de permiso no existe.");
            }

            asignacion.Activo = dto.Activo;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                asignacion.RolPantallaPermisoId,
                asignacion.RolId,
                asignacion.PantallaId,
                asignacion.PermisoId,
                asignacion.Activo
            });
        }

        // DELETE: api/RolPantallaPermisos/5
        [HttpDelete("{rolPantallaPermisoId:int}")]
        public async Task<IActionResult> DeleteRolPantallaPermiso(int rolPantallaPermisoId)
        {
            var asignacion = await _context.RolPantallaPermisos
                .FirstOrDefaultAsync(rpp => rpp.RolPantallaPermisoId == rolPantallaPermisoId);

            if (asignacion == null)
            {
                return NotFound("La asignación de permiso no existe.");
            }

            asignacion.Activo = false;
            await _context.SaveChangesAsync();

            return Ok("Permiso desactivado correctamente.");
        }
    }
}