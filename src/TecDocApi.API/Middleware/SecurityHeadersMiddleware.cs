namespace TecDocApi.API.Middleware;

/// <summary>
/// Middleware для добавления HTTP заголовков безопасности
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityHeadersMiddleware> _logger;

    public SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["Referrer-Policy"] = "no-referrer";
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
        context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
        var isDocumentationPage = path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
                                   path.StartsWith("/docs", StringComparison.OrdinalIgnoreCase);
        
        if (isDocumentationPage)
        {
            // ReDoc требует inline стили, Web Workers и внешние изображения для логотипа
            context.Response.Headers["Content-Security-Policy"] = 
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval' blob:; " +
                "worker-src 'self' blob:; " +
                "style-src 'self' 'unsafe-inline'; " +
                "img-src 'self' data: https: http://studio-web-client.interparts.ru; " +
                "font-src 'self' data:; " +
                "connect-src 'self'";
        }
        else
        {
            context.Response.Headers["Content-Security-Policy"] =
                "default-src 'self'; " +
                "img-src 'self' data: https://s3.timeweb.cloud";
        }

        await _next(context);
    }
}

