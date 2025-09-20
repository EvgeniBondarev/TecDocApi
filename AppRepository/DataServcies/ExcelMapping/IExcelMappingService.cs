using OzonOrdersWeb.Areas.OzonCards.ViewModels;

namespace Servcies.DataServcies.ExcelMapping;

public interface IExcelMappingService
{
    Task<int> SaveMappingAsync(ExcelMappingRequest request, string name, string createdBy = null);
    Task<ExcelMappingRequest> GetMappingAsync(int id);
    Task<List<OzonDomains.ExcelMapping>> GetAllMappingsAsync();
    Task UpdateMappingAsync(int id, ExcelMappingRequest request, string name = null);
    Task DeleteMappingAsync(int id);
}