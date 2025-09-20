using Microsoft.EntityFrameworkCore;
using OzonDomains;
using OzonDomains.Models;
using OzonRepositories.Context;

namespace OzonRepositories.Data;

public class DeliveryRepository
{
    private readonly OzonOrderContext _context;

    public DeliveryRepository(OzonOrderContext context)
    {
        _context = context;
    }
    
    public async Task<Delivery> GetOrCreateDeliveryAsync(string deliveryName, string providerName)
    {
        var provider = await GetOrCreateProviderAsync(providerName);

        var delivery = await _context.Deliveries
            .FirstOrDefaultAsync(d => d.Name == deliveryName && d.ProviderId == provider.Id);

        if (delivery != null)
            return delivery;

        delivery = new Delivery { Name = deliveryName, ProviderId = provider.Id };
        _context.Deliveries.Add(delivery);
        await _context.SaveChangesAsync();
        return delivery;
    }


    public async Task<List<Delivery>> GetAllAsync()
    {
        return await _context.Deliveries.ToListAsync();
    }

    public async Task<Delivery?> GetByIdAsync(int id)
    {
        return await _context.Deliveries.FindAsync(id);
    }
    
    private async Task<DeliveryProvider> GetOrCreateProviderAsync(string providerName)
    {
        var provider = await _context.DeliveryProviders
            .FirstOrDefaultAsync(p => p.Name == providerName);

        if (provider != null)
            return provider;

        provider = new DeliveryProvider { Name = providerName };
        _context.DeliveryProviders.Add(provider);
        await _context.SaveChangesAsync();
        return provider;
    }

}