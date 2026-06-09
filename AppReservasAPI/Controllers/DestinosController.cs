using AppReservasAPI.Context;
using AppReservasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DestinosController : ControllerBase
{
    private readonly AppDbContext _context;
    public DestinosController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Destino
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Destino>>> GetDestino()
    {
        return await _context.Destinos.ToListAsync();
    }

    // GET: api/Destino/5
    [HttpGet("{destinoid}")]
    public async Task<ActionResult<Destino>> GetDestino(int destinoid)
    {
        var destino = await _context.Destinos.FindAsync(destinoid);

        if (destino == null)
        {
            return NotFound();
        }

        return destino;
    }

    // PUT: api/Destino/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{destinoid}")]
    public async Task<IActionResult> PutDestino(int? destinoid, Destino destino)
    {
        if (destinoid != destino.DestinoId)
        {
            return BadRequest();
        }

        _context.Entry(destino).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!DestinoExists(destinoid))
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

    // POST: api/Destino
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Destino>> PostDestino(Destino destino)
    {
        _context.Destinos.Add(destino);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetDestino", new { destinoid = destino.DestinoId }, destino);
    }

    // DELETE: api/Destino/5
    [HttpDelete("{destinoid}")]
    public async Task<IActionResult> DeleteDestino(int? destinoid)
    {
        var destino = await _context.Destinos.FindAsync(destinoid);
        if (destino == null)
        {
            return NotFound();
        }

        _context.Destinos.Remove(destino);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool DestinoExists(int? destinoid)
    {
        return _context.Destinos.Any(e => e.DestinoId == destinoid);
    }
}
