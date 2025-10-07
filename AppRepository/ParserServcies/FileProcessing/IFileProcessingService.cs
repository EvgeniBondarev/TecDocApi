using Microsoft.AspNetCore.Http;

namespace Servcies.ParserServcies.FileProcessing;

public interface IFileProcessingService
{
    Task<List<(string OrderNumber, string Article)>> ProcessExcelFileAsync(IFormFile file);
    Task<List<(string OrderNumber, string Article)>> ProcessCsvFileAsync(IFormFile file);
    Task<List<(string OrderNumber, string Article)>> ProcessFileAsync(IFormFile file);
}