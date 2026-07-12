using AppReservasAPI.Context;
using AppReservasAPI.DTOs.Viajes;
using AppReservasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppReservasAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ViajesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ViajesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Viajes
    [HttpGet]
    public async Task<IActionResult> GetViajes(
        [FromQuery] bool? activo,
        [FromQuery] int? tipoViajeId,
        [FromQuery] int? destinoId,
        [FromQuery] string? busqueda)
    {
        var query = _context.Viajes.AsNoTracking().AsQueryable();

        if (activo.HasValue)
        {
            query = query.Where(v => v.Activo == activo.Value);
        }

        if (tipoViajeId.HasValue)
        {
            query = query.Where(v => v.TipoViajeId == tipoViajeId.Value);
        }

        if (destinoId.HasValue)
        {
            query = query.Where(v => v.DestinoId == destinoId.Value);
        }

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var texto = busqueda.Trim();
            query = query.Where(v =>
                v.Titulo.Contains(texto) ||
                (v.Descripcion != null && v.Descripcion.Contains(texto)));
        }

        var viajes = await query
            .Select(v => new
            {
                v.ViajeId,
                v.TipoViajeId,
                TipoViaje = v.TipoViaje == null ? null : v.TipoViaje.Nombre,
                v.DestinoId,
                Destino = v.Destino == null ? null : new
                {
                    v.Destino.DestinoId,
                    v.Destino.Descripcion,
                    Ciudad = v.Destino.Ciudad == null ? null : v.Destino.Ciudad.Nombre,
                    Pais = v.Destino.Ciudad == null || v.Destino.Ciudad.Pais == null
                        ? null
                        : v.Destino.Ciudad.Pais.Nombre
                },
                v.Titulo,
                v.Descripcion,
                v.Precio,
                v.CuposTotales,
                v.Activo,
                v.EstadoPublicacion,
                ImagenUrl = ToAbsoluteImageUrl(v.ImagenUrl),
                v.PublicadoPorUsuarioId,
                v.FechaCreacion,
                v.FechaActualizacion,
                DisponibilidadesActivas = v.Disponibilidades.Count(d => d.Activo)
            })
            .OrderBy(v => v.Titulo)
            .ToListAsync();

        return Ok(viajes);
    }

    // GET: api/Viajes/5
    [HttpGet("{viajeId:int}")]
    public async Task<IActionResult> GetViaje(int viajeId)
    {
        var viaje = await _context.Viajes
            .AsNoTracking()
            .Where(v => v.ViajeId == viajeId)
            .Select(v => new
            {
                v.ViajeId,
                v.TipoViajeId,

                TipoViaje = v.TipoViaje == null
                    ? null
                    : new
                    {
                        v.TipoViaje.TipoViajeId,
                        v.TipoViaje.Nombre,
                        v.TipoViaje.Descripcion
                    },

                v.DestinoId,

                Destino = v.Destino == null
                    ? null
                    : new
                    {
                        v.Destino.DestinoId,
                        v.Destino.Descripcion,

                        Ciudad = v.Destino.Ciudad == null
                            ? null
                            : new
                            {
                                v.Destino.Ciudad.CiudadId,
                                v.Destino.Ciudad.Nombre,

                                Pais = v.Destino.Ciudad.Pais == null
                                    ? null
                                    : new
                                    {
                                        v.Destino.Ciudad.Pais.PaisId,
                                        v.Destino.Ciudad.Pais.Nombre
                                    }
                            }
                    },

                v.Titulo,
                v.Descripcion,
                v.Precio,
                v.CuposTotales,
                v.Activo,
                v.EstadoPublicacion,
                ImagenUrl = ToAbsoluteImageUrl(v.ImagenUrl),
                v.PublicadoPorUsuarioId,
                v.FechaCreacion,
                v.FechaActualizacion,

                Disponibilidades = v.Disponibilidades
                    .OrderBy(d => d.Fecha)
                    .Select(d => new
                    {
                        d.DisponibilidadId,
                        d.Fecha,
                        d.FechaRetorno,
                        d.CuposTotales,
                        d.CuposDisponibles,
                        CuposOcupados =
                            d.CuposTotales - d.CuposDisponibles,
                        d.Activo
                    })
                    .ToList(),

                Inclusiones = v.Inclusiones
                    .Where(i => i.Activo)
                    .OrderBy(i => i.Orden)
                    .ThenBy(i => i.ViajeInclusionId)
                    .Select(i => new
                    {
                        i.ViajeInclusionId,
                        i.Tipo,
                        i.Titulo,
                        i.Detalle,
                        i.Orden
                    })
                    .ToList(),

                Itinerario = v.Itinerarios
                    .Where(i => i.Activo)
                    .OrderBy(i => i.Dia)
                    .ThenBy(i => i.Orden)
                    .ThenBy(i => i.ViajeItinerarioId)
                    .Select(i => new
                    {
                        i.ViajeItinerarioId,
                        i.Dia,
                        i.Titulo,
                        i.Descripcion,
                        i.Sitios,
                        i.Orden
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (viaje == null)
        {
            return NotFound("El viaje no existe.");
        }

        return Ok(viaje);
    }

    // POST: api/Viajes
    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> CrearViaje([FromBody] CrearViajeDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var tipoViajeExiste = await _context.TiposViaje
            .AnyAsync(t => t.TipoViajeId == dto.TipoViajeId && t.Activo);

        if (!tipoViajeExiste)
        {
            return BadRequest("El tipo de viaje no existe o no está activo.");
        }

        var destinoExiste = await _context.Destinos
            .AnyAsync(d => d.DestinoId == dto.DestinoId && d.Activo);

        if (!destinoExiste)
        {
            return BadRequest("El destino no existe o no está activo.");
        }

        var titulo = dto.Titulo.Trim();
        var viajeDuplicado = await _context.Viajes
            .AnyAsync(v => v.Titulo == titulo && v.TipoViajeId == dto.TipoViajeId && v.DestinoId == dto.DestinoId);

        if (viajeDuplicado)
        {
            return BadRequest("Ya existe un viaje con ese título, tipo de viaje y destino.");
        }

        var ahora = DateTime.Now;
        var viaje = new Viaje
        {
            TipoViajeId = dto.TipoViajeId,
            DestinoId = dto.DestinoId,
            Titulo = titulo,
            Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim(),
            Precio = dto.Precio,
            CuposTotales = dto.CuposTotales,
            Activo = true,
            EstadoPublicacion = "Publicado",
            FechaCreacion = ahora,
            FechaActualizacion = ahora
        };

        _context.Viajes.Add(viaje);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetViaje),
            new { viajeId = viaje.ViajeId },
            new
            {
                viaje.ViajeId,
                viaje.TipoViajeId,
                viaje.DestinoId,
                viaje.Titulo,
                viaje.Descripcion,
                viaje.Precio,
                viaje.CuposTotales,
                viaje.Activo,
                viaje.EstadoPublicacion,
                viaje.ImagenUrl,
                viaje.PublicadoPorUsuarioId,
                viaje.FechaCreacion,
                viaje.FechaActualizacion
            });
    }

    // PUT: api/Viajes/5
    [HttpPut("{viajeId:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ActualizarViaje(int viajeId, [FromBody] ActualizarViajeDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var viaje = await _context.Viajes.FirstOrDefaultAsync(v => v.ViajeId == viajeId);
        if (viaje == null)
        {
            return NotFound("El viaje no existe.");
        }

        var tipoViajeExiste = await _context.TiposViaje
            .AnyAsync(t => t.TipoViajeId == dto.TipoViajeId && t.Activo);

        if (!tipoViajeExiste)
        {
            return BadRequest("El tipo de viaje no existe o no está activo.");
        }

        var destinoExiste = await _context.Destinos
            .AnyAsync(d => d.DestinoId == dto.DestinoId && d.Activo);

        if (!destinoExiste)
        {
            return BadRequest("El destino no existe o no está activo.");
        }

        var maxCuposDisponibilidad = await _context.Disponibilidades
            .Where(d => d.ViajeId == viajeId)
            .Select(d => (int?)d.CuposTotales)
            .MaxAsync() ?? 0;

        if (dto.CuposTotales < maxCuposDisponibilidad)
        {
            return BadRequest($"No podés poner cupos totales menores que una disponibilidad ya creada. Cupos máximos en disponibilidades: {maxCuposDisponibilidad}.");
        }

        var titulo = dto.Titulo.Trim();
        var viajeDuplicado = await _context.Viajes
            .AnyAsync(v => v.ViajeId != viajeId && v.Titulo == titulo && v.TipoViajeId == dto.TipoViajeId && v.DestinoId == dto.DestinoId);

        if (viajeDuplicado)
        {
            return BadRequest("Ya existe otro viaje con ese título, tipo de viaje y destino.");
        }

        viaje.TipoViajeId = dto.TipoViajeId;
        viaje.DestinoId = dto.DestinoId;
        viaje.Titulo = titulo;
        viaje.Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim();
        viaje.Precio = dto.Precio;
        viaje.CuposTotales = dto.CuposTotales;
        viaje.Activo = dto.Activo;
        viaje.EstadoPublicacion = dto.Activo ? "Publicado" : "Pausado";
        viaje.FechaActualizacion = DateTime.Now;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            viaje.ViajeId,
            viaje.TipoViajeId,
            viaje.DestinoId,
            viaje.Titulo,
            viaje.Descripcion,
            viaje.Precio,
            viaje.CuposTotales,
            viaje.Activo,
            viaje.EstadoPublicacion,
            ImagenUrl = ToAbsoluteImageUrl(viaje.ImagenUrl),
            viaje.PublicadoPorUsuarioId,
            viaje.FechaCreacion,
            viaje.FechaActualizacion
        });
    }

    // PUT: api/Viajes/5/activar
    [HttpPut("{viajeId:int}/activar")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ActivarViaje(int viajeId)
    {
        var viaje = await _context.Viajes.FirstOrDefaultAsync(v => v.ViajeId == viajeId);
        if (viaje == null)
        {
            return NotFound("El viaje no existe.");
        }

        viaje.Activo = true;
        viaje.EstadoPublicacion = "Publicado";
        viaje.FechaActualizacion = DateTime.Now;
        await _context.SaveChangesAsync();

        return Ok("Viaje activado correctamente.");
    }

    // PUT: api/Viajes/5/desactivar
    [HttpPut("{viajeId:int}/desactivar")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DesactivarViaje(int viajeId)
    {
        var viaje = await _context.Viajes.FirstOrDefaultAsync(v => v.ViajeId == viajeId);
        if (viaje == null)
        {
            return NotFound("El viaje no existe.");
        }

        viaje.Activo = false;
        viaje.EstadoPublicacion = "Pausado";
        viaje.FechaActualizacion = DateTime.Now;
        await _context.SaveChangesAsync();

        return Ok("Viaje desactivado correctamente.");
    }

    // DELETE: api/Viajes/5
    [HttpDelete("{viajeId:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeleteViaje(int viajeId)
    {
        var viaje = await _context.Viajes.FirstOrDefaultAsync(v => v.ViajeId == viajeId);
        if (viaje == null)
        {
            return NotFound("El viaje no existe.");
        }

        var tieneDisponibilidades = await _context.Disponibilidades.AnyAsync(d => d.ViajeId == viajeId);
        if (tieneDisponibilidades)
        {
            viaje.Activo = false;
            viaje.EstadoPublicacion = "Pausado";
            viaje.FechaActualizacion = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok("El viaje tiene disponibilidades asociadas. No se eliminó físicamente; fue desactivado.");
        }

        _context.Viajes.Remove(viaje);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static string? ToAbsoluteImageUrl(string? imagenUrl)
    {
        return imagenUrl;
    }
}
