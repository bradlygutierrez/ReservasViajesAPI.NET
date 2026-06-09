using AppReservasAPI.Context;
using AppReservasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Administrador")]
public class EstadoPagosController : ControllerBase
{
    private readonly AppDbContext _context;
    public EstadoPagosController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/EstadoPago
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EstadoPago>>> GetEstadoPago()
    {
        return await _context.EstadosPago.ToListAsync();
    }

    // GET: api/EstadoPago/5
    [HttpGet("{estadopagoid}")]
    public async Task<ActionResult<EstadoPago>> GetEstadoPago(int estadopagoid)
    {
        var estadopago = await _context.EstadosPago.FindAsync(estadopagoid);

        if (estadopago == null)
        {
            return NotFound();
        }

        return estadopago;
    }

    // PUT: api/EstadoPago/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{estadopagoid}")]
    public async Task<IActionResult> PutEstadoPago(int? estadopagoid, EstadoPago estadopago)
    {
        if (estadopagoid != estadopago.EstadoPagoId)
        {
            return BadRequest();
        }

        _context.Entry(estadopago).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!EstadoPagoExists(estadopagoid))
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

    // POST: api/EstadoPago
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<EstadoPago>> PostEstadoPago(EstadoPago estadopago)
    {
        _context.EstadosPago.Add(estadopago);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetEstadoPago", new { estadopagoid = estadopago.EstadoPagoId }, estadopago);
    }

    // DELETE: api/EstadoPago/5
    [HttpDelete("{estadopagoid}")]
    public async Task<IActionResult> DeleteEstadoPago(int? estadopagoid)
    {
        var estadopago = await _context.EstadosPago.FindAsync(estadopagoid);
        if (estadopago == null)
        {
            return NotFound();
        }

        _context.EstadosPago.Remove(estadopago);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool EstadoPagoExists(int? estadopagoid)
    {
        return _context.EstadosPago.Any(e => e.EstadoPagoId == estadopagoid);
    }
}
