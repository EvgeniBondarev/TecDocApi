using OzonDomains.Models;
using OzonRepositories.Data;

namespace Servcies.DataServcies
{
    public class OzonClientServcies : IDataServcies
    {
        private readonly OzonClientRepository _repository;
        private readonly CryptographyServcies _cryptography;
        private readonly OrdersDataServcies _ordersDataServcies;

        public OzonClientServcies(OzonClientRepository repository,
                                  CryptographyServcies cryptography,
                                  OrdersDataServcies ordersDataServcies)
        {
            _repository = repository;
            _cryptography = cryptography;
            _ordersDataServcies = ordersDataServcies;
        }
        public async Task<int> AddOzonClient(OzonClient value)
        {
            if (value.ClientId != null && value.ApiKey != null)
            {
                value.ClientId = _cryptography.Encrypt(value.ClientId);
                value.ApiKey = _cryptography.Encrypt(value.ApiKey);
            }
            return await _repository.Add(value);
        }

        public async Task<int> DeleteOzonClient(OzonClient value)
        {
            var clientOrders = await _ordersDataServcies.GetOrders();
            var ordersToUpdate = clientOrders.Where(order => order.OzonClient == value).ToList();

            foreach (var order in ordersToUpdate)
            {
                order.OzonClient = null;
                _ordersDataServcies.UpdateOrder(order);
            }

            return await _repository.Delete(value);
        }

        public async Task<List<OzonClient>> GetOzonClients()
        {
            List<OzonClient> ozonClients = await _repository.Get();
            foreach (var ozonClient in ozonClients)
            {
                if (ozonClient != null && ozonClient.ClientId != null && ozonClient.ApiKey != null)
                {
                    ozonClient.DecryptClientId = _cryptography.Decrypt(ozonClient.ClientId);
                    ozonClient.DecryptApiKey = _cryptography.Decrypt(ozonClient.ApiKey);
                }
            }

            return ozonClients;
        }
        
        public async Task<OzonClient> GetOzonClientAsync(int id)
        {
            var ozonClient = await _repository.GetAsync(id);
            if (ozonClient != null && ozonClient.ClientId != null && ozonClient.ApiKey != null)
            {
                ozonClient.DecryptClientId = _cryptography.Decrypt(ozonClient.ClientId);
                ozonClient.DecryptApiKey = _cryptography.Decrypt(ozonClient.ApiKey);
            }
            return ozonClient;
        }

        public async Task<OzonClient> GetOzonClientAsync(OzonClient value)
        {
            var ozonClient = await _repository.GetAsync(value);
            if (ozonClient != null && ozonClient.ClientId != null && ozonClient.ApiKey != null)
            {
                ozonClient.DecryptClientId = _cryptography.Decrypt(ozonClient.ClientId);
                ozonClient.DecryptApiKey = _cryptography.Decrypt(ozonClient.ApiKey);
            }
            return ozonClient;
        }

        public async Task<int> Update(OzonClient value)
        {
            if (value.ClientId != null && value.ApiKey != null)
            {
                value.ClientId = _cryptography.Encrypt(value.ClientId);
                value.ApiKey = _cryptography.Encrypt(value.ApiKey);
            }

            return await _repository.Update(value);
        }
        public Task<int> SaveChanges()
        {
            return _repository.SaveChanges();
        }
    }
}
