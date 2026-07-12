using AppReservasAPI.Context;
using AppReservasAPI.DTOs.Agentes;
using AppReservasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AppReservasAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AgentesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    private static readonly HashSet<string> ExtensionesPermitidas =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp",
            ".pdf"
        };

    public AgentesController(
        AppDbContext context,
        IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // GET: api/Agentes/certificacion/estado
    [HttpGet("certificacion/estado")]
    public async Task<IActionResult> ObtenerEstadoCertificacion()
    {
        var usuarioId = ObtenerUsuarioId();

        if (usuarioId == null)
        {
            return Unauthorized("Token inválido.");
        }

        var certificacion = await _context.AgenteCertificaciones
            .AsNoTracking()
            .Where(c =>
                c.UsuarioId == usuarioId.Value &&
                c.Activo)
            .Select(c => new
            {
                c.AgenteCertificacionId,
                c.NombreLegal,
                c.Cedula,
                c.NumeroLicencia,
                c.DocumentoCedulaUrl,
                c.DocumentoLicenciaUrl,
                c.Estado,
                c.MotivoRechazo,
                c.FechaSolicitud,
                c.FechaRevision
            })
            .FirstOrDefaultAsync();

        return Ok(new
        {
            TieneSolicitud = certificacion != null,
            Certificado = certificacion?.Estado == "Aprobada",
            Certificacion = certificacion
        });
    }

    // POST: api/Agentes/certificacion
    [HttpPost("certificacion")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(12_000_000)]
    public async Task<IActionResult> SolicitarCertificacion(
        [FromForm] SolicitarCertificacionDto dto)
    {
        var usuarioId = ObtenerUsuarioId();

        if (usuarioId == null)
        {
            return Unauthorized("Token inválido.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var solicitudExistente =
            await _context.AgenteCertificaciones
                .FirstOrDefaultAsync(c =>
                    c.UsuarioId == usuarioId.Value);

        if (solicitudExistente != null &&
            solicitudExistente.Estado == "Pendiente")
        {
            return BadRequest(
                "Ya tienes una solicitud de certificación pendiente.");
        }

        if (solicitudExistente != null &&
            solicitudExistente.Estado == "Aprobada")
        {
            return BadRequest(
                "Tu certificación ya fue aprobada.");
        }

        var cedula = dto.Cedula.Trim();
        var licencia = dto.NumeroLicencia.Trim();

        var documentoDuplicado =
            await _context.AgenteCertificaciones.AnyAsync(c =>
                c.UsuarioId != usuarioId.Value &&
                (c.Cedula == cedula ||
                 c.NumeroLicencia == licencia));

        if (documentoDuplicado)
        {
            return BadRequest(
                "La cédula o la licencia ya están registradas.");
        }

        string rutaCedula;
        string rutaLicencia;

        try
        {
            rutaCedula = await GuardarDocumentoAsync(
                dto.DocumentoCedula,
                usuarioId.Value,
                "cedula");

            rutaLicencia = await GuardarDocumentoAsync(
                dto.DocumentoLicencia,
                usuarioId.Value,
                "licencia");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }

        if (solicitudExistente == null)
        {
            solicitudExistente = new AgenteCertificacion
            {
                UsuarioId = usuarioId.Value
            };

            _context.AgenteCertificaciones.Add(
                solicitudExistente);
        }

        solicitudExistente.NombreLegal =
            dto.NombreLegal.Trim();

        solicitudExistente.Cedula = cedula;
        solicitudExistente.NumeroLicencia = licencia;
        solicitudExistente.DocumentoCedulaUrl = rutaCedula;
        solicitudExistente.DocumentoLicenciaUrl = rutaLicencia;
        solicitudExistente.Estado = "Pendiente";
        solicitudExistente.MotivoRechazo = null;
        solicitudExistente.FechaSolicitud = DateTime.Now;
        solicitudExistente.FechaRevision = null;
        solicitudExistente.RevisadoPorUsuarioId = null;
        solicitudExistente.Activo = true;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message =
                "Solicitud enviada correctamente.",
            solicitudExistente.AgenteCertificacionId,
            solicitudExistente.Estado
        });
    }

    // GET: api/Agentes/certificaciones
    [HttpGet("certificaciones")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ObtenerCertificaciones(
        [FromQuery] string? estado)
    {
        var query = _context.AgenteCertificaciones
            .AsNoTracking()
            .Where(c => c.Activo)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
        {
            query = query.Where(c =>
                c.Estado == estado.Trim());
        }

        var solicitudes = await query
            .OrderByDescending(c => c.FechaSolicitud)
            .Select(c => new
            {
                c.AgenteCertificacionId,
                c.UsuarioId,
                Usuario = c.Usuario == null
                    ? null
                    : new
                    {
                        c.Usuario.Nombre,
                        c.Usuario.Email
                    },
                c.NombreLegal,
                c.Cedula,
                c.NumeroLicencia,
                c.DocumentoCedulaUrl,
                c.DocumentoLicenciaUrl,
                c.Estado,
                c.MotivoRechazo,
                c.FechaSolicitud,
                c.FechaRevision
            })
            .ToListAsync();

        return Ok(solicitudes);
    }

    // PUT: api/Agentes/certificaciones/5/estado
    [HttpPut("certificaciones/{id:int}/estado")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RevisarCertificacion(
        int id,
        [FromBody] RevisarCertificacionDto dto)
    {
        var administradorId = ObtenerUsuarioId();

        if (administradorId == null)
        {
            return Unauthorized("Token inválido.");
        }

        var estadosPermitidos = new[]
        {
            "Aprobada",
            "Rechazada"
        };

        var estado = dto.Estado.Trim();

        if (!estadosPermitidos.Contains(
                estado,
                StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(
                "El estado debe ser Aprobada o Rechazada.");
        }

        if (estado.Equals(
                "Rechazada",
                StringComparison.OrdinalIgnoreCase) &&
            string.IsNullOrWhiteSpace(dto.MotivoRechazo))
        {
            return BadRequest(
                "Debe indicar el motivo del rechazo.");
        }

        var certificacion =
            await _context.AgenteCertificaciones
                .FirstOrDefaultAsync(c =>
                    c.AgenteCertificacionId == id &&
                    c.Activo);

        if (certificacion == null)
        {
            return NotFound(
                "La solicitud de certificación no existe.");
        }

        certificacion.Estado =
            estado.Equals(
                "Aprobada",
                StringComparison.OrdinalIgnoreCase)
                ? "Aprobada"
                : "Rechazada";

        certificacion.MotivoRechazo =
            certificacion.Estado == "Rechazada"
                ? dto.MotivoRechazo?.Trim()
                : null;

        certificacion.FechaRevision = DateTime.Now;
        certificacion.RevisadoPorUsuarioId =
            administradorId.Value;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message =
                $"La certificación fue {certificacion.Estado.ToLower()}.",
            certificacion.AgenteCertificacionId,
            certificacion.Estado,
            certificacion.MotivoRechazo
        });
    }

    private int? ObtenerUsuarioId()
    {
        var valor = User.FindFirst(
            ClaimTypes.NameIdentifier)?.Value;

        return int.TryParse(valor, out var usuarioId)
            ? usuarioId
            : null;
    }

    private async Task<string> GuardarDocumentoAsync(
        IFormFile archivo,
        int usuarioId,
        string tipoDocumento)
    {
        if (archivo == null || archivo.Length == 0)
        {
            throw new InvalidOperationException(
                "Debe seleccionar un documento.");
        }

        const long tamanoMaximo = 5 * 1024 * 1024;

        if (archivo.Length > tamanoMaximo)
        {
            throw new InvalidOperationException(
                "Cada documento puede pesar como máximo 5 MB.");
        }

        var extension = Path
            .GetExtension(archivo.FileName)
            .ToLowerInvariant();

        if (!ExtensionesPermitidas.Contains(extension))
        {
            throw new InvalidOperationException(
                "Solo se permiten archivos JPG, JPEG, PNG, WEBP o PDF.");
        }

        var webRootPath = _env.WebRootPath;

        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(
                _env.ContentRootPath,
                "wwwroot");
        }

        var carpetaFisica = Path.Combine(
            webRootPath,
            "uploads",
            "certificaciones",
            usuarioId.ToString());

        Directory.CreateDirectory(carpetaFisica);

        var nombreArchivo =
            $"{tipoDocumento}_{Guid.NewGuid():N}{extension}";

        var rutaFisica = Path.Combine(
            carpetaFisica,
            nombreArchivo);

        await using var stream = new FileStream(
            rutaFisica,
            FileMode.Create);

        await archivo.CopyToAsync(stream);

        return
            $"/uploads/certificaciones/{usuarioId}/{nombreArchivo}";
    }
}