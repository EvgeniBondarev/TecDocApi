namespace TecDocApi.Application.Options;

/// <summary>
/// Строки подключения к базам данных.
/// Значения могут быть переопределены через переменные окружения
/// TECDOC_DATABASE_CONNECTION_STRING и S3INFO_DATABASE_CONNECTION_STRING.
/// </summary>
public class DatabaseConnectionOptions
{
    public const string SectionName = "ConnectionStrings";

    /// <summary>Строка подключения к базе данных TecDoc (MySQL)</summary>
    public string TecDocDatabase { get; set; } = string.Empty;

    /// <summary>
    /// Строка подключения к базе данных S3Info (опционально).
    /// Если не задана — вычисляется из TecDocDatabase заменой имени БД на S3Info.
    /// </summary>
    public string? S3InfoDatabase { get; set; }
}

