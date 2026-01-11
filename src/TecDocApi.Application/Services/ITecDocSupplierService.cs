namespace TecDocApi.Application.Services;

public interface ITecDocSupplierService
{
    Task<object> SearchSuppliersAsync(string? matchcode = null, ushort? id = null, CancellationToken cancellationToken = default);
    Task<object> GetSupplierByIdAsync(ushort id, CancellationToken cancellationToken = default);
}

