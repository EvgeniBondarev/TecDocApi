using OzonRepositories.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzonRepositories.Data
{
    public class MainRepository
    {
        public readonly OzonOrderContext _context;
        public MainRepository(OzonOrderContext orderContext)
        {
            _context = orderContext;
        }

        public async Task<int> SaveChanges()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
