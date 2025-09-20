using Microsoft.EntityFrameworkCore;
using OzonDomains.Models.MatchedRowSys;
using OzonRepositories.Context;
using System.Text.Json;


namespace OzonRepositories.Data
{
    public class MatchedRowRepository : MainRepository, IRepository<MatchedRow>
    {
        public MatchedRowRepository(OzonOrderContext orderContext) : base(orderContext)
        {
        }

        public async Task<int> Add(MatchedRow value)
        {
            _context.MatchedRows.Add(value);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> AddRange(List<MatchedRow> values)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.MatchedRows.AddRange(values);
                    var result = await _context.SaveChangesAsync();

                    // Подтверждаем транзакцию
                    await transaction.CommitAsync();

                    return result;
                }
                catch
                {
                    // В случае ошибки откатываем транзакцию
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }


        public async Task<int> Delete(MatchedRow value)
        {
            _context.MatchedRows.Remove(value);
            return await _context.SaveChangesAsync();
        }

        public async Task<List<MatchedRow>> Get()
        {
            return await _context.MatchedRows.ToListAsync();
        }

        public async Task<MatchedRow> GetAsync(int id)
        {
            return await _context.MatchedRows.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<MatchedRow> GetAsync(MatchedRow value)
        {
            var file1DataSerialized = JsonSerializer.Serialize(value.File1Data);
            var file2DataSerialized = JsonSerializer.Serialize(value.File2Data);

            return await _context.MatchedRows.FirstOrDefaultAsync(m =>
                m.File1DataSerialized == file1DataSerialized &&
                m.File2DataSerialized == file2DataSerialized);
        }

        public async Task<int> Update(MatchedRow value)
        {
            _context.MatchedRows.Update(value);
            return await _context.SaveChangesAsync();
        }
    }

}
