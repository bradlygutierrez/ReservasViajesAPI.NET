using AppReservasAPI.Context;
using AppReservasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Administrador")]
public class MetodoPagosController : ControllerBase
{
    private readonly AppDbContext _context;
    public MetodoPagosController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/MetodoPago
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MetodoPago>>> GetMetodoPago()
    {
        return await _context.MetodosPago.ToListAsync();
    }

    // GET: api/MetodoPago/5
    [HttpGet("{metodopagoid}")]
    public async Task<ActionResult<MetodoPago>> GetMetodoPago(int metodopagoid)
    {
        var metodopago = await _context.MetodosPago.FindAsync(metodopagoid);

        if (metodopago == null)
        {
            return NotFound();
        }

        return metodopago;
    }

    // PUT: api/MetodoPago/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{metodopagoid}")]
    public async Task<IActionResult> PutMetodoPago(int? metodopagoid, MetodoPago metodopago)
    {
        if (metodopagoid != metodopago.MetodoPagoId)
        {
            return BadRequest();
        }

        _context.Entry(metodopago).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!MetodoPagoExists(metodopagoid))
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

    // POST: api/MetodoPago
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<MetodoPago>> PostMetodoPago(MetodoPago metodopago)
    {
        _context.MetodosPago.Add(metodopago);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetMetodoPago", new { metodopagoid = metodopago.MetodoPagoId }, metodopago);
    }

    // DELETE: api/MetodoPago/5
    [HttpDelete("{metodopagoid}")]
    public async Task<IActionResult> DeleteMetodoPago(int? metodopagoid)
    {
        var metodopago = await _context.MetodosPago.FindAsync(metodopagoid);
        if (metodopago == null)
        {
            return NotFound();
        }

        _context.MetodosPago.Remove(metodopago);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool MetodoPagoExists(int? metodopagoid)
    {
        return _context.MetodosPago.Any(e => e.MetodoPagoId == metodopagoid);
    }
}
