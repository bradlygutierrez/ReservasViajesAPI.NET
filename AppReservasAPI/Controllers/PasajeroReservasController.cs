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

    // GET: api/PasajeroReserva
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PasajeroReserva>>> GetPasajeroReserva()
    {
        return await _context.PasajerosReserva.ToListAsync();
    }

    // GET: api/PasajeroReserva/5
    [HttpGet("{pasajeroid}")]
    public async Task<ActionResult<PasajeroReserva>> GetPasajeroReserva(int pasajeroid)
    {
        var pasajeroreserva = await _context.PasajerosReserva.FindAsync(pasajeroid);

        if (pasajeroreserva == null)
        {
            return NotFound();
        }

        return pasajeroreserva;
    }

    // PUT: api/PasajeroReserva/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{pasajeroid}")]
    public async Task<IActionResult> PutPasajeroReserva(int? pasajeroid, PasajeroReserva pasajeroreserva)
    {
        if (pasajeroid != pasajeroreserva.PasajeroId)
        {
            return BadRequest();
        }

        _context.Entry(pasajeroreserva).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PasajeroReservaExists(pasajeroid))
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

    // POST: api/PasajeroReserva
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<PasajeroReserva>> PostPasajeroReserva(PasajeroReserva pasajeroreserva)
    {
        _context.PasajerosReserva.Add(pasajeroreserva);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetPasajeroReserva", new { pasajeroid = pasajeroreserva.PasajeroId }, pasajeroreserva);
    }

    // DELETE: api/PasajeroReserva/5
    [HttpDelete("{pasajeroid}")]
    public async Task<IActionResult> DeletePasajeroReserva(int? pasajeroid)
    {
        var pasajeroreserva = await _context.PasajerosReserva.FindAsync(pasajeroid);
        if (pasajeroreserva == null)
        {
            return NotFound();
        }

        _context.PasajerosReserva.Remove(pasajeroreserva);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool PasajeroReservaExists(int? pasajeroid)
    {
        return _context.PasajerosReserva.Any(e => e.PasajeroId == pasajeroid);
    }
}
