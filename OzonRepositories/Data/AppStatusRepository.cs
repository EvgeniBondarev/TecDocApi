using Microsoft.EntityFrameworkCore;
using OzonDomains;
using OzonDomains.Models;
using OzonRepositories.Context;

namespace OzonRepositories.Data
{
    public class AppStatusRepository : MainRepository, IRepository<AppStatus>
    {
        public AppStatusRepository(OzonOrderContext orderContext) : base(orderContext)
        {
        }

        public Task<int> Add(AppStatus value)
        {
            _context.AppStatuses.AddAsync(value);
            return _context.SaveChangesAsync();
        }

        public Task<int> Delete(AppStatus value)
        {
            _context.Remove(value);
            return _context.SaveChangesAsync();
        }

        public async Task<List<AppStatus>> Get()
        {
            return await _context.AppStatuses
                .Include(s => s.StatusColor)
                .ToListAsync();
        }

        public async Task<AppStatus> GetAsync(int id)
        {
            return await _context.AppStatuses
                .Include(s => s.StatusColor)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<AppStatus> GetAsync(AppStatus value)
        {
            return _context.AppStatuses.Include(s => s.StatusColor).SingleOrDefault(a => a.Name == value.Name);
        }

        public async Task<int> Update(AppStatus value)
        {
            _context.AppStatuses.Update(value);
            return _context.SaveChanges();
        }
        public async Task UpdateStatusColor(int statusId, string colorCode)
        {
            var status = await _context.AppStatuses.FindAsync(statusId);
            if (status != null)
            {
                var color = await _context.StatusColors.FirstOrDefaultAsync(c => c.ColorCode == colorCode);
                if (color == null)
                {
                    color = new StatusColor { ColorCode = colorCode };
                    await _context.StatusColors.AddAsync(color);
                    await _context.SaveChangesAsync();
                }
        
                status.StatusColorId = color.Id;
                await _context.SaveChangesAsync();
            }
        }

        public async Task ResetStatusColor(int statusId)
        {
            var status = await _context.AppStatuses.FindAsync(statusId);
            if (status != null)
            {
                status.StatusColorId = null;
                await _context.SaveChangesAsync();
            }
        }
    }
}
