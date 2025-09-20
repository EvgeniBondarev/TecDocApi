using System.Text.Json;
using OzonOrdersWeb.Areas.OzonCards.ViewModels;

namespace Servcies.DataServcies.ExcelMapping;

public class ExcelMappingService : IExcelMappingService
    {
        private readonly IExcelMappingRepository _repository;

        public ExcelMappingService(IExcelMappingRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> SaveMappingAsync(ExcelMappingRequest request, string name, string createdBy = null)
        {
            var mapping = new OzonDomains.ExcelMapping
            {
                Name = name,
                SourceFileName = request.SourceFileName,
                DestinationFileName = request.DestinationFileName,
                SourceSheet = request.SourceSheet,
                DestinationSheet = request.DestinationSheet,
                SourceHeaderRow = request.SourceHeaderRow,
                DestinationHeaderRow = request.DestinationHeaderRow,
                SourceDataStartRow = request.SourceDataStartRow,
                DestinationDataStartRow = request.DestinationDataStartRow,
                ColumnMappingsJson = JsonSerializer.Serialize(request.ColumnMappings),
                CreatedBy = createdBy
            };

            return await _repository.CreateAsync(mapping);
        }

        public async Task<ExcelMappingRequest> GetMappingAsync(int id)
        {
            return await _repository.GetRequestByIdAsync(id);
        }

        public async Task<List<OzonDomains.ExcelMapping>> GetAllMappingsAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task UpdateMappingAsync(int id, ExcelMappingRequest request, string name = null)
        {
            var existingMapping = await _repository.GetByIdAsync(id);
            if (existingMapping == null)
                throw new ArgumentException("Mapping not found");

            if (!string.IsNullOrEmpty(name))
                existingMapping.Name = name;

            existingMapping.SourceFileName = request.SourceFileName;
            existingMapping.DestinationFileName = request.DestinationFileName;
            existingMapping.SourceSheet = request.SourceSheet;
            existingMapping.DestinationSheet = request.DestinationSheet;
            existingMapping.SourceHeaderRow = request.SourceHeaderRow;
            existingMapping.DestinationHeaderRow = request.DestinationHeaderRow;
            existingMapping.SourceDataStartRow = request.SourceDataStartRow;
            existingMapping.DestinationDataStartRow = request.DestinationDataStartRow;
            existingMapping.ColumnMappingsJson = JsonSerializer.Serialize(request.ColumnMappings);
            existingMapping.ModifiedDate = DateTime.UtcNow;

            await _repository.UpdateAsync(existingMapping);
        }

        public async Task DeleteMappingAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }
    }