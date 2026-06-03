using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppReservasAPI.Models;
using AppReservasAPI.Context;

[Route("api/[controller]")]
[ApiController]
public class PasajeroReservasController : ControllerBase
{
    private readonly AppDbContext _context;
    public PasajeroReservasController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/PasajeroReservas
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PasajeroReservas>>> GetPasajeroReservas()
    {
        return await _context.PasajeroReservas.ToListAsync();
    }

    // GET: api/PasajeroReservas/5
    [HttpGet("{pasajeroreservaid}")]
    public async Task<ActionResult<PasajeroReservas>> GetPasajeroReservas(int pasajeroreservaid)
    {
        var pasajeroreservas = await _context.PasajeroReservas.FindAsync(pasajeroreservaid);

        if (pasajeroreservas == null)
        {
            return NotFound();
        }

        return pasajeroreservas;
    }

    // PUT: api/PasajeroReservas/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{pasajeroreservaid}")]
    public async Task<IActionResult> PutPasajeroReservas(int? pasajeroreservaid, PasajeroReservas pasajeroreservas)
    {
        if (pasajeroreservaid != pasajeroreservas.PasajeroReservaId)
        {
            return BadRequest();
        }

        _context.Entry(pasajeroreservas).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PasajeroReservasExists(pasajeroreservaid))
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

    // POST: api/PasajeroReservas
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<PasajeroReservas>> PostPasajeroReservas(PasajeroReservas pasajeroreservas)
    {
        _context.PasajeroReservas.Add(pasajeroreservas);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetPasajeroReservas", new { pasajeroreservaid = pasajeroreservas.PasajeroReservaId }, pasajeroreservas);
    }

    // DELETE: api/PasajeroReservas/5
    [HttpDelete("{pasajeroreservaid}")]
    public async Task<IActionResult> DeletePasajeroReservas(int? pasajeroreservaid)
    {
        var pasajeroreservas = await _context.PasajeroReservas.FindAsync(pasajeroreservaid);
        if (pasajeroreservas == null)
        {
            return NotFound();
        }

        _context.PasajeroReservas.Remove(pasajeroreservas);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool PasajeroReservasExists(int? pasajeroreservaid)
    {
        return _context.PasajeroReservas.Any(e => e.PasajeroReservaId == pasajeroreservaid);
    }
}
