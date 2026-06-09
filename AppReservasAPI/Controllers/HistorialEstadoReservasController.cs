using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppReservasAPI.Models;
using AppReservasAPI.Context;

[Route("api/[controller]")]
[ApiController]
public class HistorialEstadoReservasController : ControllerBase
{
    private readonly AppDbContext _context;
    public HistorialEstadoReservasController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/HistorialEstadoReserva
    [HttpGet]
    public async Task<ActionResult<IEnumerable<HistorialEstadoReserva>>> GetHistorialEstadoReserva()
    {
        return await _context.HistorialEstadosReserva.ToListAsync();
    }

    // GET: api/HistorialEstadoReserva/5
    [HttpGet("{historialid}")]
    public async Task<ActionResult<HistorialEstadoReserva>> GetHistorialEstadoReserva(long historialid)
    {
        var historialestadoreserva = await _context.HistorialEstadosReserva.FindAsync(historialid);

        if (historialestadoreserva == null)
        {
            return NotFound();
        }

        return historialestadoreserva;
    }

    // PUT: api/HistorialEstadoReserva/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{historialid}")]
    public async Task<IActionResult> PutHistorialEstadoReserva(long? historialid, HistorialEstadoReserva historialestadoreserva)
    {
        if (historialid != historialestadoreserva.HistorialId)
        {
            return BadRequest();
        }

        _context.Entry(historialestadoreserva).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!HistorialEstadoReservaExists(historialid))
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

    // POST: api/HistorialEstadoReserva
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<HistorialEstadoReserva>> PostHistorialEstadoReserva(HistorialEstadoReserva historialestadoreserva)
    {
        _context.HistorialEstadosReserva.Add(historialestadoreserva);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetHistorialEstadoReserva", new { historialid = historialestadoreserva.HistorialId }, historialestadoreserva);
    }

    // DELETE: api/HistorialEstadoReserva/5
    [HttpDelete("{historialid}")]
    public async Task<IActionResult> DeleteHistorialEstadoReserva(long? historialid)
    {
        var historialestadoreserva = await _context.HistorialEstadosReserva.FindAsync(historialid);
        if (historialestadoreserva == null)
        {
            return NotFound();
        }

        _context.HistorialEstadosReserva.Remove(historialestadoreserva);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool HistorialEstadoReservaExists(long? historialid)
    {
        return _context.HistorialEstadosReserva.Any(e => e.HistorialId == historialid);
    }
}
