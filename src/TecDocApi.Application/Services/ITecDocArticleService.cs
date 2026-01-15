namespace TecDocApi.Application.Services;

public interface ITecDocArticleService
{
    Task<object> SearchByArticleAsync(string articleNumber, ushort? supplierId = null, CancellationToken cancellationToken = default);
    Task<object> GetByExactMatchAsync(ushort supplierId, string articleNumber, CancellationToken cancellationToken = default);
    Task<object> SearchByEanAsync(string eanCode, CancellationToken cancellationToken = default);
}

