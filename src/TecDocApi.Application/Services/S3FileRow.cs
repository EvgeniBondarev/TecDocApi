namespace TecDocApi.Application.Services;

internal sealed class S3FileRow
{
    public long Id { get; set; }

    public string Filename { get; set; } = string.Empty;

    public int? FolderId { get; set; }

    public string Extension { get; set; } = string.Empty;

    public DateTime LastModified { get; set; }

    public DateTime CreatedAt { get; set; }
}

