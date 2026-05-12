using System.Security;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TecDocApi.API.Exceptions;
using TecDocApi.API.Models;

namespace TecDocApi.API.Middleware;

/// <summary>
/// Централизованный обработчик исключений
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;

        // Определяем код ошибки и HTTP статус
        var (errorCode, statusCode, message) = GetErrorInfo(exception);

        // Логируем безопасность: подозрительные ошибки
        if (exception is UnauthorizedAccessException || exception is SecurityException)
        {
            _logger.LogWarning(
                "SECURITY: {ExceptionType} от IP {ClientIp} на {Method} {Path}",
                exception.GetType().Name,
                clientIp,
                requestMethod,
                requestPath);
        }

        // Логируем все исключения
        _logger.LogError(
            exception,
            "Ошибка при обработке запроса {Method} {Path} от IP {ClientIp}, Code: {ErrorCode}",
            requestMethod,
            requestPath,
            clientIp,
            errorCode);

        response.StatusCode = statusCode;

        // Создаем единый формат ошибки
        var errorResponse = new ErrorResponse
        {
            Code = errorCode,
            Message = message,
            Path = requestPath.ToString(),
            Timestamp = DateTime.UtcNow
        };

        // В Development добавляем детали
        if (_environment.IsDevelopment())
        {
            errorResponse.Details = exception.ToString();
        }

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(jsonResponse);
    }

    private (string Code, int StatusCode, string Message) GetErrorInfo(Exception exception)
    {
        // API исключения
        if (exception is ApiException apiEx)
        {
            return (apiEx.Code, apiEx.StatusCode, apiEx.Message);
        }

        // Валидация
        if (exception is ArgumentNullException)
        {
            return (ErrorCodes.VALIDATION_ERROR, 400, "Required parameter is missing");
        }
        
        if (exception is ArgumentException argEx)
        {
            return (ErrorCodes.VALIDATION_ERROR, 400, argEx.Message);
        }

        // Безопасность
        if (exception is UnauthorizedAccessException)
        {
            return (ErrorCodes.UNAUTHORIZED, 401, "Unauthorized access");
        }
        
        if (exception is SecurityException)
        {
            return (ErrorCodes.FORBIDDEN, 403, "Access forbidden");
        }

        // Не найдено
        if (exception is KeyNotFoundException)
        {
            return (ErrorCodes.NOT_FOUND, 404, "Resource not found");
        }

        // База данных
        if (exception is DbUpdateException)
        {
            return (ErrorCodes.DATABASE_ERROR, 500, "Database operation failed");
        }
        
        if (exception is TimeoutException)
        {
            return (ErrorCodes.DATABASE_TIMEOUT, 504, "Request timeout");
        }

        // По умолчанию
        return (ErrorCodes.INTERNAL_SERVER_ERROR, 500, 
            _environment.IsDevelopment() ? exception.Message : "Internal server error");
    }
}

