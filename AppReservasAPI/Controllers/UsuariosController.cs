using AppReservasAPI.Context;
using AppReservasAPI.DTOs.Usuarios;
using AppReservasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppReservasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Usuarios
        [HttpGet]
        public async Task<IActionResult> GetUsuarios()
        {
            var usuarios = await _context.Usuarios
                .AsNoTracking()
                .Select(u => new
                {
                    u.UsuarioId,
                    u.Nombre,
                    u.Email,
                    u.Telefono,
                    u.RolId,
                    Rol = u.Rol == null ? null : u.Rol.Nombre,
                    u.FechaRegistro,
                    u.Activo
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        // GET: api/Usuarios/5
        [HttpGet("{usuarioId:int}")]
        public async Task<IActionResult> GetUsuario(int usuarioId)
        {
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .Where(u => u.UsuarioId == usuarioId)
                .Select(u => new
                {
                    u.UsuarioId,
                    u.Nombre,
                    u.Email,
                    u.Telefono,
                    u.RolId,
                    Rol = u.Rol == null ? null : u.Rol.Nombre,
                    u.FechaRegistro,
                    u.Activo,
                    Reservas = u.Reservas.Select(r => new
                    {
                        r.ReservaId,
                        r.FechaReserva,
                        r.CantidadPersonas,
                        r.Total,
                        Estado = r.EstadoReserva == null ? null : r.EstadoReserva.Nombre
                    })
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
            {
                return NotFound("El usuario no existe.");
            }

            return Ok(usuario);
        }

        // POST: api/Usuarios
        [HttpPost]
        public async Task<IActionResult> CrearUsuario([FromBody] CrearUsuarioDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var rolExiste = await _context.Roles
                .AnyAsync(r => r.RolId == dto.RolId);

            if (!rolExiste)
            {
                return BadRequest("El rol indicado no existe.");
            }

            var emailExiste = await _context.Usuarios
                .AnyAsync(u => u.Email == dto.Email);

            if (emailExiste)
            {
                return BadRequest("Ya existe un usuario registrado con ese correo.");
            }

            var usuario = new Usuario
            {
                Nombre = dto.Nombre.Trim(),
                Email = dto.Email.Trim().ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RolId = dto.RolId,
                Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim(),
                FechaRegistro = DateTime.Now,
                Activo = true
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetUsuario),
                new { usuarioId = usuario.UsuarioId },
                new
                {
                    usuario.UsuarioId,
                    usuario.Nombre,
                    usuario.Email,
                    usuario.Telefono,
                    usuario.RolId,
                    usuario.FechaRegistro,
                    usuario.Activo
                }
            );
        }

        // PUT: api/Usuarios/5
        [HttpPut("{usuarioId:int}")]
        public async Task<IActionResult> ActualizarUsuario(int usuarioId, [FromBody] ActualizarUsuarioDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

            if (usuario == null)
            {
                return NotFound("El usuario no existe.");
            }

            var rolExiste = await _context.Roles
                .AnyAsync(r => r.RolId == dto.RolId);

            if (!rolExiste)
            {
                return BadRequest("El rol indicado no existe.");
            }

            var emailEnUso = await _context.Usuarios
                .AnyAsync(u => u.Email == dto.Email && u.UsuarioId != usuarioId);

            if (emailEnUso)
            {
                return BadRequest("Otro usuario ya está usando ese correo.");
            }

            usuario.Nombre = dto.Nombre.Trim();
            usuario.Email = dto.Email.Trim().ToLower();
            usuario.RolId = dto.RolId;
            usuario.Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim();
            usuario.Activo = dto.Activo;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                usuario.UsuarioId,
                usuario.Nombre,
                usuario.Email,
                usuario.Telefono,
                usuario.RolId,
                usuario.Activo
            });
        }

        // PUT: api/Usuarios/5/password
        [HttpPut("{usuarioId:int}/password")]
        public async Task<IActionResult> CambiarPassword(int usuarioId, [FromBody] CambiarPasswordUsuarioDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

            if (usuario == null)
            {
                return NotFound("El usuario no existe.");
            }

            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NuevaPassword);

            await _context.SaveChangesAsync();

            return Ok("Contraseña actualizada correctamente.");
        }

        // PUT: api/Usuarios/5/activar
        [HttpPut("{usuarioId:int}/activar")]
        public async Task<IActionResult> ActivarUsuario(int usuarioId)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

            if (usuario == null)
            {
                return NotFound("El usuario no existe.");
            }

            usuario.Activo = true;
            await _context.SaveChangesAsync();

            return Ok("Usuario activado correctamente.");
        }

        // PUT: api/Usuarios/5/desactivar
        [HttpPut("{usuarioId:int}/desactivar")]
        public async Task<IActionResult> DesactivarUsuario(int usuarioId)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

            if (usuario == null)
            {
                return NotFound("El usuario no existe.");
            }

            usuario.Activo = false;
            await _context.SaveChangesAsync();

            return Ok("Usuario desactivado correctamente.");
        }

        // DELETE: api/Usuarios/5
        [HttpDelete("{usuarioId:int}")]
        public IActionResult DeleteUsuario(int usuarioId)
        {
            return BadRequest("No se recomienda eliminar usuarios físicamente. Usá PUT /api/Usuarios/{usuarioId}/desactivar.");
        }
    }
}