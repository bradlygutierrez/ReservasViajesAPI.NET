using AppReservasAPI.Context;
using AppReservasAPI.DTOs.MetodosPago;
using AppReservasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppReservasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MetodosPagoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MetodosPagoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/MetodosPago
        [HttpGet]
        public async Task<IActionResult> GetMetodosPago(
            [FromQuery] bool? activo,
            [FromQuery] string? busqueda)
        {
            var query = _context.MetodosPago
                .AsNoTracking()
                .AsQueryable();

            if (activo.HasValue)
            {
                query = query.Where(m => m.Activo == activo.Value);
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var texto = busqueda.Trim();

                query = query.Where(m =>
                    m.Nombre.Contains(texto) ||
                    (m.Descripcion != null && m.Descripcion.Contains(texto)));
            }

            var metodosPago = await query
                .Select(m => new
                {
                    m.MetodoPagoId,
                    m.Nombre,
                    m.Descripcion,
                    m.Activo,
                    m.FechaCreacion,
                    PagosAsociados = m.Pagos.Count()
                })
                .OrderBy(m => m.Nombre)
                .ToListAsync();

            return Ok(metodosPago);
        }

        // GET: api/MetodosPago/5
        [HttpGet("{metodoPagoId:int}")]
        public async Task<IActionResult> GetMetodoPago(int metodoPagoId)
        {
            var metodoPago = await _context.MetodosPago
                .AsNoTracking()
                .Where(m => m.MetodoPagoId == metodoPagoId)
                .Select(m => new
                {
                    m.MetodoPagoId,
                    m.Nombre,
                    m.Descripcion,
                    m.Activo,
                    m.FechaCreacion,
                    Pagos = m.Pagos.Select(p => new
                    {
                        p.PagoId,
                        p.ReservaId,
                        p.Monto,
                        p.Referencia,
                        p.FechaPago,
                        EstadoPago = p.EstadoPago == null ? null : p.EstadoPago.Nombre
                    })
                })
                .FirstOrDefaultAsync();

            if (metodoPago == null)
            {
                return NotFound("El método de pago no existe.");
            }

            return Ok(metodoPago);
        }

        // POST: api/MetodosPago
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CrearMetodoPago([FromBody] CrearMetodoPagoDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var nombre = dto.Nombre.Trim();

            var nombreDuplicado = await _context.MetodosPago
                .AnyAsync(m => m.Nombre == nombre);

            if (nombreDuplicado)
            {
                return BadRequest("Ya existe un método de pago con ese nombre.");
            }

            var metodoPago = new MetodoPago
            {
                Nombre = nombre,
                Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim(),
                Activo = true,
                FechaCreacion = DateTime.Now
            };

            _context.MetodosPago.Add(metodoPago);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetMetodoPago),
                new { metodoPagoId = metodoPago.MetodoPagoId },
                new
                {
                    metodoPago.MetodoPagoId,
                    metodoPago.Nombre,
                    metodoPago.Descripcion,
                    metodoPago.Activo,
                    metodoPago.FechaCreacion
                }
            );
        }

        // PUT: api/MetodosPago/5
        [HttpPut("{metodoPagoId:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ActualizarMetodoPago(int metodoPagoId, [FromBody] ActualizarMetodoPagoDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var metodoPago = await _context.MetodosPago
                .FirstOrDefaultAsync(m => m.MetodoPagoId == metodoPagoId);

            if (metodoPago == null)
            {
                return NotFound("El método de pago no existe.");
            }

            var nombre = dto.Nombre.Trim();

            var nombreDuplicado = await _context.MetodosPago
                .AnyAsync(m => m.MetodoPagoId != metodoPagoId && m.Nombre == nombre);

            if (nombreDuplicado)
            {
                return BadRequest("Ya existe otro método de pago con ese nombre.");
            }

            metodoPago.Nombre = nombre;
            metodoPago.Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim();
            metodoPago.Activo = dto.Activo;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                metodoPago.MetodoPagoId,
                metodoPago.Nombre,
                metodoPago.Descripcion,
                metodoPago.Activo,
                metodoPago.FechaCreacion
            });
        }

        // PUT: api/MetodosPago/5/activar
        [HttpPut("{metodoPagoId:int}/activar")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ActivarMetodoPago(int metodoPagoId)
        {
            var metodoPago = await _context.MetodosPago
                .FirstOrDefaultAsync(m => m.MetodoPagoId == metodoPagoId);

            if (metodoPago == null)
            {
                return NotFound("El método de pago no existe.");
            }

            metodoPago.Activo = true;
            await _context.SaveChangesAsync();

            return Ok("Método de pago activado correctamente.");
        }

        // PUT: api/MetodosPago/5/desactivar
        [HttpPut("{metodoPagoId:int}/desactivar")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DesactivarMetodoPago(int metodoPagoId)
        {
            var metodoPago = await _context.MetodosPago
                .FirstOrDefaultAsync(m => m.MetodoPagoId == metodoPagoId);

            if (metodoPago == null)
            {
                return NotFound("El método de pago no existe.");
            }

            metodoPago.Activo = false;
            await _context.SaveChangesAsync();

            return Ok("Método de pago desactivado correctamente.");
        }

        // DELETE: api/MetodosPago/5
        [HttpDelete("{metodoPagoId:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteMetodoPago(int metodoPagoId)
        {
            var metodoPago = await _context.MetodosPago
                .FirstOrDefaultAsync(m => m.MetodoPagoId == metodoPagoId);

            if (metodoPago == null)
            {
                return NotFound("El método de pago no existe.");
            }

            var tienePagos = await _context.Pagos
                .AnyAsync(p => p.MetodoPagoId == metodoPagoId);

            if (tienePagos)
            {
                metodoPago.Activo = false;
                await _context.SaveChangesAsync();

                return Ok("El método de pago tiene pagos asociados. No se eliminó físicamente; fue desactivado.");
            }

            _context.MetodosPago.Remove(metodoPago);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}