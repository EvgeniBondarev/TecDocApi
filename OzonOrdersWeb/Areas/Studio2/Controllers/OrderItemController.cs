using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OzonRepositories.Data;
using Servcies.CacheServcies.Cache.OzonOrdersCache;

namespace OzonOrdersWeb.Controllers
{
    [Area("Studio2")]
    public class OrderItemController : Controller
    {
        private readonly OrderItemRepository _orderItemRepository;
        private readonly OrderCache _cache;
        public OrderItemController(OrderItemRepository orderItemRepository,
                                   OrderCache cache)
        {
            _orderItemRepository = orderItemRepository;
            _cache = cache;
        }
        
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _orderItemRepository.GetAsync(id);
            if (item == null) return NotFound();

            return View(item);
        }
        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _orderItemRepository.DeleteAsync(id);
            return RedirectToAction("Index", "Cart"); // Возврат к списку корзин
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SwapStudioOrderIds(string ids, int page)
        {
            if (string.IsNullOrEmpty(ids))
                return BadRequest("ids пустые");

            var idArray = ids.Split(',').Select(int.Parse).ToArray();

            if (idArray.Length != 2)
                return BadRequest("нужно передать ровно 2 id");

            var result = await _orderItemRepository.SwapStudioOrderIdsWithValidationAsync(idArray[0], idArray[1]);

            if (!result)
                return BadRequest("обмен не удался");

            HttpContext.Session.SetString("selectedIds", JsonConvert.SerializeObject(new List<int>()));
            await _cache.Update();

            return Ok(new { success = true });
        }
    }
}