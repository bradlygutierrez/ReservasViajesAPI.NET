using System.Net;

namespace AppReservasAPI.Middleware;

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
        if (context.Request.Method == HttpMethods.Options)
        {
            await _next(context);
            return;
        }

        var path = context.Request.Path.Value?.ToLower();

        if (path != null && (
            path.StartsWith("/swagger") ||
            path.StartsWith("/favicon") ||
            path.StartsWith("/health")
        ))
        {
            await _next(context);
            return;
        }

        var headerName = _configuration["ApiKey:HeaderName"] ?? "X-API-KEY";
        var expectedApiKey = _configuration["ApiKey:Value"];

        if (string.IsNullOrWhiteSpace(expectedApiKey))
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsync("API Key no configurada.");
            return;
        }

        if (!context.Request.Headers.TryGetValue(headerName, out var extractedApiKey))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("API Key requerida.");
            return;
        }

        if (!expectedApiKey.Equals(extractedApiKey))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("API Key inválida.");
            return;
        }

        await _next(context);
    }
}   