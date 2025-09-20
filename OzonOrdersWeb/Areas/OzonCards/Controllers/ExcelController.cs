using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using OzonOrdersWeb.Areas.OzonCards.ViewModels;
using Servcies.DataServcies.ExcelMapping;

namespace ExcelProcessor.Controllers
{
    [Area("OzonCards")]
    [Authorize(Roles = "User,Admin")]
    public class ExcelController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IExcelMappingService _mappingService;
        public ExcelController(IWebHostEnvironment environment,
                               IExcelMappingService mappingService)
        {
            _environment = environment;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            _mappingService = mappingService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? mappingId)
        {
            if(mappingId == null)
                return View();
            
            var mapping = await _mappingService.GetMappingAsync(mappingId.Value);
            return View(mapping);
        }
        
        

        [HttpPost]
        public async Task<IActionResult> UploadFiles(IFormFile sourceFile, IFormFile destinationFile)
        {
            try
            {
                if (sourceFile == null || destinationFile == null)
                {
                    return Json(new { success = false, message = "Оба файла должны быть загружены" });
                }

                // Сохраняем файлы и получаем их имена
                var sourceFileName = await SaveFile(sourceFile);
                var destFileName = await SaveFile(destinationFile);

                var fileInfo = await GetFileInfo(sourceFile, destinationFile);

                return Json(new
                {
                    success = true,
                    sourceFileName,
                    destFileName,
                    sourceSheets = fileInfo.SourceSheets,
                    destinationSheets = fileInfo.DestinationSheets,
                    sourceColumns = fileInfo.SourceColumns,
                    destinationColumns = fileInfo.DestinationColumns
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ProcessExcel([FromForm] ExcelMappingRequest request,  string MappingName=null)
        {
            try
            {
                if (string.IsNullOrEmpty(request.SourceFileName) || string.IsNullOrEmpty(request.DestinationFileName))
                {
                    return BadRequest("Файлы не были загружены");
                }

                if (request.ColumnMappings == null || !request.ColumnMappings.Any())
                {
                    return BadRequest("Не указаны сопоставления столбцов");
                }

                var result = await ProcessExcelFiles(request);


                DeleteTempFile(request.SourceFileName);
                DeleteTempFile(request.DestinationFileName);

                if (result.Success)
                {
                    if (MappingName != null)
                    {
                        var id = await _mappingService.SaveMappingAsync(request, MappingName, User.Identity.Name);
                    }
                    
                    return File(result.ResultFile, 
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                        result.FileName);
                }
                else
                {
                    return BadRequest(result.Message);
                }
            }
            catch (Exception ex)
            {
                DeleteTempFile(request.SourceFileName);
                DeleteTempFile(request.DestinationFileName);
                return StatusCode(500, $"Ошибка обработки: {ex.Message}");
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> GetColumnsInfo(
            IFormFile sourceFile, 
            IFormFile destinationFile, 
            string sourceSheet, 
            string destinationSheet, 
            int sourceHeaderRow = 1, 
            int destinationHeaderRow = 1)
        {
            try
            {
                if (sourceFile == null || destinationFile == null)
                {
                    return Json(new { success = false, message = "Файлы не были загружены" });
                }

                using (var sourceStream = new MemoryStream())
                using (var destStream = new MemoryStream())
                {
                    await sourceFile.CopyToAsync(sourceStream);
                    await destinationFile.CopyToAsync(destStream);

                    sourceStream.Position = 0;
                    destStream.Position = 0;

                    using (var sourcePackage = new ExcelPackage(sourceStream))
                    using (var destPackage = new ExcelPackage(destStream))
                    {
                        var sourceWorksheet = sourcePackage.Workbook.Worksheets
                            .FirstOrDefault(ws => ws.Name.Equals(sourceSheet, StringComparison.OrdinalIgnoreCase));
                        
                        var destWorksheet = destPackage.Workbook.Worksheets
                            .FirstOrDefault(ws => ws.Name.Equals(destinationSheet, StringComparison.OrdinalIgnoreCase));

                        if (sourceWorksheet == null || destWorksheet == null)
                        {
                            return Json(new { success = false, message = "Указанные листы не найдены" });
                        }

                        var sourceHeaders = GetHeaders(sourceWorksheet, sourceHeaderRow);
                        var destHeaders = GetHeaders(destWorksheet, destinationHeaderRow);

                        var sourceExamples = GetColumnExamples(sourceWorksheet, sourceHeaderRow + 1, sourceHeaders);
                        var destExamples = GetColumnExamples(destWorksheet, destinationHeaderRow + 1, destHeaders);

                        return Json(new
                        {
                            success = true,
                            sourceColumns = sourceHeaders.Keys.Select(k => new 
                            { 
                                name = k, 
                                example = sourceExamples.ContainsKey(k) ? sourceExamples[k] : "" 
                            }),
                            destinationColumns = destHeaders.Keys.Select(k => new 
                            { 
                                name = k, 
                                example = destExamples.ContainsKey(k) ? destExamples[k] : "" 
                            })
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private async Task<string> SaveFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        private void DeleteTempFile(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                var filePath = Path.Combine(_environment.WebRootPath, "uploads", fileName);
                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        System.IO.File.Delete(filePath);
                    }
                    catch
                    {
                        // Игнорируем ошибки удаления
                    }
                }
            }
        }

        private Dictionary<string, string> GetColumnExamples(ExcelWorksheet worksheet, int dataRow, Dictionary<string, int> headers)
        {
            var examples = new Dictionary<string, string>();
            
            foreach (var header in headers)
            {
                var value = worksheet.Cells[dataRow, header.Value].Value?.ToString();
                examples[header.Key] = value ?? "нет данных";
            }

            return examples;
        }

        private async Task<FileInfoResponse> GetFileInfo(IFormFile sourceFile, IFormFile destinationFile)
        {
            var response = new FileInfoResponse();

            using (var sourceStream = new MemoryStream())
            using (var destStream = new MemoryStream())
            {
                await sourceFile.CopyToAsync(sourceStream);
                await destinationFile.CopyToAsync(destStream);

                sourceStream.Position = 0;
                destStream.Position = 0;

                using (var sourcePackage = new ExcelPackage(sourceStream))
                using (var destPackage = new ExcelPackage(destStream))
                {
                    response.SourceSheets = sourcePackage.Workbook.Worksheets
                        .Select(ws => ws.Name)
                        .ToList();

                    response.DestinationSheets = destPackage.Workbook.Worksheets
                        .Select(ws => ws.Name)
                        .ToList();

                    if (response.SourceSheets.Any())
                    {
                        var firstSourceSheet = sourcePackage.Workbook.Worksheets[response.SourceSheets[0]];
                        response.SourceColumns = GetHeaders(firstSourceSheet, 1).Keys.ToList();
                    }

                    if (response.DestinationSheets.Any())
                    {
                        var firstDestSheet = destPackage.Workbook.Worksheets[response.DestinationSheets[0]];
                        response.DestinationColumns = GetHeaders(firstDestSheet, 1).Keys.ToList();
                    }
                }
            }

            return response;
        }

        private async Task<ExcelProcessingResult> ProcessExcelFiles(ExcelMappingRequest request)
        {
            var sourceFilePath = Path.Combine(_environment.WebRootPath, "uploads", request.SourceFileName);
            var destFilePath = Path.Combine(_environment.WebRootPath, "uploads", request.DestinationFileName);

            if (!System.IO.File.Exists(sourceFilePath) || !System.IO.File.Exists(destFilePath))
            {
                return new ExcelProcessingResult 
                { 
                    Success = false, 
                    Message = "Файлы не найдены на сервере" 
                };
            }

            using (var sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
            using (var destStream = new FileStream(destFilePath, FileMode.Open, FileAccess.ReadWrite))
            {
                using (var sourcePackage = new ExcelPackage(sourceStream))
                using (var destPackage = new ExcelPackage(destStream))
                {
                    var sourceWorksheet = sourcePackage.Workbook.Worksheets
                        .FirstOrDefault(ws => ws.Name.Equals(request.SourceSheet, StringComparison.OrdinalIgnoreCase));
                    
                    var destWorksheet = destPackage.Workbook.Worksheets
                        .FirstOrDefault(ws => ws.Name.Equals(request.DestinationSheet, StringComparison.OrdinalIgnoreCase));

                    if (sourceWorksheet == null)
                    {
                        return new ExcelProcessingResult 
                        { 
                            Success = false, 
                            Message = $"Лист '{request.SourceSheet}' не найден в исходном файле" 
                        };
                    }

                    if (destWorksheet == null)
                    {
                        return new ExcelProcessingResult 
                        { 
                            Success = false, 
                            Message = $"Лист '{request.DestinationSheet}' не найден в целевом файле" 
                        };
                    }

                    var columnIndexMap = GetColumnIndexMapping(sourceWorksheet, destWorksheet, request);

                    if (columnIndexMap == null)
                    {
                        return new ExcelProcessingResult 
                        { 
                            Success = false, 
                            Message = "Ошибка сопоставления столбцов" 
                        };
                    }

                    int processedRows = CopyData(sourceWorksheet, destWorksheet, request, columnIndexMap);

                    using (var resultStream = new MemoryStream())
                    {
                        destPackage.SaveAs(resultStream);
                        resultStream.Position = 0;

                        return new ExcelProcessingResult
                        {
                            Success = true,
                            Message = $"Успешно обработано {processedRows} строк",
                            ProcessedRows = processedRows,
                            ResultFile = resultStream.ToArray(),
                            FileName = $"processed_{Path.GetFileNameWithoutExtension(request.DestinationFileName)}.xlsx"
                        };
                    }
                }
            }
        }

        private Dictionary<int, int> GetColumnIndexMapping(
            ExcelWorksheet sourceWorksheet, 
            ExcelWorksheet destWorksheet, 
            ExcelMappingRequest request)
        {
            var mapping = new Dictionary<int, int>();

            var sourceHeaders = GetHeaders(sourceWorksheet, request.SourceHeaderRow);
            var destHeaders = GetHeaders(destWorksheet, request.DestinationHeaderRow);

            foreach (var columnMapping in request.ColumnMappings)
            {
                if (!sourceHeaders.ContainsKey(columnMapping.SourceColumn))
                {
                    throw new ValidationException($"Столбец '{columnMapping.SourceColumn}' не найден в исходном файле");
                }

                if (!destHeaders.ContainsKey(columnMapping.DestinationColumn))
                {
                    throw new ValidationException($"Столбец '{columnMapping.DestinationColumn}' не найден в целевом файле");
                }

                int sourceColIndex = sourceHeaders[columnMapping.SourceColumn];
                int destColIndex = destHeaders[columnMapping.DestinationColumn];

                mapping.Add(sourceColIndex, destColIndex);
            }

            return mapping;
        }

        private Dictionary<string, int> GetHeaders(ExcelWorksheet worksheet, int headerRow)
        {
            var headers = new Dictionary<string, int>();
            int col = 1;

            while (true)
            {
                var headerValue = worksheet.Cells[headerRow, col].Value?.ToString();
                if (string.IsNullOrEmpty(headerValue))
                    break;

                headers[headerValue.Trim()] = col;
                col++;
            }

            return headers;
        }

        private int CopyData(
            ExcelWorksheet sourceWorksheet, 
            ExcelWorksheet destWorksheet, 
            ExcelMappingRequest request, 
            Dictionary<int, int> columnIndexMap)
        {
            int processedRows = 0;
            int sourceRow = request.SourceDataStartRow;
            int destRow = request.DestinationDataStartRow;

            while (true)
            {
                bool hasData = false;
                foreach (var mapping in columnIndexMap)
                {
                    var sourceValue = sourceWorksheet.Cells[sourceRow, mapping.Key].Value;
                    if (sourceValue != null && !string.IsNullOrEmpty(sourceValue.ToString()))
                    {
                        hasData = true;
                        break;
                    }
                }

                if (!hasData)
                    break;

                foreach (var mapping in columnIndexMap)
                {
                    var sourceValue = sourceWorksheet.Cells[sourceRow, mapping.Key].Value;
                    destWorksheet.Cells[destRow, mapping.Value].Value = sourceValue;
                }

                sourceRow++;
                destRow++;
                processedRows++;
            }

            return processedRows;
        }

        // Периодическая очистка старых файлов
        private void CleanOldTempFiles()
        {
            try
            {
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
                if (Directory.Exists(uploadsPath))
                {
                    foreach (var file in Directory.GetFiles(uploadsPath))
                    {
                        var fileInfo = new FileInfo(file);
                        if (fileInfo.LastWriteTime < DateTime.Now.AddHours(-1))
                        {
                            fileInfo.Delete();
                        }
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки очистки
            }
        }
    }
}