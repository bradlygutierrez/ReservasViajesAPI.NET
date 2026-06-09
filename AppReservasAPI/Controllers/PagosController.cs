using AppReservasAPI.Context;
using AppReservasAPI.DTOs.Pagos;
using AppReservasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppReservasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PagosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PagosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Pagos
        [HttpGet]
        public async Task<IActionResult> GetPagos()
        {
            var pagos = await _context.Pagos
                .AsNoTracking()
                .Select(p => new
                {
                    p.PagoId,
                    p.ReservaId,
                    Reserva = p.Reserva == null ? null : new
                    {
                        p.Reserva.ReservaId,
                        p.Reserva.UsuarioId,
                        Usuario = p.Reserva.Usuario == null ? null : p.Reserva.Usuario.Nombre,
                        p.Reserva.Total,
                        EstadoReserva = p.Reserva.EstadoReserva == null ? null : p.Reserva.EstadoReserva.Nombre
                    },
                    p.Monto,
                    p.Referencia,
                    p.FechaPago,
                    p.EstadoPagoId,
                    EstadoPago = p.EstadoPago == null ? null : p.EstadoPago.Nombre,
                    p.MetodoPagoId,
                    MetodoPago = p.MetodoPago == null ? null : p.MetodoPago.Nombre
                })
                .ToListAsync();

            return Ok(pagos);
        }

        // GET: api/Pagos/5
        [HttpGet("{pagoId:int}")]
        public async Task<IActionResult> GetPago(int pagoId)
        {
            var pago = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.PagoId == pagoId)
                .Select(p => new
                {
                    p.PagoId,
                    p.ReservaId,
                    Reserva = p.Reserva == null ? null : new
                    {
                        p.Reserva.ReservaId,
                        p.Reserva.UsuarioId,
                        Usuario = p.Reserva.Usuario == null ? null : new
                        {
                            p.Reserva.Usuario.UsuarioId,
                            p.Reserva.Usuario.Nombre,
                            p.Reserva.Usuario.Email
                        },
                        p.Reserva.Total,
                        p.Reserva.EstadoReservaId,
                        EstadoReserva = p.Reserva.EstadoReserva == null ? null : p.Reserva.EstadoReserva.Nombre
                    },
                    p.Monto,
                    p.Referencia,
                    p.FechaPago,
                    p.EstadoPagoId,
                    EstadoPago = p.EstadoPago == null ? null : p.EstadoPago.Nombre,
                    p.MetodoPagoId,
                    MetodoPago = p.MetodoPago == null ? null : p.MetodoPago.Nombre
                })
                .FirstOrDefaultAsync();

            if (pago == null)
            {
                return NotFound("El pago no existe.");
            }

            return Ok(pago);
        }

        // GET: api/Pagos/reserva/5
        [HttpGet("reserva/{reservaId:int}")]
        public async Task<IActionResult> GetPagosPorReserva(int reservaId)
        {
            var reservaExiste = await _context.Reservas
                .AnyAsync(r => r.ReservaId == reservaId);

            if (!reservaExiste)
            {
                return NotFound("La reserva no existe.");
            }

            var pagos = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.ReservaId == reservaId)
                .Select(p => new
                {
                    p.PagoId,
                    p.Monto,
                    p.Referencia,
                    p.FechaPago,
                    p.EstadoPagoId,
                    EstadoPago = p.EstadoPago == null ? null : p.EstadoPago.Nombre,
                    p.MetodoPagoId,
                    MetodoPago = p.MetodoPago == null ? null : p.MetodoPago.Nombre
                })
                .ToListAsync();

            return Ok(pagos);
        }

        // POST: api/Pagos
        [HttpPost]
        public async Task<IActionResult> CrearPago([FromBody] CrearPagoDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var reserva = await _context.Reservas
                .Include(r => r.EstadoReserva)
                .FirstOrDefaultAsync(r => r.ReservaId == dto.ReservaId);

            if (reserva == null)
            {
                return NotFound("La reserva no existe.");
            }

            var metodoPago = await _context.MetodosPago
                .FirstOrDefaultAsync(mp => mp.MetodoPagoId == dto.MetodoPagoId && mp.Activo);

            if (metodoPago == null)
            {
                return BadRequest("El método de pago no existe o no está activo.");
            }

            var estadoPago = await _context.EstadosPago
                .FirstOrDefaultAsync(ep => ep.EstadoPagoId == dto.EstadoPagoId && ep.Activo);

            if (estadoPago == null)
            {
                return BadRequest("El estado de pago no existe o no está activo.");
            }

            if (dto.UsuarioId.HasValue)
            {
                var usuarioExiste = await _context.Usuarios
                    .AnyAsync(u => u.UsuarioId == dto.UsuarioId.Value && u.Activo);

                if (!usuarioExiste)
                {
                    return BadRequest("El usuario que registra el pago no existe o no está activo.");
                }
            }

            var estadoPagado = estadoPago.Nombre.Equals("Pagado", StringComparison.OrdinalIgnoreCase);

            if (estadoPagado && reserva.Total.HasValue)
            {
                var estadoPagadoId = await _context.EstadosPago
                    .Where(ep => ep.Nombre == "Pagado")
                    .Select(ep => ep.EstadoPagoId)
                    .FirstOrDefaultAsync();

                var totalPagadoAnterior = await _context.Pagos
                    .Where(p => p.ReservaId == dto.ReservaId && p.EstadoPagoId == estadoPagadoId)
                    .SumAsync(p => p.Monto);

                var nuevoTotalPagado = totalPagadoAnterior + dto.Monto;

                if (nuevoTotalPagado > reserva.Total.Value)
                {
                    return BadRequest($"El pago excede el total de la reserva. Total de reserva: {reserva.Total.Value}, ya pagado: {totalPagadoAnterior}.");
                }
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var pago = new Pago
                {
                    ReservaId = dto.ReservaId,
                    MetodoPagoId = dto.MetodoPagoId,
                    EstadoPagoId = dto.EstadoPagoId,
                    Monto = dto.Monto,
                    Referencia = dto.Referencia,
                    FechaPago = DateTime.Now
                };

                _context.Pagos.Add(pago);

                if (estadoPagado)
                {
                    await ConfirmarReservaSiEstaPagada(reserva, dto.Monto, dto.UsuarioId);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(
                    nameof(GetPago),
                    new { pagoId = pago.PagoId },
                    new
                    {
                        pago.PagoId,
                        pago.ReservaId,
                        pago.Monto,
                        pago.Referencia,
                        pago.FechaPago,
                        EstadoPago = estadoPago.Nombre,
                        MetodoPago = metodoPago.Nombre
                    }
                );
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // PUT: api/Pagos/5/estado
        [HttpPut("{pagoId:int}/estado")]
        public async Task<IActionResult> CambiarEstadoPago(int pagoId, [FromBody] CambiarEstadoPagoDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var pago = await _context.Pagos
                .Include(p => p.EstadoPago)
                .Include(p => p.Reserva)
                    .ThenInclude(r => r.EstadoReserva)
                .FirstOrDefaultAsync(p => p.PagoId == pagoId);

            if (pago == null)
            {
                return NotFound("El pago no existe.");
            }

            var estadoNuevo = await _context.EstadosPago
                .FirstOrDefaultAsync(ep => ep.EstadoPagoId == dto.EstadoPagoId && ep.Activo);

            if (estadoNuevo == null)
            {
                return BadRequest("El nuevo estado de pago no existe o no está activo.");
            }

            if (pago.EstadoPagoId == dto.EstadoPagoId)
            {
                return BadRequest("El pago ya tiene ese estado.");
            }

            if (dto.UsuarioId.HasValue)
            {
                var usuarioExiste = await _context.Usuarios
                    .AnyAsync(u => u.UsuarioId == dto.UsuarioId.Value && u.Activo);

                if (!usuarioExiste)
                {
                    return BadRequest("El usuario que realiza el cambio no existe o no está activo.");
                }
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var estadoAnterior = pago.EstadoPago?.Nombre ?? string.Empty;

                pago.EstadoPagoId = estadoNuevo.EstadoPagoId;

                if (estadoNuevo.Nombre.Equals("Pagado", StringComparison.OrdinalIgnoreCase) && pago.Reserva != null)
                {
                    await ConfirmarReservaSiEstaPagada(pago.Reserva, pago.Monto, dto.UsuarioId);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    pago.PagoId,
                    EstadoAnterior = estadoAnterior,
                    EstadoNuevo = estadoNuevo.Nombre
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // DELETE: api/Pagos/5
        [HttpDelete("{pagoId:int}")]
        public IActionResult DeletePago(int pagoId)
        {
            return BadRequest("No se recomienda eliminar pagos físicamente. Cambiá el estado del pago a Cancelado o Reembolsado.");
        }

        private async Task ConfirmarReservaSiEstaPagada(Reserva reserva, decimal montoNuevoPago, int? usuarioId)
        {
            if (!reserva.Total.HasValue)
            {
                return;
            }

            var estadoPagadoId = await _context.EstadosPago
                .Where(ep => ep.Nombre == "Pagado")
                .Select(ep => ep.EstadoPagoId)
                .FirstOrDefaultAsync();

            if (estadoPagadoId == 0)
            {
                return;
            }

            var totalPagadoAnterior = await _context.Pagos
                .Where(p => p.ReservaId == reserva.ReservaId && p.EstadoPagoId == estadoPagadoId)
                .SumAsync(p => p.Monto);

            var totalPagado = totalPagadoAnterior + montoNuevoPago;

            if (totalPagado < reserva.Total.Value)
            {
                return;
            }

            var estadoConfirmada = await _context.EstadosReserva
                .FirstOrDefaultAsync(er => er.Nombre == "Confirmada" && er.Activo);

            if (estadoConfirmada == null)
            {
                return;
            }

            if (reserva.EstadoReservaId == estadoConfirmada.EstadoReservaId)
            {
                return;
            }

            var estadoAnteriorId = reserva.EstadoReservaId;

            reserva.EstadoReservaId = estadoConfirmada.EstadoReservaId;
            reserva.FechaActualizacion = DateTime.Now;

            var historial = new HistorialEstadoReserva
            {
                ReservaId = reserva.ReservaId,
                UsuarioId = usuarioId,
                EstadoAnteriorId = estadoAnteriorId,
                EstadoNuevoId = estadoConfirmada.EstadoReservaId,
                Motivo = "Reserva confirmada automáticamente por pago completado.",
                FechaCambio = DateTime.Now
            };

            _context.HistorialEstadosReserva.Add(historial);
        }
    }
}