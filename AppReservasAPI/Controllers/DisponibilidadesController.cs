using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppReservasAPI.Models;
using AppReservasAPI.Context;

[Route("api/[controller]")]
[ApiController]
public class DisponibilidadesController : ControllerBase
{
    private readonly AppDbContext _context;
    public DisponibilidadesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Disponibilidades
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Disponibilidades>>> GetDisponibilidades()
    {
        return await _context.Disponibilidades.ToListAsync();
    }

    // GET: api/Disponibilidades/5
    [HttpGet("{disponibilidadid}")]
    public async Task<ActionResult<Disponibilidades>> GetDisponibilidades(int disponibilidadid)
    {
        var disponibilidades = await _context.Disponibilidades.FindAsync(disponibilidadid);

        if (disponibilidades == null)
        {
            return NotFound();
        }

        return disponibilidades;
    }

    // PUT: api/Disponibilidades/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{disponibilidadid}")]
    public async Task<IActionResult> PutDisponibilidades(int? disponibilidadid, Disponibilidades disponibilidades)
    {
        if (disponibilidadid != disponibilidades.DisponibilidadId)
        {
            return BadRequest();
        }

        _context.Entry(disponibilidades).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!DisponibilidadesExists(disponibilidadid))
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

    // POST: api/Disponibilidades
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Disponibilidades>> PostDisponibilidades(Disponibilidades disponibilidades)
    {
        _context.Disponibilidades.Add(disponibilidades);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetDisponibilidades", new { disponibilidadid = disponibilidades.DisponibilidadId }, disponibilidades);
    }

    // DELETE: api/Disponibilidades/5
    [HttpDelete("{disponibilidadid}")]
    public async Task<IActionResult> DeleteDisponibilidades(int? disponibilidadid)
    {
        var disponibilidades = await _context.Disponibilidades.FindAsync(disponibilidadid);
        if (disponibilidades == null)
        {
            return NotFound();
        }

        _context.Disponibilidades.Remove(disponibilidades);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool DisponibilidadesExists(int? disponibilidadid)
    {
        return _context.Disponibilidades.Any(e => e.DisponibilidadId == disponibilidadid);
    }
}
