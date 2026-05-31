using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppReservasAPI.Models;
using AppReservasAPI.Context;

[Route("api/[controller]")]
[ApiController]
public class UsuariosController : ControllerBase
{
    private readonly AppDbContext _context;
    public UsuariosController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Usuarios
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Usuarios>>> GetUsuarios()
    {
        return await _context.Usuarios.ToListAsync();
    }

    // GET: api/Usuarios/5
    [HttpGet("{usuarioid}")]
    public async Task<ActionResult<Usuarios>> GetUsuarios(int usuarioid)
    {
        var usuarios = await _context.Usuarios.FindAsync(usuarioid);

        if (usuarios == null)
        {
            return NotFound();
        }

        return usuarios;
    }

    // PUT: api/Usuarios/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{usuarioid}")]
    public async Task<IActionResult> PutUsuarios(int? usuarioid, Usuarios usuarios)
    {
        if (usuarioid != usuarios.UsuarioId)
        {
            return BadRequest();
        }

        _context.Entry(usuarios).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UsuariosExists(usuarioid))
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

    // POST: api/Usuarios
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Usuarios>> PostUsuarios(Usuarios usuarios)
    {
        _context.Usuarios.Add(usuarios);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetUsuarios", new { usuarioid = usuarios.UsuarioId }, usuarios);
    }

    // DELETE: api/Usuarios/5
    [HttpDelete("{usuarioid}")]
    public async Task<IActionResult> DeleteUsuarios(int? usuarioid)
    {
        var usuarios = await _context.Usuarios.FindAsync(usuarioid);
        if (usuarios == null)
        {
            return NotFound();
        }

        _context.Usuarios.Remove(usuarios);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool UsuariosExists(int? usuarioid)
    {
        return _context.Usuarios.Any(e => e.UsuarioId == usuarioid);
    }
}
