
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OzonDomains;
using OzonOrdersWeb.Areas.OzonCards.ViewModels;
using OzonRepositories.Context;

public class ExcelMappingRepository : IExcelMappingRepository
{
        private readonly OzonOrderContext _context;

        public ExcelMappingRepository(OzonOrderContext context)
        {
            _context = context;
        }

        public async Task<ExcelMapping> GetByIdAsync(int id)
        {
            return await _context.ExcelMappings.FindAsync(id);
        }

        public async Task<List<ExcelMapping>> GetAllAsync()
        {
            return await _context.ExcelMappings
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();
        }

        public async Task<int> CreateAsync(ExcelMapping mapping)
        {
            _context.ExcelMappings.Add(mapping);
            await _context.SaveChangesAsync();
            return mapping.Id;
        }

        public async Task UpdateAsync(ExcelMapping mapping)
        {
            mapping.ModifiedDate = DateTime.UtcNow;
            _context.ExcelMappings.Update(mapping);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var mapping = await GetByIdAsync(id);
            if (mapping != null)
            {
                _context.ExcelMappings.Remove(mapping);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<ExcelMappingRequest> GetRequestByIdAsync(int id)
        {
            var mapping = await GetByIdAsync(id);
            if (mapping == null)
                return null;

            return new ExcelMappingRequest
            {
                SourceFileName = mapping.SourceFileName,
                DestinationFileName = mapping.DestinationFileName,
                SourceSheet = mapping.SourceSheet,
                DestinationSheet = mapping.DestinationSheet,
                SourceHeaderRow = mapping.SourceHeaderRow,
                DestinationHeaderRow = mapping.DestinationHeaderRow,
                SourceDataStartRow = mapping.SourceDataStartRow,
                DestinationDataStartRow = mapping.DestinationDataStartRow,
                ColumnMappings = JsonSerializer.Deserialize<List<ColumnMappingExcel>>(mapping.ColumnMappingsJson)
            };
        }
}