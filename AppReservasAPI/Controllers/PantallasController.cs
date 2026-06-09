using AppReservasAPI.Context;
using AppReservasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Administrador")]
public class PantallasController : ControllerBase
{
    private readonly AppDbContext _context;
    public PantallasController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Pantalla
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Pantalla>>> GetPantalla()
    {
        return await _context.Pantallas.ToListAsync();
    }

    // GET: api/Pantalla/5
    [HttpGet("{pantallaid}")]
    public async Task<ActionResult<Pantalla>> GetPantalla(int pantallaid)
    {
        var pantalla = await _context.Pantallas.FindAsync(pantallaid);

        if (pantalla == null)
        {
            return NotFound();
        }

        return pantalla;
    }

    // PUT: api/Pantalla/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{pantallaid}")]
    public async Task<IActionResult> PutPantalla(int? pantallaid, Pantalla pantalla)
    {
        if (pantallaid != pantalla.PantallaId)
        {
            return BadRequest();
        }

        _context.Entry(pantalla).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PantallaExists(pantallaid))
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

    // POST: api/Pantalla
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Pantalla>> PostPantalla(Pantalla pantalla)
    {
        _context.Pantallas.Add(pantalla);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetPantalla", new { pantallaid = pantalla.PantallaId }, pantalla);
    }

    // DELETE: api/Pantalla/5
    [HttpDelete("{pantallaid}")]
    public async Task<IActionResult> DeletePantalla(int? pantallaid)
    {
        var pantalla = await _context.Pantallas.FindAsync(pantallaid);
        if (pantalla == null)
        {
            return NotFound();
        }

        _context.Pantallas.Remove(pantalla);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool PantallaExists(int? pantallaid)
    {
        return _context.Pantallas.Any(e => e.PantallaId == pantallaid);
    }
}
