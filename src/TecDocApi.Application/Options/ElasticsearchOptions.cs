namespace TecDocApi.Application.Options;

/// <summary>
/// Настройки подключения и конфигурации Elasticsearch
/// </summary>
public class ElasticsearchOptions
{
    public const string SectionName = "Elasticsearch";

    /// <summary>URL Elasticsearch, например http://localhost:9200</summary>
    public string Url { get; set; } = "http://localhost:9200";

    /// <summary>Имя индекса для артикулов</summary>
    public string IndexName { get; set; } = "articles";

    /// <summary>Имя индекса для поставщиков</summary>
    public string SupplierIndexName { get; set; } = "suppliers";

    /// <summary>Размер пакета при массовой индексации артикулов</summary>
    public int BulkSize { get; set; } = 1000;

    /// <summary>Размер пакета при массовой индексации поставщиков</summary>
    public int SupplierBulkSize { get; set; } = 500;

    /// <summary>Имя пользователя для Basic-аутентификации (опционально)</summary>
    public string? Username { get; set; }

    /// <summary>Пароль для Basic-аутентификации (опционально)</summary>
    public string? Password { get; set; }
}

