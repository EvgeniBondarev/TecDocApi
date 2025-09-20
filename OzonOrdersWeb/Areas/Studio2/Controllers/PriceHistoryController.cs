using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OzonRepositories.Data.Bitrix;
using Servcies.DataServcies;

namespace OzonOrdersWeb.Areas.Studio2.Controllers;

[Authorize(Roles = "User,Admin")]
[Area("Studio2")]
 public class PriceHistoryController : Controller
 {
     private readonly IPriceHistoryDataService _priceHistoryService;

     public PriceHistoryController(IPriceHistoryDataService priceHistoryService)
     {
         _priceHistoryService = priceHistoryService;
     }
     
     public async Task<IActionResult> Index()
     {
         try
         {
             var last15Records = await _priceHistoryService.GetLastPriceHistoriesAsync(15);
             return View(last15Records);
         }
         catch (Exception ex)
         {
             ModelState.AddModelError("", $"Ошибка при загрузке данных: {ex.Message}");
             return View(new List<PriceHistory>());
         }
     }
     
     public IActionResult SearchByArticle()
     {
         return View();
     }

     [HttpPost]
     public async Task<IActionResult> SearchByArticle(string article)
     { 
         if (string.IsNullOrEmpty(article))
         {
             ModelState.AddModelError("", "Пожалуйста, введите артикул для поиска");
             return View();
         }

         try
         {
             var results = await _priceHistoryService.GetPriceHistoriesByArticleAsync(article);
             ViewBag.SearchTerm = article;
             return View("SearchResults", results);
         }
         catch (Exception ex)
         {
             ModelState.AddModelError("", $"Ошибка при поиске: {ex.Message}");
             return View();
         }
     }

     [HttpGet]
     public async Task<IActionResult> SearchByBitrixId(int bitrixId)
     {
         try
         {
             var results = await _priceHistoryService.GetPriceHistoriesByBitrixIdAsync(bitrixId);
             return Ok(results);
         }
         catch (Exception ex)
         {
             return StatusCode(500, new { error = $"Ошибка при поиске: {ex.Message}" });
         }
     }
     
     public IActionResult Create()
     {
         return View();
     }

     [HttpPost]
     public async Task<IActionResult> Create(PriceHistory model) 
     {
         if (!ModelState.IsValid)
         {
             if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
             {
                 var errors = ModelState.ToDictionary(
                     k => k.Key,
                     v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                 );
                 return BadRequest(new { errors });
             }
             return View(model);
         }

         try
         {
             if (model.CreateDateTime == default)
             {
                 model.CreateDateTime = DateTime.Now;
             }

             await _priceHistoryService.AddPriceHistory(model);
    
             if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
             {
                 return Ok(new { success = true, message = "Запись успешно создана!" });
             }
        
             TempData["SuccessMessage"] = "Запись успешно создана!";
             return RedirectToAction(nameof(Index));
         }
         catch (Exception ex)
         {
             if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
             {
                 return StatusCode(500, new { error = $"Ошибка при создании записи: {ex.Message}" });
             }
        
             ModelState.AddModelError("", $"Ошибка при создании записи: {ex.Message}");
             return View(model);
         }
     }
     
     public async Task<IActionResult> Details(int id)
     {
         var priceHistory = await _priceHistoryService.GetPriceHistoryAsync(id);
         if (priceHistory == null)
         {
             return NotFound();
         }
         return View(priceHistory);
     }

     public async Task<IActionResult> Edit(int id)
     {
         var priceHistory = await _priceHistoryService.GetPriceHistoryAsync(id);
         if (priceHistory == null)
         {
             return NotFound();
         }
         return View(priceHistory);
     }

     [HttpPost]
     [ValidateAntiForgeryToken]
     public async Task<IActionResult> Edit(int id, PriceHistory model)
     {
         if (id != model.Id)
         {
             return BadRequest();
         }

         if (!ModelState.IsValid)
         {
             return View(model);
         }

         try
         {
             await _priceHistoryService.UpdatePriceHistory(model);
             TempData["SuccessMessage"] = "Запись успешно обновлена!";
             return RedirectToAction(nameof(Details), new { id = model.Id });
         }
         catch (Exception ex)
         {
             ModelState.AddModelError("", $"Ошибка при обновлении записи: {ex.Message}");
             return View(model);
         }
     }

     [HttpPost]
     [ValidateAntiForgeryToken]
     public async Task<IActionResult> Delete(int id)
     {
         try
         {
             var priceHistory = await _priceHistoryService.GetPriceHistoryAsync(id);
             if (priceHistory == null)
             {
                 return NotFound();
             }

             await _priceHistoryService.DeletePriceHistory(priceHistory);
             return Content("ok");
         }
         catch (Exception ex)
         {
             return BadRequest($"Ошибка при удалении записи: {ex.Message}");
         }
     }
}