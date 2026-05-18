namespace TecDocApi.Application.Services;

internal sealed class FolderRow
{
    public int Id { get; set; }

    public string FolderName { get; set; } = string.Empty;

    public int? ParentId { get; set; }
}

