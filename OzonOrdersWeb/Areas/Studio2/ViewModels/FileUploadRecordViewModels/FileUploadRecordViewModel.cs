using OzonDomains.Models;

namespace OzonOrdersWeb.Areas.Studio2.ViewModels.FileUploadRecordViewModels;

public class FileUploadRecordViewModel
{
    public List<FileUploadRecord> Items { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public string NameFilter { get; set; }
    public DateTime? DateFromFilter { get; set; }
    public DateTime? DateToFilter { get; set; }
}