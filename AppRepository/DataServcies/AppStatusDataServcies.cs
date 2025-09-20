using OzonDomains.Models;
using OzonRepositories.Data;

namespace Servcies.DataServcies
{
    public class AppStatusDataServcies : IDataServcies
    {
        private readonly AppStatusRepository _repository;

        public AppStatusDataServcies(AppStatusRepository repository)
        {
            _repository = repository;
        }

        public Task<int> AddAppStatus(AppStatus value)
        {
            return _repository.Add(value);
        }

        public Task<int> DeleteAppStatus(AppStatus value)
        {
            return (_repository.Delete(value));
        }

        public async Task<List<AppStatus>> GetAppStatuses()
        {
            return await _repository.Get();
        }

        public async Task<AppStatus> GetAppStatusAsync(int id)
        {
            return await _repository.GetAsync(id);
        }

        public async Task<AppStatus> GetAppStatusAsync(AppStatus value)
        {
            return await _repository.GetAsync(value);
        }

        public async Task<int> UpdateAppStatus(AppStatus value)
        {
            return (await _repository.Update(value));
        }

        public async Task<int> SaveChanges()
        {
            return await _repository.SaveChanges();
        }
        public async Task UpdateStatusColor(int statusId, string colorCode)
        {
            await _repository.UpdateStatusColor(statusId, colorCode);
        }

        public async Task ResetStatusColor(int statusId)
        {
            await _repository.ResetStatusColor(statusId);
        }
    }
}
