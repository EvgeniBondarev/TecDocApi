using OzonDomains;
using OzonRepositories.Data;

namespace Servcies.DataServcies;

public class DeliveryDataServcies
{
    private readonly DeliveryRepository _repository;

    public DeliveryDataServcies(DeliveryRepository repository)
    {
        _repository = repository;
    }

    public async Task<Delivery> GetOrCreateDeliveryAsync(string name, string? provider = null)
    {
        return await _repository.GetOrCreateDeliveryAsync(name, provider);
    }

    public async Task<List<Delivery>> GetDeliveriesAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Delivery?> GetDeliveryAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }
}