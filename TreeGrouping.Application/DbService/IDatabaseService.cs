using ConsoleApp1;
using TreeGrouping.Application.DbService.Models;

namespace TreeGrouping.Application.DbService;

public interface IDatabaseService
{
    Task<IEnumerable<CategoryModel>> ExecuteStoredProcedureAsync(StoredProcedureType type, object parameters = null);
    IEnumerable<CategoryModel> ExecuteStoredProcedure(StoredProcedureType type, object? parameters = null);
    Task<IEnumerable<CategoryLinkModel>> GetAllCategoryLinksAsync();
}