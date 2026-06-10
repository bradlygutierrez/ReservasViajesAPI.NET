using System.Security.Cryptography;
using System.Text;

namespace AppReservasAPI.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? string.Empty;

            // Permitir Swagger sin API Key para poder abrir la documentación.
            // Los endpoints /api sí pedirán API Key.
            if (path.StartsWith("/swagger"))
            {
                await _next(context);
                return;
            }

            var headerName = _configuration["ApiKey:HeaderName"] ?? "X-API-KEY";
            var configuredApiKey = _configuration["ApiKey:Value"];

            if (string.IsNullOrWhiteSpace(configuredApiKey))
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("API Key no configurada en el servidor.");
                return;
            }

            if (!context.Request.Headers.TryGetValue(headerName, out var extractedApiKey))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("API Key requerida.");
                return;
            }

            if (!ApiKeysMatch(configuredApiKey, extractedApiKey.ToString()))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("API Key inválida.");
                return;
            }

            await _next(context);
        }

        private static bool ApiKeysMatch(string configuredApiKey, string providedApiKey)
        {
            var configuredBytes = Encoding.UTF8.GetBytes(configuredApiKey);
            var providedBytes = Encoding.UTF8.GetBytes(providedApiKey);

            if (configuredBytes.Length != providedBytes.Length)
            {
                return false;
            }

            return CryptographicOperations.FixedTimeEquals(configuredBytes, providedBytes);
        }
    }
}