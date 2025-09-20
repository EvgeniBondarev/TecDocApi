using Microsoft.AspNetCore.Mvc;
using OzonDomains;
using OzonDomains.Models.MatchedRowSys;
using OzonOrdersWeb.ViewModels.MatchFileDataViewModel;
using OzonOrdersWeb.ViewModels.OrderViewModels;
using OzonRepositories.Context;
using Servcies.DataServcies;
using Servcies.ParserServcies.FielParsers;
using Servcies.ParserServcies.FielParsers.Models;

namespace OzonOrdersWeb.Controllers
{
    [Area("Studio2")]
    public class MatchFileDataController : Controller
    {
        private readonly ExcelParser _excelParser;
        private readonly MatchedResultDataServices _matchedResultDataServices;
        private readonly MatchedRowDataServices _matchedRowDataServices;
        private readonly SavedMatchingColumnDataServcies _savedMatchingColumnDataServcies;
        private readonly OzonOrderContext _ozonOrderContext;
        public MatchFileDataController(ExcelParser excelParser,
                                       MatchedResultDataServices matchedResultDataServices,
                                       MatchedRowDataServices matchedRowDataServices,
                                       SavedMatchingColumnDataServcies savedMatchingColumnDataServcies,
                                       OzonOrderContext ozonOrderContext)
        {
            _excelParser = excelParser;
            _matchedResultDataServices = matchedResultDataServices;
            _matchedRowDataServices = matchedRowDataServices;
            _savedMatchingColumnDataServcies = savedMatchingColumnDataServcies;
            _ozonOrderContext = ozonOrderContext;
        }

        public async Task<IActionResult> Index()
        {
            List<MatchedResultInfo> matchedResults = (await _matchedResultDataServices.GetBaseMatchedResults()).Select(mr => new MatchedResultInfo
            {
                Id = mr.Id,
                MainFileName = mr.MainFileName,
                ScondaryFileName = mr.ScondaryFileName,
                СreationDate = mr.СreationDate
            }).ToList();
            return View(new MatchFileDataViewModel() { MatchedResults = matchedResults.OrderByDescending(d => d.СreationDate).ToList(),
                                                       SavedMatchingColumns = await _savedMatchingColumnDataServcies.GetSavedMatchingColumns()
                                                     });
        }



        [HttpPost]
        public async Task<IActionResult> MatchFiles(
            IFormFile file1,
            IFormFile file2,
            List<MatchingColumn> matchingColumns,
            string matchName,
            int startRow1 = 1,
            int startColumn1 = 1,
            int startRow2 = 1,
            int startColumn2 = 1)
        {
            if (file1 == null || file2 == null || matchingColumns == null || !matchingColumns.Any())
            {
                ModelState.AddModelError("", "Необходимо загрузить два файла и указать столбцы для сопоставления.");
                return View();
            }

            if(matchName != null)
            {
                await _savedMatchingColumnDataServcies.AddSavedMatchingColumn(new SavedMatchingColumn() { 
                                                                                                            Name = matchName, 
                                                                                                            MatchingColumns = matchingColumns,
                                                                                                            StartRow1 = startRow1,
                                                                                                            StartColumn1 = startColumn1,
                                                                                                            StartRow2 = startRow2,
                                                                                                            StartColumn2 = startColumn2,
                                                                                                        });
            }

            int matchedResultsId = 0;
            using (var file1Stream = file1.OpenReadStream())
            using (var file2Stream = file2.OpenReadStream())
            {

                List<MatchedRowModel> matchedResults = await _excelParser.MatchExcelDataAsync(file1Stream,
                                                                                              file2Stream,
                                                                                              matchingColumns,
                                                                                              startRow1,
                                                                                              startColumn1,
                                                                                              startRow2,
                                                                                              startColumn2);

                if (!matchedResults.Any())
                {
                    ViewBag.Message = "Совпадений не найдено.";
                }
                else
                {
                    ViewBag.Message = $"{matchedResults.Count} совпадений найдено.";
                }

                if (matchedResults.Count() > 0)
                {
                    var matchedRows = matchedResults.Select(mr => new MatchedRow
                    {
                        File1Data = mr.File1Data,
                        File2Data = mr.File2Data
                    }).ToList();

                    var matchedResult = new MatchedResult
                    {
                        MainFileName = file1.FileName,
                        ScondaryFileName = file2.FileName,
                        MatchedRows = matchedRows,
                        MatchedColumns = matchingColumns,
                        СreationDate = DateTime.Today
                    };

                    var result = await _matchedResultDataServices.AddMatchedResult(matchedResult);

                    matchedResultsId = matchedResult.Id;
                    int matchedCount = matchedResult.MatchedRows.Where(mr => mr.File1Data != null && mr.File2Data != null).Count();
                    if (result > 0)
                    {
                        ViewBag.Message = $"Данные сохранены в базу. {matchedCount} совпадений найдено.";
                    }
                    else
                    {
                        ViewBag.Message = "Ошибка при сохранении данных.";
                    }
                }


                return View("MatchResults", new MatchedRowViewModel() { MatchedResultsId = matchedResultsId,
                                                                        MatchedResults = matchedResults, 
                                                                        MatchingColumns = matchingColumns,
                                                                        MainFileName = file1.FileName, 
                                                                        ScondaryFileName = file2.FileName });;
            }
        }

        public async Task<IActionResult> GetMatchResult(int id)
        {
            MatchedResult matchedResult = await _matchedResultDataServices.GetMatchedResultAsync(id);
            List<MatchedRowModel> matchedRowModel = matchedResult.MatchedRows.Select(mr => new MatchedRowModel
            {
                File1Data = mr.File1Data,
                File2Data = mr.File2Data
            }).ToList();

            return View("MatchResults", new MatchedRowViewModel()
            {
                MatchedResultsId = matchedResult.Id,
                MatchedResults = matchedRowModel,
                MainFileName = matchedResult.MainFileName,
                ScondaryFileName = matchedResult.ScondaryFileName,
                MatchingColumns = matchedResult.MatchedColumns.ToList()
            });;
        }

        public async Task<IActionResult> ExtendMatchedData(int id)
        {
            MatchedResult selectMatchedResult = await _matchedResultDataServices.GetMatchedResultAsync(id);
            return View(new ExtendMatchedDataViewModel()
            {
                Id = selectMatchedResult.Id,
                MainFileName = selectMatchedResult.MainFileName,
                ScondaryFileName = selectMatchedResult.ScondaryFileName
            });
        }

        [HttpPost]
        public async Task<IActionResult> ExtendMatchedData(int id,
                                                            IFormFile file1,
                                                            IFormFile file2,
                                                            List<MatchingColumn> matchingColumns,
                                                            int startRow1 = 1,
                                                            int startColumn1 = 1,
                                                            int startRow2 = 1,
                                                            int startColumn2 = 1)
        {
            // Проверяем, что файлы были загружены
            if (file1 == null || file2 == null || matchingColumns == null || !matchingColumns.Any())
            {
                ModelState.AddModelError("", "Необходимо загрузить два файла и указать столбцы для сопоставления.");
                return View();
            }

            // Открываем потоки файлов
            using (var file1Stream = file1.OpenReadStream())
            using (var file2Stream = file2.OpenReadStream())
            {
                MatchedResult selectMatchedResult = await _matchedResultDataServices.GetMatchedResultAsync(id);
                List<MatchedRowModel> matchedRowModel = selectMatchedResult.MatchedRows.Select(mr => new MatchedRowModel()
                {
                    File1Data = mr.File1Data,
                    File2Data = mr.File2Data,
                }).ToList();

                List<MatchedRowModel> matchedResults = await _excelParser.MatchExcelDataAsync(matchedRowModel,
                                                                                              file1Stream,
                                                                                              file2Stream,
                                                                                              matchingColumns,
                                                                                              startRow1,
                                                                                              startColumn1,
                                                                                              startRow2,
                                                                                              startColumn2);

                if (!matchedResults.Any())
                {
                    ViewBag.Message = "Совпадений не найдено.";
                }
                else
                {
                    ViewBag.Message = $"{matchedResults.Count} совпадений найдено.";
                }

                if (matchedResults.Count() > 0)
                {
                    var matchedRows = matchedResults.Select(mr => new MatchedRow
                    {
                        File1Data = mr.File1Data,
                        File2Data = mr.File2Data
                    }).ToList();

                    var result = await _matchedResultDataServices.UpdateMatchedResult(new MatchedResult()
                                                                                        {
                                                                                           Id = selectMatchedResult.Id,
                                                                                           MainFileName = file1.FileName,
                                                                                           ScondaryFileName = file2.FileName,
                                                                                           MatchedColumns = matchingColumns,
                                                                                           MatchedRows = matchedRows,
                                                                                           СreationDate= DateTime.Today
                                                                                        });

                }


                return View("MatchResults", new MatchedRowViewModel() { MatchedResultsId = id,
                                                                        MatchedResults = matchedResults,
                                                                        MainFileName = file1.FileName, 
                                                                        MatchingColumns = matchingColumns, 
                                                                        ScondaryFileName = file2.FileName });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ProcessMatchedRows([FromBody] MatchedRowsRequestModel request)
        {
            if (request.MatchedRows == null || !request.MatchedRows.Any())
            {
                return BadRequest(new { message = "Нет данных для обработки." });
            }

            MatchedResult matchedResult = await _matchedResultDataServices.GetMatchedResultAsync(request.MatchedResultsId);
            if (matchedResult == null) 
            {
                return BadRequest(new { message = "Не удалось найти данное сопоставление" });
            }

            try
            {
                List<MatchedRow> matchedRows = request.MatchedRows
                .Select(mr => new MatchedRow
                {
                    File1Data = mr.File1Data,
                    File2Data = mr.File2Data,
                })
                .ToList();

                var result = await _matchedResultDataServices.UpdateMatchedResult(new MatchedResult()
                {
                    Id = matchedResult.Id,
                    MainFileName = matchedResult.MainFileName,
                    ScondaryFileName = matchedResult.ScondaryFileName,
                    MatchedColumns = matchedResult.MatchedColumns,
                    MatchedRows = matchedRows,
                    СreationDate = matchedResult.СreationDate
                });
                return Ok(new { message = $"Данные успешно обработаны." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        public async Task<IActionResult> DeleteMatchResult(int id)
        {
            await _matchedResultDataServices.DeleteMatchedResult(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> GetMappingById(int id)
        {
            SavedMatchingColumn savedMatchingColumn = await _savedMatchingColumnDataServcies.GetSavedMatchingColumnAsync(id);

            if (savedMatchingColumn == null)
            {
                return NotFound(); 
            }

            return Json(savedMatchingColumn); 
        }

    }
}
