using AppReservasAPI.Context;
using AppReservasAPI.DTOs.Publicaciones;
using AppReservasAPI.DTOs.Viajes;
using System.Text.Json;
using AppReservasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AppReservasAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PublicacionesViajesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    private static readonly HashSet<string> ExtensionesPermitidas = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    public PublicacionesViajesController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // GET: api/PublicacionesViajes/catalogos
    [HttpGet("catalogos")]
    public async Task<IActionResult> GetCatalogos()
    {
        var tipos = await _context.TiposViaje
            .AsNoTracking()
            .Where(t => t.Activo)
            .OrderBy(t => t.Nombre)
            .Select(t => new
            {
                t.TipoViajeId,
                t.Nombre,
                t.Descripcion
            })
            .ToListAsync();

        var paises = await _context.Paises
            .AsNoTracking()
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .Select(p => new
            {
                p.PaisId,
                p.Nombre
            })
            .ToListAsync();

        return Ok(new { tipos, paises });
    }

    // GET: api/PublicacionesViajes/mis-viajes
    [HttpGet("mis-viajes")]
    public async Task<IActionResult> GetMisViajes()
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId == null)
        {
            return Unauthorized("Token inválido.");
        }

        var viajes = await QueryViajesBase()
            .Where(v => v.PublicadoPorUsuarioId == usuarioId.Value)
            .OrderByDescending(v => v.FechaCreacion)
            .ToListAsync();

        return Ok(viajes.Select(ProyectarViaje));
    }

    // GET: api/PublicacionesViajes/admin
    [HttpGet("admin")]
    public async Task<IActionResult> GetAdmin()
    {
        if (!EsAdministrador())
        {
            return Forbid();
        }

        var viajes = await QueryViajesBase()
            .OrderByDescending(v => v.FechaCreacion)
            .ToListAsync();

        return Ok(viajes.Select(ProyectarViaje));
    }

    // POST: api/PublicacionesViajes
    [HttpPost]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> CrearPublicacion(
        [FromForm] CrearPublicacionViajeDto dto)
    {
        var usuarioId = ObtenerUsuarioId();

        if (usuarioId == null)
        {
            return Unauthorized("Token inválido.");
        }

        // El administrador puede publicar sin certificación.
        var esAdmin = User.IsInRole("Admin");

        if (!esAdmin)
        {
            var agenteCertificado = await _context
                .AgenteCertificaciones
                .AnyAsync(c =>
                    c.UsuarioId == usuarioId.Value &&
                    c.Estado == "Aprobada" &&
                    c.Activo);

            if (!agenteCertificado)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message =
                            "Debes ser un agente certificado para publicar viajes.",
                        codigo = "AGENTE_NO_CERTIFICADO"
                    });
            }
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var fechaSalida = dto.FechaSalida.Date;
        var fechaRetorno = dto.FechaRetorno?.Date;

        if (fechaSalida < DateTime.Today)
        {
            return BadRequest(
                "No se puede publicar un viaje con fecha de salida pasada.");
        }

        if (fechaRetorno.HasValue &&
            fechaRetorno.Value < fechaSalida)
        {
            return BadRequest(
                "La fecha de retorno no puede ser menor que la fecha de salida.");
        }

        var tipoExiste = await _context.TiposViaje
            .AnyAsync(t =>
                t.TipoViajeId == dto.TipoViajeId &&
                t.Activo);

        if (!tipoExiste)
        {
            return BadRequest(
                "El tipo de viaje no existe o no está activo.");
        }

        // Convertir las inclusiones y el itinerario recibidos como JSON.
        List<ViajeInclusionDto> inclusiones;
        List<ViajeItinerarioDto> itinerario;

        var opcionesJson = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        try
        {
            inclusiones = DeserializarListaFlexible<ViajeInclusionDto>(
                dto.InclusionesJson,
                opcionesJson);

            itinerario = DeserializarListaFlexible<ViajeItinerarioDto>(
                dto.ItinerarioJson,
                opcionesJson);
        }
        catch (JsonException)
        {
            return BadRequest(new
            {
                message =
                    "El formato de inclusiones o itinerario no es válido.",
                codigo = "JSON_VIAJE_INVALIDO"
            });
        }

        // Validar inclusiones.
        if (inclusiones.Any(i =>
                string.IsNullOrWhiteSpace(i.Tipo) ||
                string.IsNullOrWhiteSpace(i.Titulo)))
        {
            return BadRequest(new
            {
                message =
                    "Cada inclusión debe contener un tipo y un título.",
                codigo = "INCLUSION_INVALIDA"
            });
        }

        if (inclusiones.Any(i => i.Orden < 0))
        {
            return BadRequest(new
            {
                message =
                    "El orden de las inclusiones no puede ser negativo.",
                codigo = "ORDEN_INCLUSION_INVALIDO"
            });
        }

        // Validar itinerario.
        if (itinerario.Any(i =>
                i.Dia <= 0 ||
                string.IsNullOrWhiteSpace(i.Titulo)))
        {
            return BadRequest(new
            {
                message =
                    "Cada elemento del itinerario debe tener un día válido y un título.",
                codigo = "ITINERARIO_INVALIDO"
            });
        }

        if (itinerario.Any(i => i.Orden < 0))
        {
            return BadRequest(new
            {
                message =
                    "El orden del itinerario no puede ser negativo.",
                codigo = "ORDEN_ITINERARIO_INVALIDO"
            });
        }

        var diasDuplicados = itinerario
            .GroupBy(i => i.Dia)
            .Any(g => g.Count() > 1);

        if (diasDuplicados)
        {
            return BadRequest(new
            {
                message =
                    "No se puede registrar más de un itinerario con el mismo número de día.",
                codigo = "DIA_ITINERARIO_DUPLICADO"
            });
        }

        string? imagenUrl;

        try
        {
            imagenUrl = await GuardarImagenAsync(dto.Imagen);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }

        var paisNombre = Normalizar(dto.Pais);
        var ciudadNombre = Normalizar(dto.Ciudad);

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var pais = await _context.Paises
                .FirstOrDefaultAsync(p =>
                    p.Nombre == paisNombre);

            if (pais == null)
            {
                pais = new Pais
                {
                    Nombre = paisNombre,
                    Activo = true
                };

                _context.Paises.Add(pais);
                await _context.SaveChangesAsync();
            }
            else if (!pais.Activo)
            {
                pais.Activo = true;
            }

            var ciudad = await _context.Ciudades
                .FirstOrDefaultAsync(c =>
                    c.PaisId == pais.PaisId &&
                    c.Nombre == ciudadNombre);

            if (ciudad == null)
            {
                ciudad = new Ciudad
                {
                    PaisId = pais.PaisId,
                    Nombre = ciudadNombre,
                    Activo = true
                };

                _context.Ciudades.Add(ciudad);
                await _context.SaveChangesAsync();
            }
            else if (!ciudad.Activo)
            {
                ciudad.Activo = true;
            }

            var destino = new Destino
            {
                CiudadId = ciudad.CiudadId,
                Descripcion =
                    string.IsNullOrWhiteSpace(
                        dto.DestinoDescripcion)
                        ? dto.Descripcion?.Trim()
                        : dto.DestinoDescripcion.Trim(),
                Activo = true
            };

            _context.Destinos.Add(destino);
            await _context.SaveChangesAsync();

            var ahora = DateTime.Now;

            var viaje = new Viaje
            {
                TipoViajeId = dto.TipoViajeId,
                DestinoId = destino.DestinoId,
                Titulo = dto.Titulo.Trim(),
                Descripcion =
                    string.IsNullOrWhiteSpace(dto.Descripcion)
                        ? null
                        : dto.Descripcion.Trim(),
                Precio = dto.Precio,
                CuposTotales = dto.CuposTotales,
                Activo = true,
                EstadoPublicacion = "Publicado",
                ImagenUrl = imagenUrl,
                PublicadoPorUsuarioId = usuarioId.Value,
                FechaCreacion = ahora,
                FechaActualizacion = ahora
            };

            _context.Viajes.Add(viaje);
            await _context.SaveChangesAsync();

            // Guardar inclusiones relacionadas con el viaje.
            foreach (var item in inclusiones)
            {
                var inclusion = new ViajeInclusion
                {
                    ViajeId = viaje.ViajeId,
                    Tipo = item.Tipo.Trim(),
                    Titulo = item.Titulo.Trim(),
                    Detalle =
                        string.IsNullOrWhiteSpace(item.Detalle)
                            ? null
                            : item.Detalle.Trim(),
                    Orden = item.Orden,
                    Activo = true,
                    FechaCreacion = ahora
                };

                _context.ViajeInclusiones.Add(inclusion);
            }

            // Guardar itinerario relacionado con el viaje.
            foreach (var item in itinerario)
            {
                var detalleItinerario =
                    new ViajeItinerario
                    {
                        ViajeId = viaje.ViajeId,
                        Dia = item.Dia,
                        Titulo = item.Titulo.Trim(),
                        Descripcion =
                            string.IsNullOrWhiteSpace(
                                item.Descripcion)
                                ? null
                                : item.Descripcion.Trim(),
                        Sitios =
                            string.IsNullOrWhiteSpace(
                                item.Sitios)
                                ? null
                                : item.Sitios.Trim(),
                        Orden = item.Orden,
                        Activo = true,
                        FechaCreacion = ahora
                    };

                _context.ViajeItinerarios.Add(
                    detalleItinerario);
            }

            await _context.SaveChangesAsync();

            var disponibilidad = new Disponibilidad
            {
                ViajeId = viaje.ViajeId,
                Fecha = fechaSalida,
                FechaRetorno = fechaRetorno,
                CuposTotales = dto.CuposTotales,
                CuposDisponibles = dto.CuposTotales,
                Activo = true
            };

            _context.Disponibilidades.Add(disponibilidad);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            var creado = await QueryViajesBase()
                .FirstAsync(v =>
                    v.ViajeId == viaje.ViajeId);

            return CreatedAtAction(
                nameof(GetMisViajes),
                new
                {
                    viajeId = viaje.ViajeId
                },
                ProyectarViaje(creado));
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static List<T> DeserializarListaFlexible<T>(
    List<string>? valores,
    JsonSerializerOptions opciones)
    {
        var resultado = new List<T>();

        if (valores == null || valores.Count == 0)
        {
            return resultado;
        }

        foreach (var valor in valores)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                continue;
            }

            var texto = valor.Trim();

            // Permite un arreglo completo:
            // [{"tipo":"Hotel"}, {"tipo":"Transporte"}]
            if (texto.StartsWith("["))
            {
                var lista = JsonSerializer.Deserialize<List<T>>(
                    texto,
                    opciones);

                if (lista != null)
                {
                    resultado.AddRange(lista);
                }
            }
            else
            {
                // Permite objetos separados:
                // {"tipo":"Hotel"}
                var elemento = JsonSerializer.Deserialize<T>(
                    texto,
                    opciones);

                if (elemento != null)
                {
                    resultado.Add(elemento);
                }
            }
        }

        return resultado;
    }

    // PUT: api/PublicacionesViajes/5/estado
    [HttpPut("{viajeId:int}/estado")]
    public async Task<IActionResult> CambiarEstado(
        int viajeId,
        [FromBody] CambiarEstadoPublicacionDto dto)
    {
        var usuarioId = ObtenerUsuarioId();

        if (usuarioId == null)
        {
            return Unauthorized("Token inválido.");
        }

        if (!dto.Activo && string.IsNullOrWhiteSpace(dto.Motivo))
        {
            return BadRequest(new
            {
                message = "Debe indicar el motivo por el cual se pausa el viaje.",
                codigo = "MOTIVO_PAUSA_REQUERIDO"
            });
        }

        var viaje = await _context.Viajes
            .Include(v => v.Disponibilidades)
            .FirstOrDefaultAsync(v => v.ViajeId == viajeId);

        if (viaje == null)
        {
            return NotFound("El viaje no existe.");
        }

        if (!EsAdministrador() &&
            viaje.PublicadoPorUsuarioId != usuarioId.Value)
        {
            return Forbid();
        }

        if (viaje.Activo == dto.Activo)
        {
            return BadRequest(new
            {
                message = dto.Activo
                    ? "El viaje ya se encuentra publicado."
                    : "El viaje ya se encuentra pausado.",
                codigo = "ESTADO_VIAJE_SIN_CAMBIOS"
            });
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var ahora = DateTime.Now;

            /*
             * REACTIVAR VIAJE
             *
             * Reactiva el viaje y sus disponibilidades.
             * Las reservas pausadas no se reactivan automáticamente,
             * porque cada cliente debe elegir reagendar o solicitar
             * un reembolso.
             */
            if (dto.Activo)
            {
                viaje.Activo = true;
                viaje.EstadoPublicacion = "Publicado";
                viaje.MotivoPausa = null;
                viaje.FechaPausa = null;
                viaje.PausadoPorUsuarioId = null;
                viaje.FechaActualizacion = ahora;

                foreach (var disponibilidad in viaje.Disponibilidades)
                {
                    disponibilidad.Activo = true;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    viaje.ViajeId,
                    viaje.Activo,
                    viaje.EstadoPublicacion,
                    message = "El viaje fue reactivado correctamente."
                });
            }

            /*
             * PAUSAR VIAJE
             */
            var estadoPausada = await _context.EstadosReserva
                .FirstOrDefaultAsync(e =>
                    e.Nombre == "Pausada" &&
                    e.Activo);

            if (estadoPausada == null)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message =
                            "No se encontró el estado de reserva 'Pausada'.",
                        codigo = "ESTADO_PAUSADA_NO_CONFIGURADO"
                    });
            }

            viaje.Activo = false;
            viaje.EstadoPublicacion = "Pausado";
            viaje.MotivoPausa = dto.Motivo!.Trim();
            viaje.FechaPausa = ahora;
            viaje.PausadoPorUsuarioId = usuarioId.Value;
            viaje.FechaActualizacion = ahora;

            foreach (var disponibilidad in viaje.Disponibilidades)
            {
                disponibilidad.Activo = false;
            }

            /*
             * Buscar todas las reservas del viaje que todavía
             * pueden ser afectadas por la pausa.
             */
            var reservas = await _context.Reservas
                .Include(r => r.Disponibilidad)
                .Include(r => r.EstadoReserva)
                .Where(r =>
                    r.Disponibilidad != null &&
                    r.Disponibilidad.ViajeId == viajeId &&
                    r.EstadoReserva != null &&
                    r.EstadoReserva.Nombre != "Cancelada" &&
                    r.EstadoReserva.Nombre != "Reembolsada" &&
                    r.EstadoReserva.Nombre != "Reembolso solicitado" &&
                    r.EstadoReserva.Nombre != "Pausada")
                .ToListAsync();

            foreach (var reserva in reservas)
            {
                var estadoAnteriorId = reserva.EstadoReservaId;

                reserva.EstadoReservaId =
                    estadoPausada.EstadoReservaId;

                reserva.FechaActualizacion = ahora;

                /*
                 * Registrar el cambio de estado.
                 */
                _context.HistorialEstadosReserva.Add(
                    new HistorialEstadoReserva
                    {
                        ReservaId = reserva.ReservaId,
                        EstadoAnteriorId = estadoAnteriorId,
                        EstadoNuevoId =
                            estadoPausada.EstadoReservaId,
                        UsuarioId = usuarioId.Value,
                        Motivo = dto.Motivo.Trim(),
                        FechaCambio = ahora
                    });

                /*
                 * Crear notificación para el cliente.
                 */
                _context.Notificaciones.Add(
                    new Notificacion
                    {
                        UsuarioId = reserva.UsuarioId,
                        ViajeId = viaje.ViajeId,
                        ReservaId = reserva.ReservaId,
                        Titulo = "Tu viaje fue pausado",
                        Mensaje =
                            $"El viaje \"{viaje.Titulo}\" fue pausado. " +
                            $"Motivo: {dto.Motivo.Trim()} " +
                            "Puedes reagendar tu reserva o solicitar un reembolso.",
                        Tipo = "ViajePausado",
                        Leida = false,
                        FechaCreacion = ahora
                    });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                viaje.ViajeId,
                viaje.Activo,
                viaje.EstadoPublicacion,
                viaje.MotivoPausa,
                viaje.FechaPausa,
                ReservasAfectadas = reservas.Count,
                message = "El viaje y sus reservas fueron pausados correctamente."
            });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private IQueryable<Viaje> QueryViajesBase()
    {
        return _context.Viajes
            .AsNoTracking()
            .Include(v => v.TipoViaje)
            .Include(v => v.Destino)
                .ThenInclude(d => d!.Ciudad)
                    .ThenInclude(c => c!.Pais)
            .Include(v => v.Disponibilidades);
    }

    private object ProyectarViaje(Viaje v)
    {
        var primeraDisponibilidad = v.Disponibilidades
            .OrderBy(d => d.Fecha)
            .FirstOrDefault();

        return new
        {
            v.ViajeId,
            v.Titulo,
            v.Descripcion,
            v.Precio,
            v.CuposTotales,
            v.Activo,
            v.EstadoPublicacion,
            v.PublicadoPorUsuarioId,
            v.FechaCreacion,
            v.FechaActualizacion,
            ImagenUrl = ConstruirUrlPublica(v.ImagenUrl),
            TipoViaje = v.TipoViaje == null ? null : new
            {
                v.TipoViaje.TipoViajeId,
                v.TipoViaje.Nombre
            },
            Destino = v.Destino == null ? null : new
            {
                v.Destino.DestinoId,
                v.Destino.Descripcion,
                Ciudad = v.Destino.Ciudad == null ? null : new
                {
                    v.Destino.Ciudad.CiudadId,
                    v.Destino.Ciudad.Nombre,
                    Pais = v.Destino.Ciudad.Pais == null ? null : new
                    {
                        v.Destino.Ciudad.Pais.PaisId,
                        v.Destino.Ciudad.Pais.Nombre
                    }
                }
            },
            Disponibilidad = primeraDisponibilidad == null ? null : new
            {
                primeraDisponibilidad.DisponibilidadId,
                Fecha = primeraDisponibilidad.Fecha,
                primeraDisponibilidad.FechaRetorno,
                primeraDisponibilidad.CuposTotales,
                primeraDisponibilidad.CuposDisponibles,
                CuposOcupados = primeraDisponibilidad.CuposTotales - primeraDisponibilidad.CuposDisponibles,
                primeraDisponibilidad.Activo
            }
        };
    }

    private async Task<string?> GuardarImagenAsync(IFormFile? imagen)
    {
        if (imagen == null || imagen.Length == 0)
        {
            return null;
        }

        if (imagen.Length > 5_000_000)
        {
            throw new InvalidOperationException("La imagen no puede pesar más de 5 MB.");
        }

        var extension = Path.GetExtension(imagen.FileName);
        if (!ExtensionesPermitidas.Contains(extension))
        {
            throw new InvalidOperationException("Formato de imagen no permitido. Usá JPG, PNG o WEBP.");
        }

        var webRoot = _env.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }

        var uploadsPath = Path.Combine(webRoot, "uploads", "viajes");
        Directory.CreateDirectory(uploadsPath);

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var filePath = Path.Combine(uploadsPath, fileName);

        await using var stream = System.IO.File.Create(filePath);
        await imagen.CopyToAsync(stream);

        return $"/uploads/viajes/{fileName}";
    }

    private string? ConstruirUrlPublica(string? relativeUrl)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl))
        {
            return null;
        }

        if (relativeUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return relativeUrl;
        }

        return $"{Request.Scheme}://{Request.Host}{relativeUrl}";
    }

    private int? ObtenerUsuarioId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var usuarioId) ? usuarioId : null;
    }

    private bool EsAdministrador()
    {
        return User.IsInRole("Administrador") || User.IsInRole("Admin");
    }

    private static string Normalizar(string value)
    {
        return value.Trim();
    }
}
