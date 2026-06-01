using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppReservasAPI.Models;
using AppReservasAPI.Context;

[Route("api/[controller]")]
[ApiController]
public class TipoVIajesController : ControllerBase
{
    private readonly AppDbContext _context;
    public TipoVIajesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/TIpoVIaje
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TIpoVIaje>>> GetTIpoVIaje()
    {
        return await _context.TIpoVIaje.ToListAsync();
    }

    // GET: api/TIpoVIaje/5
    [HttpGet("{tipoviajeid}")]
    public async Task<ActionResult<TIpoVIaje>> GetTIpoVIaje(int tipoviajeid)
    {
        var tipoviaje = await _context.TIpoVIaje.FindAsync(tipoviajeid);

        if (tipoviaje == null)
        {
            return NotFound();
        }

        return tipoviaje;
    }

    // PUT: api/TIpoVIaje/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{tipoviajeid}")]
    public async Task<IActionResult> PutTIpoVIaje(int? tipoviajeid, TIpoVIaje tipoviaje)
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
            if (!TIpoVIajeExists(tipoviajeid))
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

    // POST: api/TIpoVIaje
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<TIpoVIaje>> PostTIpoVIaje(TIpoVIaje tipoviaje)
    {
        _context.TIpoVIaje.Add(tipoviaje);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetTIpoVIaje", new { tipoviajeid = tipoviaje.TipoViajeId }, tipoviaje);
    }

    // DELETE: api/TIpoVIaje/5
    [HttpDelete("{tipoviajeid}")]
    public async Task<IActionResult> DeleteTIpoVIaje(int? tipoviajeid)
    {
        var tipoviaje = await _context.TIpoVIaje.FindAsync(tipoviajeid);
        if (tipoviaje == null)
        {
            return NotFound();
        }

        _context.TIpoVIaje.Remove(tipoviaje);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool TIpoVIajeExists(int? tipoviajeid)
    {
        return _context.TIpoVIaje.Any(e => e.TipoViajeId == tipoviajeid);
    }
}
