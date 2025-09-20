using OzonDomains.Models;
using OzonRepositories.Data;

namespace Servcies.DataServcies;

public class FileUploadRecordDataService : IDataServcies
{
    private readonly FileUploadRecordRepository _repository;

    public FileUploadRecordDataService(FileUploadRecordRepository repository)
    {
        _repository = repository;
    }

    public Task<int> AddFileUploadRecord(FileUploadRecord value)
    {
        return _repository.Add(value);
    }

    public Task<int> DeleteFileUploadRecord(FileUploadRecord value)
    {
        return _repository.Delete(value);
    }

    public async Task<List<FileUploadRecord>> GetFileUploadRecords()
    {
        return await _repository.Get();
    }

    public async Task<FileUploadRecord> GetFileUploadRecordAsync(int id)
    {
        return await _repository.GetAsync(id);
    }

    public async Task<FileUploadRecord> GetFileUploadRecordAsync(FileUploadRecord value)
    {
        return await _repository.GetAsync(value);
    }

    public async Task<List<FileUploadRecord>> GetPaginatedAsync(int pageNumber = 1, int pageSize = 15)
    {
        return await _repository.GetPaginatedAsync(pageNumber, pageSize);
    }

    public async Task<int> UpdateFileUploadRecord(FileUploadRecord value)
    {
        return await _repository.Update(value);
    }

    public async Task<int> SaveChanges()
    {
        return await _repository.SaveChanges();
    }
}