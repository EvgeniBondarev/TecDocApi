namespace TecDocApi.Application.Options;

/// <summary>
/// Настройки S3-совместимого хранилища (Timeweb Cloud)
/// </summary>
public class S3Options
{
    public const string SectionName = "S3";

    /// <summary>Access Key для аутентификации</summary>
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>Secret Key для аутентификации</summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>URL эндпоинта S3, например https://s3.timeweb.cloud</summary>
    public string EndpointUrl { get; set; } = string.Empty;

    /// <summary>Название региона</summary>
    public string RegionName { get; set; } = "ru-1";

    /// <summary>Название бакета</summary>
    public string BucketName { get; set; } = string.Empty;

    /// <summary>Базовый путь внутри бакета</summary>
    public string BasePath { get; set; } = "TD2018/images";
}

