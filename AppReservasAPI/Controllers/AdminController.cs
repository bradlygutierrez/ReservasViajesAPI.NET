using AppReservasAPI.Context;
using AppReservasAPI.DTOs.Admin;
using AppReservasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AppReservasAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Administrador,Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Admin/dashboard
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var totalUsuarios = await _context.Usuarios.CountAsync();
        var usuariosActivos = await _context.Usuarios.CountAsync(u => u.Activo);
        var totalViajes = await _context.Viajes.CountAsync();
        var viajesActivos = await _context.Viajes.CountAsync(v => v.Activo);
        var totalReservas = await _context.Reservas.CountAsync();
        var reservasPendientes = await _context.Reservas.CountAsync(r => r.EstadoReserva != null && r.EstadoReserva.Nombre == "Pendiente");
        var totalPagado = await _context.Pagos
            .Where(p => p.EstadoPago != null && p.EstadoPago.Nombre == "Pagado")
            .SumAsync(p => (decimal?)p.Monto) ?? 0;
        var quejasPendientes = await _context.Quejas.CountAsync(q => q.Estado == "Pendiente" || q.Estado == "En revisión");
        var totalLikes = await _context.ViajeLikes.CountAsync();
        var totalCompartidos = await _context.Compartidos.CountAsync();

        var ultimasReservas = await _context.Reservas
            .AsNoTracking()
            .Include(r => r.Usuario)
            .Include(r => r.EstadoReserva)
            .Include(r => r.Disponibilidad)
                .ThenInclude(d => d!.Viaje)
            .OrderByDescending(r => r.FechaReserva)
            .Take(5)
            .Select(r => new
            {
                r.ReservaId,
                r.FechaReserva,
                r.CantidadPersonas,
                r.Total,
                Estado = r.EstadoReserva == null ? null : r.EstadoReserva.Nombre,
                Usuario = r.Usuario == null ? null : r.Usuario.Nombre,
                Viaje = r.Disponibilidad == null || r.Disponibilidad.Viaje == null ? null : r.Disponibilidad.Viaje.Titulo
            })
            .ToListAsync();

        var viajesMasGustados = await _context.Viajes
            .AsNoTracking()
            .Select(v => new
            {
                v.ViajeId,
                v.Titulo,
                v.Precio,
                v.Activo,
                Likes = v.Likes.Count
            })
            .OrderByDescending(v => v.Likes)
            .ThenBy(v => v.Titulo)
            .Take(5)
            .ToListAsync();

        return Ok(new
        {
            totalUsuarios,
            usuariosActivos,
            totalViajes,
            viajesActivos,
            totalReservas,
            reservasPendientes,
            totalPagado,
            quejasPendientes,
            totalLikes,
            totalCompartidos,
            ultimasReservas,
            viajesMasGustados
        });
    }

    // GET: api/Admin/usuarios
    [HttpGet("usuarios")]
    public async Task<IActionResult> GetUsuarios()
    {
        var usuarios = await _context.Usuarios
            .AsNoTracking()
            .Include(u => u.Rol)
            .OrderByDescending(u => u.FechaRegistro)
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
                Reservas = u.Reservas.Count
            })
            .ToListAsync();

        return Ok(usuarios);
    }

    // PUT: api/Admin/usuarios/5/estado
    [HttpPut("usuarios/{usuarioId:int}/estado")]
    public async Task<IActionResult> CambiarEstadoUsuario(int usuarioId, [FromBody] CambiarEstadoUsuarioDto dto)
    {
        var currentUserId = ObtenerUsuarioId();
        if (currentUserId == usuarioId && !dto.Activo)
        {
            return BadRequest("No podés desactivar tu propio usuario administrador.");
        }

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);
        if (usuario == null) return NotFound("El usuario no existe.");

        usuario.Activo = dto.Activo;
        await _context.SaveChangesAsync();

        return Ok(new { usuario.UsuarioId, usuario.Activo });
    }

    // PUT: api/Admin/usuarios/5/rol
    [HttpPut("usuarios/{usuarioId:int}/rol")]
    public async Task<IActionResult> CambiarRolUsuario(int usuarioId, [FromBody] CambiarRolUsuarioDto dto)
    {
        var rolExiste = await _context.Roles.AnyAsync(r => r.RolId == dto.RolId);
        if (!rolExiste) return BadRequest("El rol no existe.");

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);
        if (usuario == null) return NotFound("El usuario no existe.");

        usuario.RolId = dto.RolId;
        await _context.SaveChangesAsync();

        return Ok(new { usuario.UsuarioId, usuario.RolId });
    }

    // GET: api/Admin/roles
    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _context.Roles
            .AsNoTracking()
            .OrderBy(r => r.Nombre)
            .Select(r => new
            {
                r.RolId,
                r.Nombre,
                Usuarios = r.Usuarios.Count,
                Permisos = r.RolPantallaPermisos.Count
            })
            .ToListAsync();

        return Ok(roles);
    }


    // GET: api/Admin/viajes
    [HttpGet("viajes")]
    public async Task<IActionResult> GetViajes()
    {
        var viajes = await _context.Viajes
            .AsNoTracking()
            .Include(v => v.PublicadoPorUsuario)
            .Include(v => v.TipoViaje)
            .Include(v => v.Destino)
                .ThenInclude(d => d!.Ciudad)
                    .ThenInclude(c => c!.Pais)
            .Include(v => v.Disponibilidades)
                .ThenInclude(d => d.Reservas)
            .Include(v => v.Likes)
            .Include(v => v.Compartidos)
            .OrderByDescending(v => v.FechaCreacion)
            .ToListAsync();

        return Ok(viajes.Select(v => new
        {
            v.ViajeId,
            v.Titulo,
            v.Descripcion,
            v.Precio,
            v.CuposTotales,
            v.Activo,
            v.EstadoPublicacion,
            ImagenUrl = ToAbsoluteImageUrl(v.ImagenUrl),
            v.FechaCreacion,
            v.FechaActualizacion,
            PublicadoPor = v.PublicadoPorUsuario == null ? null : new
            {
                v.PublicadoPorUsuario.UsuarioId,
                v.PublicadoPorUsuario.Nombre,
                v.PublicadoPorUsuario.Email
            },
            TipoViaje = v.TipoViaje == null ? null : v.TipoViaje.Nombre,
            Ciudad = v.Destino == null || v.Destino.Ciudad == null ? null : v.Destino.Ciudad.Nombre,
            Pais = v.Destino == null || v.Destino.Ciudad == null || v.Destino.Ciudad.Pais == null ? null : v.Destino.Ciudad.Pais.Nombre,
            Reservas = v.Disponibilidades.SelectMany(d => d.Reservas).Count(),
            Likes = v.Likes.Count,
            Compartidos = v.Compartidos.Count
        }));
    }

    // PUT: api/Admin/viajes/5/estado
    [HttpPut("viajes/{viajeId:int}/estado")]
    public async Task<IActionResult> CambiarEstadoViaje(int viajeId, [FromBody] CambiarEstadoViajeAdminDto dto)
    {
        var viaje = await _context.Viajes
            .Include(v => v.Disponibilidades)
            .FirstOrDefaultAsync(v => v.ViajeId == viajeId);

        if (viaje == null) return NotFound("El viaje no existe.");

        viaje.Activo = dto.Activo;
        viaje.EstadoPublicacion = dto.Activo ? "Publicado" : "Pausado";
        viaje.FechaActualizacion = DateTime.Now;

        foreach (var disponibilidad in viaje.Disponibilidades)
        {
            disponibilidad.Activo = dto.Activo;
        }

        await _context.SaveChangesAsync();
        return Ok(new { viaje.ViajeId, viaje.Activo, viaje.EstadoPublicacion });
    }

    // GET: api/Admin/reservas
    [HttpGet("reservas")]
    public async Task<IActionResult> GetReservas()
    {
        var reservas = await _context.Reservas
            .AsNoTracking()
            .Include(r => r.Usuario)
            .Include(r => r.EstadoReserva)
            .Include(r => r.Disponibilidad)
                .ThenInclude(d => d!.Viaje)
            .Include(r => r.Pagos)
            .OrderByDescending(r => r.FechaReserva)
            .Select(r => new
            {
                r.ReservaId,
                r.UsuarioId,
                Usuario = r.Usuario == null ? null : new
                {
                    r.Usuario.UsuarioId,
                    r.Usuario.Nombre,
                    r.Usuario.Email
                },
                r.EstadoReservaId,
                Estado = r.EstadoReserva == null ? null : r.EstadoReserva.Nombre,
                r.FechaReserva,
                r.CantidadPersonas,
                r.PrecioUnitario,
                r.Total,
                TotalPagado = r.Pagos.Sum(p => p.Monto),
                Viaje = r.Disponibilidad == null || r.Disponibilidad.Viaje == null ? null : new
                {
                    r.Disponibilidad.Viaje.ViajeId,
                    r.Disponibilidad.Viaje.Titulo,
                    Fecha = r.Disponibilidad.Fecha,
                    FechaRetorno = r.Disponibilidad.FechaRetorno
                }
            })
            .ToListAsync();

        return Ok(reservas);
    }

    // PUT: api/Admin/reservas/5/estado
    [HttpPut("reservas/{reservaId:int}/estado")]
    public async Task<IActionResult> CambiarEstadoReserva(int reservaId, [FromBody] CambiarEstadoReservaAdminDto dto)
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId == null) return Unauthorized("Token inválido.");

        var estadoExiste = await _context.EstadosReserva.AnyAsync(e => e.EstadoReservaId == dto.EstadoReservaId && e.Activo);
        if (!estadoExiste) return BadRequest("El estado de reserva no existe o no está activo.");

        var reserva = await _context.Reservas.FirstOrDefaultAsync(r => r.ReservaId == reservaId);
        if (reserva == null) return NotFound("La reserva no existe.");

        var estadoAnteriorId = reserva.EstadoReservaId;
        reserva.EstadoReservaId = dto.EstadoReservaId;
        reserva.FechaActualizacion = DateTime.Now;

        _context.HistorialEstadosReserva.Add(new HistorialEstadoReserva
        {
            ReservaId = reserva.ReservaId,
            UsuarioId = usuarioId.Value,
            EstadoAnteriorId = estadoAnteriorId,
            EstadoNuevoId = dto.EstadoReservaId,
            Motivo = string.IsNullOrWhiteSpace(dto.Motivo) ? "Cambio realizado desde panel admin." : dto.Motivo.Trim(),
            FechaCambio = DateTime.Now
        });

        await _context.SaveChangesAsync();
        return Ok(new { reserva.ReservaId, reserva.EstadoReservaId });
    }

    // GET: api/Admin/estados-reserva
    [HttpGet("estados-reserva")]
    public async Task<IActionResult> GetEstadosReserva()
    {
        var estados = await _context.EstadosReserva
            .AsNoTracking()
            .Where(e => e.Activo)
            .OrderBy(e => e.Nombre)
            .Select(e => new { e.EstadoReservaId, e.Nombre })
            .ToListAsync();

        return Ok(estados);
    }

    private string? ToAbsoluteImageUrl(string? imagenUrl)
    {
        if (string.IsNullOrWhiteSpace(imagenUrl)) return null;
        if (imagenUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return imagenUrl;
        return $"{Request.Scheme}://{Request.Host}{imagenUrl}";
    }

    private int? ObtenerUsuarioId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var usuarioId) ? usuarioId : null;
    }
}
