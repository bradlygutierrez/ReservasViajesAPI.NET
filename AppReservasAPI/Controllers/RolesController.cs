using AppReservasAPI.Context;
using AppReservasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

    // GET: api/Rol
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Rol>>> GetRol()
    {
        return await _context.Roles.ToListAsync();
    }

    // GET: api/Rol/5
    [HttpGet("{rolid}")]
    public async Task<ActionResult<Rol>> GetRol(int rolid)
    {
        var rol = await _context.Roles.FindAsync(rolid);

        if (rol == null)
        {
            return NotFound();
        }

        return rol;
    }

    // PUT: api/Rol/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{rolid}")]
    public async Task<IActionResult> PutRol(int? rolid, Rol rol)
    {
        if (rolid != rol.RolId)
        {
            return BadRequest();
        }

        _context.Entry(rol).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!RolExists(rolid))
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

    // POST: api/Rol
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Rol>> PostRol(Rol rol)
    {
        _context.Roles.Add(rol);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetRol", new { rolid = rol.RolId }, rol);
    }

    // DELETE: api/Rol/5
    [HttpDelete("{rolid}")]
    public async Task<IActionResult> DeleteRol(int? rolid)
    {
        var rol = await _context.Roles.FindAsync(rolid);
        if (rol == null)
        {
            return NotFound();
        }

        _context.Roles.Remove(rol);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool RolExists(int? rolid)
    {
        return _context.Roles.Any(e => e.RolId == rolid);
    }
}
