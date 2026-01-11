using TecDocApi.API.Models;

namespace TecDocApi.API.Exceptions;

/// <summary>
/// Базовое исключение API с кодом ошибки
/// </summary>
public class ApiException : Exception
{
    public string Code { get; }
    public int StatusCode { get; }

    public ApiException(string code, string message, int statusCode = 500) 
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }

    public ApiException(string code, string message, Exception innerException, int statusCode = 500) 
        : base(message, innerException)
    {
        Code = code;
        StatusCode = statusCode;
    }
}

/// <summary>
/// Исключение для ошибок валидации (400)
/// </summary>
public class ValidationException : ApiException
{
    public ValidationException(string message) 
        : base(ErrorCodes.VALIDATION_ERROR, message, 400)
    {
    }
}

/// <summary>
/// Исключение для ошибок "Не найдено" (404)
/// </summary>
public class NotFoundException : ApiException
{
    public NotFoundException(string code, string message) 
        : base(code, message, 404)
    {
    }
}

/// <summary>
/// Исключение для ошибок конфликта (409)
/// </summary>
public class ConflictException : ApiException
{
    public ConflictException(string code, string message) 
        : base(code, message, 409)
    {
    }
}

