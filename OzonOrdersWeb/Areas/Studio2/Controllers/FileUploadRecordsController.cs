using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OzonOrdersWeb.Areas.Studio2.ViewModels.FileUploadRecordViewModels;
using Servcies.DataServcies;

namespace OzonOrdersWeb.Areas.Studio2.Controllers;

using Microsoft.AspNetCore.Mvc;
using OzonDomains.Models;
using Servcies.DataServcies;
using System;
using System.Threading.Tasks;

[Authorize(Roles = "User,Admin")]
[Area("Studio2")]
public class FileUploadRecordsController : Controller
{
    private readonly FileUploadRecordDataService _dataService;
    private const int DefaultPageSize = 15;

    public FileUploadRecordsController(FileUploadRecordDataService dataService)
    {
        _dataService = dataService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        int pageNumber = 1,
        int pageSize = DefaultPageSize,
        string nameFilter = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
    {
        // Получаем все записи
        var allRecords = await _dataService.GetFileUploadRecords();

        // Применяем фильтры
        if (!string.IsNullOrEmpty(nameFilter))
        {
            allRecords = allRecords.Where(x =>
                    x.FileName.Contains(nameFilter, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (dateFrom.HasValue)
        {
            allRecords = allRecords.Where(x => x.Date >= dateFrom.Value)
                .ToList();
        }

        if (dateTo.HasValue)
        {
            allRecords = allRecords.Where(x => x.Date <= dateTo.Value)
                .ToList();
        }

        // Сортировка от новых к старым
        allRecords = allRecords
            .OrderByDescending(x => x.Date)
            .ToList();

        // Пагинация
        var totalCount = allRecords.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var pagedRecords = allRecords
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // ViewModel
        var viewModel = new FileUploadRecordViewModel
        {
            Items = pagedRecords,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalCount = totalCount,
            NameFilter = nameFilter,
            DateFromFilter = dateFrom,
            DateToFilter = dateTo
        };

        return View(viewModel);
    }
}