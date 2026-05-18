namespace TecDocApi.Application.Services;

internal sealed class S3SearchCandidate
{
    public string ObjectKey { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string FolderPath { get; set; } = string.Empty;

    public string NormalizedFileName { get; set; } = string.Empty;

    public string NormalizedObjectKey { get; set; } = string.Empty;

    public ushort? SupplierId { get; set; }
}

