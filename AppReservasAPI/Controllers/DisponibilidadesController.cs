using AppReservasAPI.Context;
using AppReservasAPI.DTOs.Disponibilidades;
using AppReservasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppReservasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DisponibilidadesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DisponibilidadesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Disponibilidades
        [HttpGet]
        public async Task<IActionResult> GetDisponibilidades()
        {
            var disponibilidades = await _context.Disponibilidades
                .AsNoTracking()
                .Select(d => new
                {
                    d.DisponibilidadId,
                    d.ViajeId,
                    Viaje = d.Viaje == null ? null : new
                    {
                        d.Viaje.ViajeId,
                        d.Viaje.Titulo,
                        d.Viaje.Precio,
                        d.Viaje.CuposTotales,
                        d.Viaje.Activo
                    },
                    d.Fecha,
                    d.FechaRetorno,
                    d.CuposTotales,
                    d.CuposDisponibles,
                    CuposOcupados = d.CuposTotales - d.CuposDisponibles,
                    d.Activo
                })
                .OrderBy(d => d.Fecha)
                .ToListAsync();

            return Ok(disponibilidades);
        }

        // GET: api/Disponibilidades/5
        [HttpGet("{disponibilidadId:int}")]
        public async Task<IActionResult> GetDisponibilidad(int disponibilidadId)
        {
            var disponibilidad = await _context.Disponibilidades
                .AsNoTracking()
                .Where(d => d.DisponibilidadId == disponibilidadId)
                .Select(d => new
                {
                    d.DisponibilidadId,
                    d.ViajeId,
                    Viaje = d.Viaje == null ? null : new
                    {
                        d.Viaje.ViajeId,
                        d.Viaje.Titulo,
                        d.Viaje.Descripcion,
                        d.Viaje.Precio,
                        d.Viaje.CuposTotales,
                        d.Viaje.Activo
                    },
                    d.Fecha,
                    d.FechaRetorno,
                    d.CuposTotales,
                    d.CuposDisponibles,
                    CuposOcupados = d.CuposTotales - d.CuposDisponibles,
                    d.Activo,
                    Reservas = d.Reservas.Select(r => new
                    {
                        r.ReservaId,
                        r.UsuarioId,
                        Usuario = r.Usuario == null ? null : r.Usuario.Nombre,
                        r.CantidadPersonas,
                        r.Total,
                        Estado = r.EstadoReserva == null ? null : r.EstadoReserva.Nombre,
                        r.FechaReserva
                    })
                })
                .FirstOrDefaultAsync();

            if (disponibilidad == null)
            {
                return NotFound("La disponibilidad no existe.");
            }

            return Ok(disponibilidad);
        }

        // GET: api/Disponibilidades/viaje/5
        [HttpGet("viaje/{viajeId:int}")]
        public async Task<IActionResult> GetDisponibilidadesPorViaje(int viajeId)
        {
            var viajeExiste = await _context.Viajes
                .AnyAsync(v => v.ViajeId == viajeId);

            if (!viajeExiste)
            {
                return NotFound("El viaje no existe.");
            }

            var disponibilidades = await _context.Disponibilidades
                .AsNoTracking()
                .Where(d => d.ViajeId == viajeId)
                .Select(d => new
                {
                    d.DisponibilidadId,
                    d.ViajeId,
                    d.Fecha,
                    d.FechaRetorno,
                    d.CuposTotales,
                    d.CuposDisponibles,
                    CuposOcupados = d.CuposTotales - d.CuposDisponibles,
                    d.Activo
                })
                .OrderBy(d => d.Fecha)
                .ToListAsync();

            return Ok(disponibilidades);
        }

        // POST: api/Disponibilidades
        [HttpPost]
        public async Task<IActionResult> CrearDisponibilidad([FromBody] CrearDisponibilidadDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var fechaSalida = dto.Fecha.Date;
            var fechaRetorno = dto.FechaRetorno?.Date;

            if (fechaSalida < DateTime.Today)
            {
                return BadRequest("La fecha de salida no puede ser menor que la fecha actual.");
            }

            if (fechaRetorno.HasValue && fechaRetorno.Value < fechaSalida)
            {
                return BadRequest("La fecha de retorno no puede ser menor que la fecha de salida.");
            }

            var viaje = await _context.Viajes
                .FirstOrDefaultAsync(v => v.ViajeId == dto.ViajeId);

            if (viaje == null)
            {
                return NotFound("El viaje no existe.");
            }

            if (!viaje.Activo)
            {
                return BadRequest("No se puede crear disponibilidad para un viaje inactivo.");
            }

            if (dto.CuposTotales > viaje.CuposTotales)
            {
                return BadRequest($"Los cupos de la disponibilidad no pueden superar los cupos totales del viaje. Cupos del viaje: {viaje.CuposTotales}.");
            }

            var existeDisponibilidad = await _context.Disponibilidades
                .AnyAsync(d => d.ViajeId == dto.ViajeId && d.Fecha == fechaSalida);

            if (existeDisponibilidad)
            {
                return BadRequest("Ya existe una disponibilidad para este viaje en esa fecha.");
            }

            var disponibilidad = new Disponibilidad
            {
                ViajeId = dto.ViajeId,
                Fecha = fechaSalida,
                FechaRetorno = fechaRetorno,
                CuposTotales = dto.CuposTotales,
                CuposDisponibles = dto.CuposTotales,
                Activo = true
            };

            _context.Disponibilidades.Add(disponibilidad);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetDisponibilidad),
                new { disponibilidadId = disponibilidad.DisponibilidadId },
                new
                {
                    disponibilidad.DisponibilidadId,
                    disponibilidad.ViajeId,
                    disponibilidad.Fecha,
                    disponibilidad.FechaRetorno,
                    disponibilidad.CuposTotales,
                    disponibilidad.CuposDisponibles,
                    disponibilidad.Activo
                }
            );
        }

        // PUT: api/Disponibilidades/5
        [HttpPut("{disponibilidadId:int}")]
        public async Task<IActionResult> ActualizarDisponibilidad(int disponibilidadId, [FromBody] ActualizarDisponibilidadDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var disponibilidad = await _context.Disponibilidades
                .FirstOrDefaultAsync(d => d.DisponibilidadId == disponibilidadId);

            if (disponibilidad == null)
            {
                return NotFound("La disponibilidad no existe.");
            }

            var fechaSalida = dto.Fecha.Date;
            var fechaRetorno = dto.FechaRetorno?.Date;

            if (fechaRetorno.HasValue && fechaRetorno.Value < fechaSalida)
            {
                return BadRequest("La fecha de retorno no puede ser menor que la fecha de salida.");
            }

            var viaje = await _context.Viajes
                .FirstOrDefaultAsync(v => v.ViajeId == dto.ViajeId);

            if (viaje == null)
            {
                return NotFound("El viaje no existe.");
            }

            if (!viaje.Activo)
            {
                return BadRequest("No se puede asignar disponibilidad a un viaje inactivo.");
            }

            if (dto.CuposTotales > viaje.CuposTotales)
            {
                return BadRequest($"Los cupos de la disponibilidad no pueden superar los cupos totales del viaje. Cupos del viaje: {viaje.CuposTotales}.");
            }

            var existeOtraDisponibilidad = await _context.Disponibilidades
                .AnyAsync(d =>
                    d.DisponibilidadId != disponibilidadId &&
                    d.ViajeId == dto.ViajeId &&
                    d.Fecha == fechaSalida);

            if (existeOtraDisponibilidad)
            {
                return BadRequest("Ya existe otra disponibilidad para este viaje en esa fecha.");
            }

            var cuposOcupados = disponibilidad.CuposTotales - disponibilidad.CuposDisponibles;

            if (dto.CuposTotales < cuposOcupados)
            {
                return BadRequest($"No podés reducir los cupos totales por debajo de los cupos ya ocupados. Cupos ocupados: {cuposOcupados}.");
            }

            disponibilidad.ViajeId = dto.ViajeId;
            disponibilidad.Fecha = fechaSalida;
            disponibilidad.FechaRetorno = fechaRetorno;
            disponibilidad.CuposTotales = dto.CuposTotales;
            disponibilidad.CuposDisponibles = dto.CuposTotales - cuposOcupados;
            disponibilidad.Activo = dto.Activo;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                disponibilidad.DisponibilidadId,
                disponibilidad.ViajeId,
                disponibilidad.Fecha,
                disponibilidad.FechaRetorno,
                disponibilidad.CuposTotales,
                disponibilidad.CuposDisponibles,
                CuposOcupados = disponibilidad.CuposTotales - disponibilidad.CuposDisponibles,
                disponibilidad.Activo
            });
        }

        // PUT: api/Disponibilidades/5/activar
        [HttpPut("{disponibilidadId:int}/activar")]
        public async Task<IActionResult> ActivarDisponibilidad(int disponibilidadId)
        {
            var disponibilidad = await _context.Disponibilidades
                .FirstOrDefaultAsync(d => d.DisponibilidadId == disponibilidadId);

            if (disponibilidad == null)
            {
                return NotFound("La disponibilidad no existe.");
            }

            disponibilidad.Activo = true;
            await _context.SaveChangesAsync();

            return Ok("Disponibilidad activada correctamente.");
        }

        // PUT: api/Disponibilidades/5/desactivar
        [HttpPut("{disponibilidadId:int}/desactivar")]
        public async Task<IActionResult> DesactivarDisponibilidad(int disponibilidadId)
        {
            var disponibilidad = await _context.Disponibilidades
                .FirstOrDefaultAsync(d => d.DisponibilidadId == disponibilidadId);

            if (disponibilidad == null)
            {
                return NotFound("La disponibilidad no existe.");
            }

            disponibilidad.Activo = false;
            await _context.SaveChangesAsync();

            return Ok("Disponibilidad desactivada correctamente.");
        }

        // DELETE: api/Disponibilidades/5
        [HttpDelete("{disponibilidadId:int}")]
        public async Task<IActionResult> DeleteDisponibilidad(int disponibilidadId)
        {
            var disponibilidad = await _context.Disponibilidades
                .FirstOrDefaultAsync(d => d.DisponibilidadId == disponibilidadId);

            if (disponibilidad == null)
            {
                return NotFound("La disponibilidad no existe.");
            }

            var tieneReservas = await _context.Reservas
                .AnyAsync(r => r.DisponibilidadId == disponibilidadId);

            if (tieneReservas)
            {
                return BadRequest("No se puede eliminar una disponibilidad con reservas asociadas. Usá desactivar.");
            }

            _context.Disponibilidades.Remove(disponibilidad);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}