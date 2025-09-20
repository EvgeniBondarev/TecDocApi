using Microsoft.EntityFrameworkCore;
using OzonDomains.Models;
using OzonRepositories.Context;

namespace OzonRepositories.Data;

public class FileUploadRecordRepository : MainRepository, IRepository<FileUploadRecord>
{
    public FileUploadRecordRepository(OzonOrderContext dbContext) : base(dbContext)
    {
    }

    public async Task<int> Add(FileUploadRecord value)
    {
        await _context.FileUploadRecords.AddAsync(value);
        return await _context.SaveChangesAsync();
    }

    public async Task<int> Delete(FileUploadRecord value)
    {
        _context.FileUploadRecords.Remove(value);
        return await _context.SaveChangesAsync();
    }

    public async Task<List<FileUploadRecord>> Get()
    {
        return await _context.FileUploadRecords.ToListAsync();
    }
    
    public async Task<List<FileUploadRecord>> GetPaginatedAsync(int pageNumber, int pageSize)
    {
        return await _context.FileUploadRecords
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<FileUploadRecord> GetAsync(int id)
    {
        return await _context.FileUploadRecords.FindAsync(id);
    }

    public async Task<FileUploadRecord> GetAsync(FileUploadRecord value)
    {
        return await _context.FileUploadRecords
            .SingleOrDefaultAsync(f => f.FileName == value.FileName);
    }

    public async Task<int> Update(FileUploadRecord value)
    {
        _context.FileUploadRecords.Update(value);
        return await _context.SaveChangesAsync();
    }
}