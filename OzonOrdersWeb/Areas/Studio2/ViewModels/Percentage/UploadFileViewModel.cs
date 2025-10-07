using OzonDomains;

namespace OzonOrdersWeb.Areas.Studio2.ViewModels.Percentage;

public class UploadFileViewModel
{
    public IFormFile ExcelFile { get; set; }
    public TransactionType SelectedTransactionType { get; set; }
}