namespace TecDocApi.Application.Services;

internal class LinkageInfo
{
    public ushort SupplierId { get; set; }
    public string DataSupplierArticleNumber { get; set; } = string.Empty;
    public string LinkageTypeId { get; set; } = string.Empty;
    public uint LinkageId { get; set; }
}

