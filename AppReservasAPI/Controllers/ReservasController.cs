using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppReservasAPI.Models;
using AppReservasAPI.Context;

[Route("api/[controller]")]
[ApiController]
public class ReservasController : ControllerBase
{
    private readonly AppDbContext _context;
    public ReservasController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Reservas
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Reservas>>> GetReservas()
    {
        return await _context.Reservas.ToListAsync();
    }

    // GET: api/Reservas/5
    [HttpGet("{reservaid}")]
    public async Task<ActionResult<Reservas>> GetReservas(int reservaid)
    {
        var reservas = await _context.Reservas.FindAsync(reservaid);

        if (reservas == null)
        {
            return NotFound();
        }

        return reservas;
    }

    // PUT: api/Reservas/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{reservaid}")]
    public async Task<IActionResult> PutReservas(int? reservaid, Reservas reservas)
    {
        if (reservaid != reservas.ReservaId)
        {
            return BadRequest();
        }

        _context.Entry(reservas).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ReservasExists(reservaid))
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

    // POST: api/Reservas
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Reservas>> PostReservas(Reservas reservas)
    {
        _context.Reservas.Add(reservas);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetReservas", new { reservaid = reservas.ReservaId }, reservas);
    }

    // DELETE: api/Reservas/5
    [HttpDelete("{reservaid}")]
    public async Task<IActionResult> DeleteReservas(int? reservaid)
    {
        var reservas = await _context.Reservas.FindAsync(reservaid);
        if (reservas == null)
        {
            return NotFound();
        }

        _context.Reservas.Remove(reservas);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ReservasExists(int? reservaid)
    {
        return _context.Reservas.Any(e => e.ReservaId == reservaid);
    }
}
