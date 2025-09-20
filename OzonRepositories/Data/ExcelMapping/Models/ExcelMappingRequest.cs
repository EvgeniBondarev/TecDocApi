namespace OzonOrdersWeb.Areas.OzonCards.ViewModels;

public class ExcelMappingRequest
{
    public string SourceFileName { get; set; }
    public string DestinationFileName { get; set; }
    public string SourceSheet { get; set; }
    public string DestinationSheet { get; set; }
    public int SourceHeaderRow { get; set; } = 1;
    public int DestinationHeaderRow { get; set; } = 1;
    public int SourceDataStartRow { get; set; } = 2;
    public int DestinationDataStartRow { get; set; } = 2;
    public List<ColumnMappingExcel> ColumnMappings { get; set; }
}