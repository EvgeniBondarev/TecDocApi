namespace TecDocApi.API.Models;

/// <summary>
/// Единый формат ошибок API
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Код ошибки для программной обработки
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Сообщение об ошибке для пользователя
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Дополнительные детали (только в Development)
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Путь запроса, где произошла ошибка
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// Время возникновения ошибки
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Коды ошибок API
/// </summary>
public static class ErrorCodes
{
    // Общие ошибки
    public const string INTERNAL_SERVER_ERROR = "INTERNAL_SERVER_ERROR";
    public const string VALIDATION_ERROR = "VALIDATION_ERROR";
    public const string NOT_FOUND = "NOT_FOUND";
    
    // Ошибки безопасности
    public const string UNAUTHORIZED = "UNAUTHORIZED";
    public const string FORBIDDEN = "FORBIDDEN";
    public const string RATE_LIMIT_EXCEEDED = "RATE_LIMIT_EXCEEDED";
    
    // Ошибки бизнес-логики
    public const string CONFLICT = "CONFLICT";
    public const string BAD_REQUEST = "BAD_REQUEST";
    
    // Ошибки артикулов
    public const string ARTICLE_NOT_FOUND = "ARTICLE_NOT_FOUND";
    public const string ARTICLE_INVALID = "ARTICLE_INVALID";
    
    // Ошибки поставщиков
    public const string SUPPLIER_NOT_FOUND = "SUPPLIER_NOT_FOUND";
    public const string SUPPLIER_INVALID = "SUPPLIER_INVALID";
    
    // Ошибки базы данных
    public const string DATABASE_ERROR = "DATABASE_ERROR";
    public const string DATABASE_TIMEOUT = "DATABASE_TIMEOUT";
}

