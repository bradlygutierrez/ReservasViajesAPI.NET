using AppReservasAPI.Context;
using AppReservasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PaisesController : ControllerBase
{
    private readonly AppDbContext _context;
    public PaisesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Pais
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Pais>>> GetPais()
    {
        return await _context.Paises.ToListAsync();
    }

    // GET: api/Pais/5
    [HttpGet("{paisid}")]
    public async Task<ActionResult<Pais>> GetPais(int paisid)
    {
        var pais = await _context.Paises.FindAsync(paisid);

        if (pais == null)
        {
            return NotFound();
        }

        return pais;
    }

    // PUT: api/Pais/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{paisid}")]
    public async Task<IActionResult> PutPais(int? paisid, Pais pais)
    {
        if (paisid != pais.PaisId)
        {
            return BadRequest();
        }

        _context.Entry(pais).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PaisExists(paisid))
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

    // POST: api/Pais
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Pais>> PostPais(Pais pais)
    {
        _context.Paises.Add(pais);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetPais", new { paisid = pais.PaisId }, pais);
    }

    // DELETE: api/Pais/5
    [HttpDelete("{paisid}")]
    public async Task<IActionResult> DeletePais(int? paisid)
    {
        var pais = await _context.Paises.FindAsync(paisid);
        if (pais == null)
        {
            return NotFound();
        }

        _context.Paises.Remove(pais);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool PaisExists(int? paisid)
    {
        return _context.Paises.Any(e => e.PaisId == paisid);
    }
}
