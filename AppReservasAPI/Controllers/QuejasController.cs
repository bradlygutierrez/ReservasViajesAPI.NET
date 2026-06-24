using AppReservasAPI.Context;
using AppReservasAPI.DTOs.Quejas;
using AppReservasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AppReservasAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class QuejasController : ControllerBase
{
    private readonly AppDbContext _context;

    public QuejasController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Quejas/mis-quejas
    [HttpGet("mis-quejas")]
    public async Task<IActionResult> GetMisQuejas()
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId == null) return Unauthorized("Token inválido.");

        var quejas = await QueryBase()
            .Where(q => q.UsuarioId == usuarioId.Value)
            .OrderByDescending(q => q.FechaCreacion)
            .ToListAsync();

        return Ok(quejas.Select(ProyectarQueja));
    }

    // POST: api/Quejas
    [HttpPost]
    public async Task<IActionResult> CrearQueja([FromBody] CrearQuejaDto dto)
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId == null) return Unauthorized("Token inválido.");

        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (dto.ReservaId.HasValue)
        {
            var reservaExiste = await _context.Reservas
                .AnyAsync(r => r.ReservaId == dto.ReservaId.Value && r.UsuarioId == usuarioId.Value);

            if (!reservaExiste)
            {
                return BadRequest("La reserva no existe o no pertenece al usuario autenticado.");
            }
        }

        if (dto.ViajeId.HasValue)
        {
            var viajeExiste = await _context.Viajes.AnyAsync(v => v.ViajeId == dto.ViajeId.Value);
            if (!viajeExiste) return BadRequest("El viaje no existe.");
        }

        var queja = new Queja
        {
            UsuarioId = usuarioId.Value,
            ReservaId = dto.ReservaId,
            ViajeId = dto.ViajeId,
            Tipo = string.IsNullOrWhiteSpace(dto.Tipo) ? "General" : dto.Tipo.Trim(),
            Asunto = dto.Asunto.Trim(),
            Descripcion = dto.Descripcion.Trim(),
            Estado = "Pendiente",
            FechaCreacion = DateTime.Now
        };

        _context.Quejas.Add(queja);
        await _context.SaveChangesAsync();

        var creada = await QueryBase().FirstAsync(q => q.QuejaId == queja.QuejaId);
        return CreatedAtAction(nameof(GetMisQuejas), new { quejaId = queja.QuejaId }, ProyectarQueja(creada));
    }

    // GET: api/Quejas/admin
    [HttpGet("admin")]
    [Authorize(Roles = "Administrador,Admin")]
    public async Task<IActionResult> GetAdminQuejas([FromQuery] string? estado)
    {
        var query = QueryBase();

        if (!string.IsNullOrWhiteSpace(estado))
        {
            var value = estado.Trim();
            query = query.Where(q => q.Estado == value);
        }

        var quejas = await query
            .OrderByDescending(q => q.FechaCreacion)
            .ToListAsync();

        return Ok(quejas.Select(ProyectarQueja));
    }

    // PUT: api/Quejas/5/resolver
    [HttpPut("{quejaId:int}/resolver")]
    [Authorize(Roles = "Administrador,Admin")]
    public async Task<IActionResult> ResolverQueja(int quejaId, [FromBody] ResolverQuejaDto dto)
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId == null) return Unauthorized("Token inválido.");

        if (!ModelState.IsValid) return BadRequest(ModelState);

        var estadosValidos = new[] { "Pendiente", "En revisión", "Resuelta", "Cerrada" };
        if (!estadosValidos.Contains(dto.Estado))
        {
            return BadRequest("Estado inválido. Usá: Pendiente, En revisión, Resuelta o Cerrada.");
        }

        var queja = await _context.Quejas.FirstOrDefaultAsync(q => q.QuejaId == quejaId);
        if (queja == null) return NotFound("La queja no existe.");

        queja.Estado = dto.Estado;
        queja.RespuestaAdmin = string.IsNullOrWhiteSpace(dto.RespuestaAdmin) ? null : dto.RespuestaAdmin.Trim();
        queja.AtendidoPorUsuarioId = usuarioId.Value;
        queja.FechaActualizacion = DateTime.Now;

        await _context.SaveChangesAsync();
        return Ok(new { queja.QuejaId, queja.Estado, queja.RespuestaAdmin });
    }

    private IQueryable<Queja> QueryBase()
    {
        return _context.Quejas
            .AsNoTracking()
            .Include(q => q.Usuario)
            .Include(q => q.AtendidoPorUsuario)
            .Include(q => q.Reserva)
                .ThenInclude(r => r!.EstadoReserva)
            .Include(q => q.Reserva)
                .ThenInclude(r => r!.Disponibilidad)
                    .ThenInclude(d => d!.Viaje)
            .Include(q => q.Viaje);
    }

    private object ProyectarQueja(Queja q)
    {
        return new
        {
            q.QuejaId,
            q.UsuarioId,
            Usuario = q.Usuario == null ? null : new
            {
                q.Usuario.UsuarioId,
                q.Usuario.Nombre,
                q.Usuario.Email
            },
            q.ReservaId,
            q.ViajeId,
            q.Tipo,
            q.Asunto,
            q.Descripcion,
            q.Estado,
            q.RespuestaAdmin,
            q.FechaCreacion,
            q.FechaActualizacion,
            AtendidoPor = q.AtendidoPorUsuario == null ? null : new
            {
                q.AtendidoPorUsuario.UsuarioId,
                q.AtendidoPorUsuario.Nombre,
                q.AtendidoPorUsuario.Email
            },
            Reserva = q.Reserva == null ? null : new
            {
                q.Reserva.ReservaId,
                Estado = q.Reserva.EstadoReserva == null ? null : q.Reserva.EstadoReserva.Nombre,
                Viaje = q.Reserva.Disponibilidad?.Viaje == null ? null : new
                {
                    q.Reserva.Disponibilidad.Viaje.ViajeId,
                    q.Reserva.Disponibilidad.Viaje.Titulo
                }
            },
            Viaje = q.Viaje == null ? null : new
            {
                q.Viaje.ViajeId,
                q.Viaje.Titulo
            }
        };
    }

    private int? ObtenerUsuarioId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var usuarioId) ? usuarioId : null;
    }
}
