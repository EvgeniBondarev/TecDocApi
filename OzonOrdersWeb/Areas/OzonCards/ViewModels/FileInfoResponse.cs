namespace OzonOrdersWeb.Areas.OzonCards.ViewModels;

public class FileInfoResponse
{
    public List<string> SourceSheets { get; set; }
    public List<string> DestinationSheets { get; set; }
    public List<string> SourceColumns { get; set; }
    public List<string> DestinationColumns { get; set; }
}