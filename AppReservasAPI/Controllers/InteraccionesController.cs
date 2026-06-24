using AppReservasAPI.Context;
using AppReservasAPI.DTOs.Interacciones;
using AppReservasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AppReservasAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class InteraccionesController : ControllerBase
{
    private readonly AppDbContext _context;

    public InteraccionesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Interacciones/likes
    [HttpGet("likes")]
    public async Task<IActionResult> GetMisLikes()
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId == null) return Unauthorized("Token inválido.");

        var likes = await _context.ViajeLikes
            .AsNoTracking()
            .Where(l => l.UsuarioId == usuarioId.Value)
            .Include(l => l.Viaje)
                .ThenInclude(v => v!.Destino)
                    .ThenInclude(d => d!.Ciudad)
                        .ThenInclude(c => c!.Pais)
            .Include(l => l.Viaje)
                .ThenInclude(v => v!.TipoViaje)
            .Include(l => l.Viaje)
                .ThenInclude(v => v!.Disponibilidades)
            .OrderByDescending(l => l.FechaCreacion)
            .ToListAsync();

        return Ok(likes.Select(l => new
        {
            l.ViajeLikeId,
            l.ViajeId,
            l.FechaCreacion,
            Viaje = l.Viaje == null ? null : new
            {
                l.Viaje.ViajeId,
                l.Viaje.Titulo,
                l.Viaje.Descripcion,
                l.Viaje.Precio,
                l.Viaje.CuposTotales,
                l.Viaje.Activo,
                l.Viaje.EstadoPublicacion,
                ImagenUrl = ToAbsoluteImageUrl(l.Viaje.ImagenUrl),
                TipoViaje = l.Viaje.TipoViaje == null ? null : l.Viaje.TipoViaje.Nombre,
                Destino = l.Viaje.Destino == null ? null : new
                {
                    l.Viaje.Destino.DestinoId,
                    l.Viaje.Destino.Descripcion,
                    Ciudad = l.Viaje.Destino.Ciudad == null ? null : new
                    {
                        l.Viaje.Destino.Ciudad.CiudadId,
                        l.Viaje.Destino.Ciudad.Nombre,
                        Pais = l.Viaje.Destino.Ciudad.Pais == null ? null : new
                        {
                            l.Viaje.Destino.Ciudad.Pais.PaisId,
                            l.Viaje.Destino.Ciudad.Pais.Nombre
                        }
                    }
                },
                Disponibilidad = l.Viaje.Disponibilidades
                    .Where(d => d.Activo)
                    .OrderBy(d => d.Fecha)
                    .Select(d => new
                    {
                        d.DisponibilidadId,
                        d.Fecha,
                        d.FechaRetorno,
                        d.CuposTotales,
                        d.CuposDisponibles,
                        d.Activo
                    })
                    .FirstOrDefault()
            }
        }));
    }

    // GET: api/Interacciones/likes/5/estado
    [HttpGet("likes/{viajeId:int}/estado")]
    public async Task<IActionResult> GetLikeEstado(int viajeId)
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId == null) return Unauthorized("Token inválido.");

        var liked = await _context.ViajeLikes
            .AnyAsync(l => l.UsuarioId == usuarioId.Value && l.ViajeId == viajeId);

        var totalLikes = await _context.ViajeLikes.CountAsync(l => l.ViajeId == viajeId);

        return Ok(new { viajeId, liked, totalLikes });
    }

    // POST: api/Interacciones/likes/5/toggle
    [HttpPost("likes/{viajeId:int}/toggle")]
    public async Task<IActionResult> ToggleLike(int viajeId)
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId == null) return Unauthorized("Token inválido.");

        var viajeExiste = await _context.Viajes.AnyAsync(v => v.ViajeId == viajeId);
        if (!viajeExiste) return NotFound("El viaje no existe.");

        var like = await _context.ViajeLikes
            .FirstOrDefaultAsync(l => l.UsuarioId == usuarioId.Value && l.ViajeId == viajeId);

        var liked = false;

        if (like == null)
        {
            _context.ViajeLikes.Add(new ViajeLike
            {
                UsuarioId = usuarioId.Value,
                ViajeId = viajeId,
                FechaCreacion = DateTime.Now
            });

            liked = true;
        }
        else
        {
            _context.ViajeLikes.Remove(like);
        }

        await _context.SaveChangesAsync();

        var totalLikes = await _context.ViajeLikes.CountAsync(l => l.ViajeId == viajeId);
        return Ok(new { viajeId, liked, totalLikes });
    }

    // POST: api/Interacciones/compartidos
    [HttpPost("compartidos")]
    public async Task<IActionResult> RegistrarCompartido([FromBody] RegistrarCompartidoDto dto)
    {
        var usuarioId = ObtenerUsuarioId();

        if (dto.ViajeId == null && dto.ReservaId == null)
        {
            return BadRequest("Debe indicar ViajeId o ReservaId.");
        }

        if (dto.ViajeId.HasValue)
        {
            var viajeExiste = await _context.Viajes.AnyAsync(v => v.ViajeId == dto.ViajeId.Value);
            if (!viajeExiste) return NotFound("El viaje no existe.");
        }

        if (dto.ReservaId.HasValue)
        {
            var reservaExiste = await _context.Reservas.AnyAsync(r => r.ReservaId == dto.ReservaId.Value);
            if (!reservaExiste) return NotFound("La reserva no existe.");
        }

        var compartido = new Compartido
        {
            UsuarioId = usuarioId,
            ViajeId = dto.ViajeId,
            ReservaId = dto.ReservaId,
            Canal = string.IsNullOrWhiteSpace(dto.Canal) ? "web" : dto.Canal.Trim(),
            Url = string.IsNullOrWhiteSpace(dto.Url) ? null : dto.Url.Trim(),
            FechaCreacion = DateTime.Now
        };

        _context.Compartidos.Add(compartido);
        await _context.SaveChangesAsync();

        return Ok(new { compartido.CompartidoId });
    }

    private string? ToAbsoluteImageUrl(string? imagenUrl)
    {
        if (string.IsNullOrWhiteSpace(imagenUrl)) return null;
        if (imagenUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return imagenUrl;
        return $"{Request.Scheme}://{Request.Host}{imagenUrl}";
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
