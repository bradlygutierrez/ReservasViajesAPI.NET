using AppReservasAPI.Context;
using AppReservasAPI.DTOs.Auth;
using AppReservasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AppReservasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var email = dto.Email.Trim().ToLower();

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (usuario == null)
            {
                return Unauthorized("Credenciales incorrectas.");
            }

            if (!usuario.Activo)
            {
                return Unauthorized("El usuario está desactivado.");
            }

            var passwordValida = BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash);

            if (!passwordValida)
            {
                return Unauthorized("Credenciales incorrectas.");
            }

            var token = GenerarJwtToken(usuario);

            var permisos = await ObtenerPermisosPorRol(usuario.RolId);

            return Ok(new
            {
                mensaje = "Login correcto.",
                token,
                tipo = "Bearer",
                usuario = new
                {
                    usuario.UsuarioId,
                    usuario.Nombre,
                    usuario.Email,
                    usuario.Telefono,
                    usuario.RolId,
                    Rol = usuario.Rol == null ? null : usuario.Rol.Nombre
                },
                permisos
            });
        }

        // POST: api/Auth/registro
        [HttpPost("registro")]
        [AllowAnonymous]
        public async Task<IActionResult> Registro([FromBody] RegistroDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var email = dto.Email.Trim().ToLower();

            var emailExiste = await _context.Usuarios
                .AnyAsync(u => u.Email == email);

            if (emailExiste)
            {
                return BadRequest("Ya existe un usuario registrado con ese correo.");
            }

            var rolCliente = await _context.Roles
                .FirstOrDefaultAsync(r => r.Nombre == "Cliente");

            if (rolCliente == null)
            {
                return StatusCode(500, "No existe el rol 'Cliente'. Crealo antes de permitir registros.");
            }

            var usuario = new Usuario
            {
                Nombre = dto.Nombre.Trim(),
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RolId = rolCliente.RolId,
                Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim(),
                FechaRegistro = DateTime.Now,
                Activo = true
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Usuario registrado correctamente.",
                usuario = new
                {
                    usuario.UsuarioId,
                    usuario.Nombre,
                    usuario.Email,
                    usuario.Telefono,
                    usuario.RolId,
                    Rol = rolCliente.Nombre,
                    usuario.FechaRegistro,
                    usuario.Activo
                }
            });
        }

        // GET: api/Auth/permisos/5
        [HttpGet("permisos/{usuarioId:int}")]
        [Authorize]
        public async Task<IActionResult> GetPermisosUsuario(int usuarioId)
        {
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId && u.Activo);

            if (usuario == null)
            {
                return NotFound("El usuario no existe o no está activo.");
            }

            var permisos = await ObtenerPermisosPorRol(usuario.RolId);

            return Ok(new
            {
                usuario = new
                {
                    usuario.UsuarioId,
                    usuario.Nombre,
                    usuario.Email,
                    usuario.RolId,
                    Rol = usuario.Rol == null ? null : usuario.Rol.Nombre
                },
                permisos
            });
        }

        // GET: api/Auth/me
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(usuarioIdClaim))
            {
                return Unauthorized("Token inválido.");
            }

            if (!int.TryParse(usuarioIdClaim, out int usuarioId))
            {
                return Unauthorized("Token inválido.");
            }

            var usuario = await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId && u.Activo);

            if (usuario == null)
            {
                return Unauthorized("Usuario no encontrado o inactivo.");
            }

            var permisos = await ObtenerPermisosPorRol(usuario.RolId);

            return Ok(new
            {
                usuario = new
                {
                    usuario.UsuarioId,
                    usuario.Nombre,
                    usuario.Email,
                    usuario.Telefono,
                    usuario.RolId,
                    Rol = usuario.Rol == null ? null : usuario.Rol.Nombre
                },
                permisos
            });
        }

        private async Task<object> ObtenerPermisosPorRol(int rolId)
        {
            var permisos = await _context.RolPantallaPermisos
                .AsNoTracking()
                .Where(rpp =>
                    rpp.RolId == rolId &&
                    rpp.Activo &&
                    rpp.Pantalla != null &&
                    rpp.Pantalla.Activo &&
                    rpp.Permiso != null &&
                    rpp.Permiso.Activo)
                .Select(rpp => new
                {
                    rpp.PantallaId,
                    Pantalla = rpp.Pantalla!.Nombre,
                    Ruta = rpp.Pantalla.Ruta,
                    Modulo = rpp.Pantalla.Modulo,
                    Icono = rpp.Pantalla.Icono,
                    Orden = rpp.Pantalla.Orden,
                    rpp.PermisoId,
                    CodigoPermiso = rpp.Permiso!.Codigo,
                    Permiso = rpp.Permiso.Nombre
                })
                .OrderBy(p => p.Orden)
                .ThenBy(p => p.Pantalla)
                .ToListAsync();

            return permisos;
        }

        private string GenerarJwtToken(Usuario usuario)
        {
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];
            var expiresMinutes = Convert.ToDouble(_configuration["Jwt:ExpiresMinutes"] ?? "120");

            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                throw new Exception("JWT Key no configurada.");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim("rolId", usuario.RolId.ToString())
            };

            if (usuario.Rol != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, usuario.Rol.Nombre));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}