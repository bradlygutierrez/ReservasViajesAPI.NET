using AppReservasAPI.Context;
using AppReservasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

    // GET: api/Permiso
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Permiso>>> GetPermiso()
    {
        return await _context.Permisos.ToListAsync();
    }

    // GET: api/Permiso/5
    [HttpGet("{permisoid}")]
    public async Task<ActionResult<Permiso>> GetPermiso(int permisoid)
    {
        var permiso = await _context.Permisos.FindAsync(permisoid);

        if (permiso == null)
        {
            return NotFound();
        }

        return permiso;
    }

    // PUT: api/Permiso/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{permisoid}")]
    public async Task<IActionResult> PutPermiso(int? permisoid, Permiso permiso)
    {
        if (permisoid != permiso.PermisoId)
        {
            return BadRequest();
        }

        _context.Entry(permiso).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PermisoExists(permisoid))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/Permiso
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Permiso>> PostPermiso(Permiso permiso)
    {
        _context.Permisos.Add(permiso);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetPermiso", new { permisoid = permiso.PermisoId }, permiso);
    }

    // DELETE: api/Permiso/5
    [HttpDelete("{permisoid}")]
    public async Task<IActionResult> DeletePermiso(int? permisoid)
    {
        var permiso = await _context.Permisos.FindAsync(permisoid);
        if (permiso == null)
        {
            return NotFound();
        }

        _context.Permisos.Remove(permiso);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool PermisoExists(int? permisoid)
    {
        return _context.Permisos.Any(e => e.PermisoId == permisoid);
    }
}
