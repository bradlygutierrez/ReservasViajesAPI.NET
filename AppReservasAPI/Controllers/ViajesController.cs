using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppReservasAPI.Models;
using AppReservasAPI.Context;

[Route("api/[controller]")]
[ApiController]
public class ViajesController : ControllerBase
{
    private readonly AppDbContext _context;
    public ViajesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Viaje
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Viaje>>> GetViaje()
    {
        return await _context.Viajes.ToListAsync();
    }

    // GET: api/Viaje/5
    [HttpGet("{viajeid}")]
    public async Task<ActionResult<Viaje>> GetViaje(int viajeid)
    {
        var viaje = await _context.Viajes.FindAsync(viajeid);

        if (viaje == null)
        {
            return NotFound();
        }

        return viaje;
    }

    // PUT: api/Viaje/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{viajeid}")]
    public async Task<IActionResult> PutViaje(int? viajeid, Viaje viaje)
    {
        if (viajeid != viaje.ViajeId)
        {
            return BadRequest();
        }

        _context.Entry(viaje).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ViajeExists(viajeid))
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

    // POST: api/Viaje
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Viaje>> PostViaje(Viaje viaje)
    {
        _context.Viajes.Add(viaje);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetViaje", new { viajeid = viaje.ViajeId }, viaje);
    }

    // DELETE: api/Viaje/5
    [HttpDelete("{viajeid}")]
    public async Task<IActionResult> DeleteViaje(int? viajeid)
    {
        var viaje = await _context.Viajes.FindAsync(viajeid);
        if (viaje == null)
        {
            return NotFound();
        }

        _context.Viajes.Remove(viaje);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ViajeExists(int? viajeid)
    {
        return _context.Viajes.Any(e => e.ViajeId == viajeid);
    }
}
