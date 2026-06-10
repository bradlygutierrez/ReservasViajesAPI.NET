using AppReservasAPI.Context;
using AppReservasAPI.DTOs.EstadosPago;
using AppReservasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppReservasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EstadosPagoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EstadosPagoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/EstadosPago
        [HttpGet]
        public async Task<IActionResult> GetEstadosPago(
            [FromQuery] bool? activo,
            [FromQuery] string? busqueda)
        {
            var query = _context.EstadosPago
                .AsNoTracking()
                .AsQueryable();

            if (activo.HasValue)
            {
                query = query.Where(e => e.Activo == activo.Value);
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var texto = busqueda.Trim();

                query = query.Where(e =>
                    e.Nombre.Contains(texto) ||
                    (e.Descripcion != null && e.Descripcion.Contains(texto)));
            }

            var estados = await query
                .Select(e => new
                {
                    e.EstadoPagoId,
                    e.Nombre,
                    e.Descripcion,
                    e.Activo,
                    e.FechaCreacion,
                    PagosAsociados = e.Pagos.Count()
                })
                .OrderBy(e => e.Nombre)
                .ToListAsync();

            return Ok(estados);
        }

        // GET: api/EstadosPago/5
        [HttpGet("{estadoPagoId:int}")]
        public async Task<IActionResult> GetEstadoPago(int estadoPagoId)
        {
            var estado = await _context.EstadosPago
                .AsNoTracking()
                .Where(e => e.EstadoPagoId == estadoPagoId)
                .Select(e => new
                {
                    e.EstadoPagoId,
                    e.Nombre,
                    e.Descripcion,
                    e.Activo,
                    e.FechaCreacion,
                    Pagos = e.Pagos.Select(p => new
                    {
                        p.PagoId,
                        p.ReservaId,
                        p.Monto,
                        p.Referencia,
                        p.FechaPago,
                        MetodoPago = p.MetodoPago == null ? null : p.MetodoPago.Nombre
                    })
                })
                .FirstOrDefaultAsync();

            if (estado == null)
            {
                return NotFound("El estado de pago no existe.");
            }

            return Ok(estado);
        }

        // POST: api/EstadosPago
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CrearEstadoPago([FromBody] CrearEstadoPagoDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var nombre = dto.Nombre.Trim();

            var nombreDuplicado = await _context.EstadosPago
                .AnyAsync(e => e.Nombre == nombre);

            if (nombreDuplicado)
            {
                return BadRequest("Ya existe un estado de pago con ese nombre.");
            }

            var estado = new EstadoPago
            {
                Nombre = nombre,
                Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim(),
                Activo = true,
                FechaCreacion = DateTime.Now
            };

            _context.EstadosPago.Add(estado);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEstadoPago),
                new { estadoPagoId = estado.EstadoPagoId },
                new
                {
                    estado.EstadoPagoId,
                    estado.Nombre,
                    estado.Descripcion,
                    estado.Activo,
                    estado.FechaCreacion
                }
            );
        }

        // PUT: api/EstadosPago/5
        [HttpPut("{estadoPagoId:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ActualizarEstadoPago(int estadoPagoId, [FromBody] ActualizarEstadoPagoDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var estado = await _context.EstadosPago
                .FirstOrDefaultAsync(e => e.EstadoPagoId == estadoPagoId);

            if (estado == null)
            {
                return NotFound("El estado de pago no existe.");
            }

            var nombre = dto.Nombre.Trim();

            var nombreDuplicado = await _context.EstadosPago
                .AnyAsync(e => e.EstadoPagoId != estadoPagoId && e.Nombre == nombre);

            if (nombreDuplicado)
            {
                return BadRequest("Ya existe otro estado de pago con ese nombre.");
            }

            estado.Nombre = nombre;
            estado.Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim();
            estado.Activo = dto.Activo;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                estado.EstadoPagoId,
                estado.Nombre,
                estado.Descripcion,
                estado.Activo,
                estado.FechaCreacion
            });
        }

        // PUT: api/EstadosPago/5/activar
        [HttpPut("{estadoPagoId:int}/activar")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ActivarEstadoPago(int estadoPagoId)
        {
            var estado = await _context.EstadosPago
                .FirstOrDefaultAsync(e => e.EstadoPagoId == estadoPagoId);

            if (estado == null)
            {
                return NotFound("El estado de pago no existe.");
            }

            estado.Activo = true;
            await _context.SaveChangesAsync();

            return Ok("Estado de pago activado correctamente.");
        }

        // PUT: api/EstadosPago/5/desactivar
        [HttpPut("{estadoPagoId:int}/desactivar")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DesactivarEstadoPago(int estadoPagoId)
        {
            var estado = await _context.EstadosPago
                .FirstOrDefaultAsync(e => e.EstadoPagoId == estadoPagoId);

            if (estado == null)
            {
                return NotFound("El estado de pago no existe.");
            }

            estado.Activo = false;
            await _context.SaveChangesAsync();

            return Ok("Estado de pago desactivado correctamente.");
        }

        // DELETE: api/EstadosPago/5
        [HttpDelete("{estadoPagoId:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteEstadoPago(int estadoPagoId)
        {
            var estado = await _context.EstadosPago
                .FirstOrDefaultAsync(e => e.EstadoPagoId == estadoPagoId);

            if (estado == null)
            {
                return NotFound("El estado de pago no existe.");
            }

            var tienePagos = await _context.Pagos
                .AnyAsync(p => p.EstadoPagoId == estadoPagoId);

            if (tienePagos)
            {
                estado.Activo = false;
                await _context.SaveChangesAsync();

                return Ok("El estado de pago tiene pagos asociados. No se eliminó físicamente; fue desactivado.");
            }

            _context.EstadosPago.Remove(estado);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}