using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppReservasAPI.Models;
using AppReservasAPI.Context;

[Route("api/[controller]")]
[ApiController]
public class PagosController : ControllerBase
{
    private readonly AppDbContext _context;
    public PagosController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Pagos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Pagos>>> GetPagos()
    {
        return await _context.Pagos.ToListAsync();
    }

    // GET: api/Pagos/5
    [HttpGet("{pagosid}")]
    public async Task<ActionResult<Pagos>> GetPagos(int pagosid)
    {
        var pagos = await _context.Pagos.FindAsync(pagosid);

        if (pagos == null)
        {
            return NotFound();
        }

        return pagos;
    }

    // PUT: api/Pagos/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{pagosid}")]
    public async Task<IActionResult> PutPagos(int? pagosid, Pagos pagos)
    {
        if (pagosid != pagos.PagosId)
        {
            return BadRequest();
        }

        _context.Entry(pagos).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PagosExists(pagosid))
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

    // POST: api/Pagos
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Pagos>> PostPagos(Pagos pagos)
    {
        _context.Pagos.Add(pagos);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetPagos", new { pagosid = pagos.PagosId }, pagos);
    }

    // DELETE: api/Pagos/5
    [HttpDelete("{pagosid}")]
    public async Task<IActionResult> DeletePagos(int? pagosid)
    {
        var pagos = await _context.Pagos.FindAsync(pagosid);
        if (pagos == null)
        {
            return NotFound();
        }

        _context.Pagos.Remove(pagos);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool PagosExists(int? pagosid)
    {
        return _context.Pagos.Any(e => e.PagosId == pagosid);
    }
}
