using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppReservasAPI.Models;
using AppReservasAPI.Context;

[Route("api/[controller]")]
[ApiController]
public class RolesController : ControllerBase
{
    private readonly AppDbContext _context;
    public RolesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Roles
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Roles>>> GetRoles()
    {
        return await _context.Roles.ToListAsync();
    }

    // GET: api/Roles/5
    [HttpGet("{rolid}")]
    public async Task<ActionResult<Roles>> GetRoles(int rolid)
    {
        var roles = await _context.Roles.FindAsync(rolid);

        if (roles == null)
        {
            return NotFound();
        }

        return roles;
    }

    // PUT: api/Roles/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{rolid}")]
    public async Task<IActionResult> PutRoles(int? rolid, Roles roles)
    {
        if (rolid != roles.RolId)
        {
            return BadRequest();
        }

        _context.Entry(roles).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!RolesExists(rolid))
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

    // POST: api/Roles
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Roles>> PostRoles(Roles roles)
    {
        _context.Roles.Add(roles);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetRoles", new { rolid = roles.RolId }, roles);
    }

    // DELETE: api/Roles/5
    [HttpDelete("{rolid}")]
    public async Task<IActionResult> DeleteRoles(int? rolid)
    {
        var roles = await _context.Roles.FindAsync(rolid);
        if (roles == null)
        {
            return NotFound();
        }

        _context.Roles.Remove(roles);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool RolesExists(int? rolid)
    {
        return _context.Roles.Any(e => e.RolId == rolid);
    }
}
