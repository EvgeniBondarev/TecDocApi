using OzonDomains.Models;
using OzonRepositories.Data;

namespace Servcies.DataServcies
{
    public class OrdersFileMetadataDataService : IDataServcies
    {
        private readonly OrdersFileMetadataRepository _repository;

        public OrdersFileMetadataDataService(OrdersFileMetadataRepository repository)
        {
            _repository = repository;
        }

        public Task<int> AddOrdersFileMetadata(OrdersFileMetadata value)
        {
            return _repository.Add(value);
        }

        public async Task<OrdersFileMetadata> GetOrdersFileMetadataAsync(OrdersFileMetadata value)
        {
            return await _repository.GetAsync(value);
        }
        
        public async Task<int> SaveChanges()
        {
            return await _repository.SaveChanges();
        }
    }

}
