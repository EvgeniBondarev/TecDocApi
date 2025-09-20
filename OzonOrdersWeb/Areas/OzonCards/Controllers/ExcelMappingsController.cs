using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OzonOrdersWeb.Areas.OzonCards.ViewModels;
using Servcies.DataServcies.ExcelMapping;

namespace OzonOrdersWeb.Areas.OzonCards.Controllers
{
    [Area("OzonCards")]
    [Authorize(Roles = "User,Admin")]
    public class ExcelMappingsController : Controller
    {
        private readonly IExcelMappingService _mappingService;

        public ExcelMappingsController(IExcelMappingService mappingService)
        {
            _mappingService = mappingService;
        }

        public async Task<IActionResult> Index()
        {
            var mappings = await _mappingService.GetAllMappingsAsync();
            return View(mappings);
        }

        [HttpPost]
        public async Task<IActionResult> Save(ExcelMappingRequest request, string mappingName)
        {
            try
            {
                var id = await _mappingService.SaveMappingAsync(request, mappingName, User.Identity.Name);
                return Json(new { success = true, id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Load(int id)
        {
            try
            {
                var mapping = await _mappingService.GetMappingAsync(id);
                return Json(new { success = true, data = mapping });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _mappingService.DeleteMappingAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}