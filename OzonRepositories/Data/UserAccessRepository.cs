using OzonDomains.Models;
using OzonRepositories.Context;
using Microsoft.EntityFrameworkCore;

namespace OzonRepositories.Data
{
    public class UserAccessRepository : MainRepository, IRepository<UserAccess>
    {
        public UserAccessRepository(OzonOrderContext orderContext) : base(orderContext)
        {
        }

        public async Task<int> Add(UserAccess value)
        {
            _context.UserAccess.Add(value);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(UserAccess value)
        {
            _context.UserAccess.Remove(value);
            return await _context.SaveChangesAsync();
        }

        public async Task<List<UserAccess>> Get()
        {
            return await _context.UserAccess.ToListAsync();
        }

        public async Task<UserAccess> GetAsync(int id)
        {
            return await _context.UserAccess.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<UserAccess> GetAsync(UserAccess value)
        {
            return await _context.UserAccess.FirstOrDefaultAsync(u => u.Name == value.Name);
        }

        public async Task<int> Update(UserAccess value)
        {
            _context.UserAccess.Update(value);
            return await _context.SaveChangesAsync();
        }
    }

}
