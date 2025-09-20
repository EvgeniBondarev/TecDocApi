using System.Data;
using System.Data.SqlClient;
using ConsoleApp1;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using TreeGrouping.Application.DbService;
using TreeGrouping.Application.DbService.Models;

public class DatabaseService:IDatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultTreeConnection") 
                            ?? throw new InvalidOperationException("Connection string not found");
    }

    public async Task<IEnumerable<CategoryModel>> ExecuteStoredProcedureAsync(StoredProcedureType type, object parameters = null)
    {
        using var connection = new SqlConnection(_connectionString);
        
        var procedureName = type.GetProcedureName();
        var paramDict = type.GetParameter(parameters);

        return await connection.QueryAsync<CategoryModel>(
            procedureName, paramDict, commandType: CommandType.StoredProcedure
        );
    }
    
    public IEnumerable<CategoryModel> ExecuteStoredProcedure(StoredProcedureType type, object? parameters = null)
    {
        using var connection = new SqlConnection(_connectionString);
    
        var procedureName = type.GetProcedureName();
        var paramDict = type.GetParameter(parameters);

        return connection.Query<CategoryModel>(
            procedureName, paramDict, commandType: CommandType.StoredProcedure
        );
    }

    
    public async Task<IEnumerable<CategoryLinkModel>> GetAllCategoryLinksAsync()
    {
        using var connection = new SqlConnection(_connectionString);
    
        return await connection.QueryAsync<CategoryLinkModel>(
            "GetAllCategoryLinks", 
            commandType: CommandType.StoredProcedure
        );
    }
}