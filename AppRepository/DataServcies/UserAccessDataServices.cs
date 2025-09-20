using OzonDomains.Models;
using OzonRepositories.Data;

namespace Servcies.DataServcies
{
    public class UserAccessDataServices : IDataServcies
    {
        private readonly UserAccessRepository _repository;

        public UserAccessDataServices(UserAccessRepository repository)
        {
            _repository = repository;
        }

        public Task<int> AddUserAccess(UserAccess value)
        {
            return _repository.Add(value);
        }

        public Task<int> DeleteUserAccess(UserAccess value)
        {
            return _repository.Delete(value);
        }

        public async Task<List<UserAccess>> GetUserAccesses()
        {
            return await _repository.Get();
        }

        public async Task<UserAccess> GetUserAccessAsync(int id)
        {
            return await _repository.GetAsync(id);
        }

        public async Task<UserAccess> GetUserAccessAsync(UserAccess value)
        {
            return await _repository.GetAsync(value);
        }

        public async Task<int> UpdateUserAccess(UserAccess value)
        {
            return await _repository.Update(value);
        }

        public async Task<int> SaveChanges()
        {
            return await _repository.SaveChanges();
        }
    }

}
