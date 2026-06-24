using AppReservasAPI.Context;
using AppReservasAPI.DTOs.Perfil;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AppReservasAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PerfilController : ControllerBase
{
    private readonly AppDbContext _context;

    public PerfilController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Perfil
    [HttpGet]
    public async Task<IActionResult> GetMiPerfil()
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId == null) return Unauthorized("Token inválido.");

        var usuario = await _context.Usuarios
            .AsNoTracking()
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId.Value && u.Activo);

        if (usuario == null)
        {
            return Unauthorized("Usuario no encontrado o inactivo.");
        }

        return Ok(new
        {
            usuario = new
            {
                usuario.UsuarioId,
                usuario.Nombre,
                usuario.Email,
                usuario.Telefono,
                usuario.RolId,
                Rol = usuario.Rol == null ? null : usuario.Rol.Nombre,
                usuario.FechaRegistro,
                usuario.Activo
            }
        });
    }

    // PUT: api/Perfil
    [HttpPut]
    public async Task<IActionResult> ActualizarMiPerfil([FromBody] ActualizarPerfilDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var usuarioId = ObtenerUsuarioId();
        if (usuarioId == null) return Unauthorized("Token inválido.");

        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId.Value && u.Activo);

        if (usuario == null)
        {
            return Unauthorized("Usuario no encontrado o inactivo.");
        }

        var nombre = dto.Nombre.Trim();
        var email = dto.Email.Trim().ToLower();
        var telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim();

        var emailOcupado = await _context.Usuarios
            .AnyAsync(u => u.UsuarioId != usuario.UsuarioId && u.Email == email);

        if (emailOcupado)
        {
            return BadRequest("Ya existe otro usuario con ese correo.");
        }

        usuario.Nombre = nombre;
        usuario.Email = email;
        usuario.Telefono = telefono;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Perfil actualizado correctamente.",
            usuario = new
            {
                usuario.UsuarioId,
                usuario.Nombre,
                usuario.Email,
                usuario.Telefono,
                usuario.RolId,
                Rol = usuario.Rol == null ? null : usuario.Rol.Nombre,
                usuario.FechaRegistro,
                usuario.Activo
            }
        });
    }

    // PUT: api/Perfil/password
    [HttpPut("password")]
    public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordPerfilDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var usuarioId = ObtenerUsuarioId();
        if (usuarioId == null) return Unauthorized("Token inválido.");

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId.Value && u.Activo);

        if (usuario == null)
        {
            return Unauthorized("Usuario no encontrado o inactivo.");
        }

        var passwordActualValida = BCrypt.Net.BCrypt.Verify(dto.PasswordActual, usuario.PasswordHash);
        if (!passwordActualValida)
        {
            return BadRequest("La contraseña actual no es correcta.");
        }

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.PasswordNueva);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Contraseña actualizada correctamente." });
    }

    private int? ObtenerUsuarioId()
    {
        var value =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("UsuarioId") ??
            User.FindFirstValue("usuarioId") ??
            User.FindFirstValue("sub") ??
            User.FindFirstValue("nameid");

        return int.TryParse(value, out var usuarioId) ? usuarioId : null;
    }
}
