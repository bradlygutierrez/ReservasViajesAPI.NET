using AppReservasAPI.Context;
using AppReservasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Administrador")]
public class EstadoReservasController : ControllerBase
{
    private readonly AppDbContext _context;
    public EstadoReservasController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/EstadoReserva
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EstadoReserva>>> GetEstadoReserva()
    {
        return await _context.EstadosReserva.ToListAsync();
    }

    // GET: api/EstadoReserva/5
    [HttpGet("{estadoreservaid}")]
    public async Task<ActionResult<EstadoReserva>> GetEstadoReserva(int estadoreservaid)
    {
        var estadoreserva = await _context.EstadosReserva.FindAsync(estadoreservaid);

        if (estadoreserva == null)
        {
            return NotFound();
        }

        return estadoreserva;
    }

    // PUT: api/EstadoReserva/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{estadoreservaid}")]
    public async Task<IActionResult> PutEstadoReserva(int? estadoreservaid, EstadoReserva estadoreserva)
    {
        if (estadoreservaid != estadoreserva.EstadoReservaId)
        {
            return BadRequest();
        }

        _context.Entry(estadoreserva).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!EstadoReservaExists(estadoreservaid))
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

    // POST: api/EstadoReserva
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<EstadoReserva>> PostEstadoReserva(EstadoReserva estadoreserva)
    {
        _context.EstadosReserva.Add(estadoreserva);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetEstadoReserva", new { estadoreservaid = estadoreserva.EstadoReservaId }, estadoreserva);
    }

    // DELETE: api/EstadoReserva/5
    [HttpDelete("{estadoreservaid}")]
    public async Task<IActionResult> DeleteEstadoReserva(int? estadoreservaid)
    {
        var estadoreserva = await _context.EstadosReserva.FindAsync(estadoreservaid);
        if (estadoreserva == null)
        {
            return NotFound();
        }

        _context.EstadosReserva.Remove(estadoreserva);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool EstadoReservaExists(int? estadoreservaid)
    {
        return _context.EstadosReserva.Any(e => e.EstadoReservaId == estadoreservaid);
    }
}
