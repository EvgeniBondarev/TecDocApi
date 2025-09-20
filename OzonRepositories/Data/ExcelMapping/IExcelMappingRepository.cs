using OzonDomains;
using OzonOrdersWeb.Areas.OzonCards.ViewModels;

public interface IExcelMappingRepository
{
    Task<ExcelMapping> GetByIdAsync(int id);
    Task<List<ExcelMapping>> GetAllAsync();
    Task<int> CreateAsync(ExcelMapping mapping);
    Task UpdateAsync(ExcelMapping mapping);
    Task DeleteAsync(int id);
    Task<ExcelMappingRequest> GetRequestByIdAsync(int id);
}