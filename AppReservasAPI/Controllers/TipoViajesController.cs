using AppReservasAPI.Context;
using AppReservasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TipoViajesController : ControllerBase
{
    private readonly AppDbContext _context;
    public TipoViajesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/TipoViaje
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TipoViaje>>> GetTipoViaje()
    {
        return await _context.TiposViaje.ToListAsync();
    }

    // GET: api/TipoViaje/5
    [HttpGet("{tipoviajeid}")]
    public async Task<ActionResult<TipoViaje>> GetTipoViaje(int tipoviajeid)
    {
        var tipoviaje = await _context.TiposViaje.FindAsync(tipoviajeid);

        if (tipoviaje == null)
        {
            return NotFound();
        }

        return tipoviaje;
    }

    // PUT: api/TipoViaje/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{tipoviajeid}")]
    public async Task<IActionResult> PutTipoViaje(int? tipoviajeid, TipoViaje tipoviaje)
    {
        if (tipoviajeid != tipoviaje.TipoViajeId)
        {
            return BadRequest();
        }

        _context.Entry(tipoviaje).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TipoViajeExists(tipoviajeid))
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

    // POST: api/TipoViaje
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<TipoViaje>> PostTipoViaje(TipoViaje tipoviaje)
    {
        _context.TiposViaje.Add(tipoviaje);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetTipoViaje", new { tipoviajeid = tipoviaje.TipoViajeId }, tipoviaje);
    }

    // DELETE: api/TipoViaje/5
    [HttpDelete("{tipoviajeid}")]
    public async Task<IActionResult> DeleteTipoViaje(int? tipoviajeid)
    {
        var tipoviaje = await _context.TiposViaje.FindAsync(tipoviajeid);
        if (tipoviaje == null)
        {
            return NotFound();
        }

        _context.TiposViaje.Remove(tipoviaje);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool TipoViajeExists(int? tipoviajeid)
    {
        return _context.TiposViaje.Any(e => e.TipoViajeId == tipoviajeid);
    }
}
