using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppReservasAPI.Models;
using AppReservasAPI.Context;

[Route("api/[controller]")]
[ApiController]
public class DestinosController : ControllerBase
{
    private readonly AppDbContext _context;
    public DestinosController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Destinos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Destinos>>> GetDestinos()
    {
        return await _context.Destinos.ToListAsync();
    }

    // GET: api/Destinos/5
    [HttpGet("{destinoid}")]
    public async Task<ActionResult<Destinos>> GetDestinos(int destinoid)
    {
        var destinos = await _context.Destinos.FindAsync(destinoid);

        if (destinos == null)
        {
            return NotFound();
        }

        return destinos;
    }

    // PUT: api/Destinos/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{destinoid}")]
    public async Task<IActionResult> PutDestinos(int? destinoid, Destinos destinos)
    {
        if (destinoid != destinos.DestinoId)
        {
            return BadRequest();
        }

        _context.Entry(destinos).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!DestinosExists(destinoid))
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

    // POST: api/Destinos
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Destinos>> PostDestinos(Destinos destinos)
    {
        _context.Destinos.Add(destinos);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetDestinos", new { destinoid = destinos.DestinoId }, destinos);
    }

    // DELETE: api/Destinos/5
    [HttpDelete("{destinoid}")]
    public async Task<IActionResult> DeleteDestinos(int? destinoid)
    {
        var destinos = await _context.Destinos.FindAsync(destinoid);
        if (destinos == null)
        {
            return NotFound();
        }

        _context.Destinos.Remove(destinos);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool DestinosExists(int? destinoid)
    {
        return _context.Destinos.Any(e => e.DestinoId == destinoid);
    }
}
