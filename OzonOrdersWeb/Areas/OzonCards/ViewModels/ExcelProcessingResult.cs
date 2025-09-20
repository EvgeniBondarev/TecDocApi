public class ExcelProcessingResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public int ProcessedRows { get; set; }
    public byte[] ResultFile { get; set; }
    public string FileName { get; set; }
}