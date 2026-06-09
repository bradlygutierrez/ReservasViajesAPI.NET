using AppReservasAPI.Context;
using AppReservasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

    // GET: api/Ciudad
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Ciudad>>> GetCiudad()
    {
        return await _context.Ciudades.ToListAsync();
    }

    // GET: api/Ciudad/5
    [HttpGet("{ciudadid}")]
    public async Task<ActionResult<Ciudad>> GetCiudad(int ciudadid)
    {
        var ciudad = await _context.Ciudades.FindAsync(ciudadid);

        if (ciudad == null)
        {
            return NotFound();
        }

        return ciudad;
    }

    // PUT: api/Ciudad/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{ciudadid}")]
    public async Task<IActionResult> PutCiudad(int? ciudadid, Ciudad ciudad)
    {
        if (ciudadid != ciudad.CiudadId)
        {
            return BadRequest();
        }

        _context.Entry(ciudad).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CiudadExists(ciudadid))
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

    // POST: api/Ciudad
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Ciudad>> PostCiudad(Ciudad ciudad)
    {
        _context.Ciudades.Add(ciudad);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetCiudad", new { ciudadid = ciudad.CiudadId }, ciudad);
    }

    // DELETE: api/Ciudad/5
    [HttpDelete("{ciudadid}")]
    public async Task<IActionResult> DeleteCiudad(int? ciudadid)
    {
        var ciudad = await _context.Ciudades.FindAsync(ciudadid);
        if (ciudad == null)
        {
            return NotFound();
        }

        _context.Ciudades.Remove(ciudad);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool CiudadExists(int? ciudadid)
    {
        return _context.Ciudades.Any(e => e.CiudadId == ciudadid);
    }
}
