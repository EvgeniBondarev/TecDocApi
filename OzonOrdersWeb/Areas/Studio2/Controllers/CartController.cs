using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OzonDomains.Models.OrderCarts;
using OzonRepositories.Data;
using Servcies.CacheServcies.Cache.CartCache;

namespace  OzonOrdersWeb.Controllers
{
    [Area("Studio2")]
    [Authorize(Roles = "User,Admin")]
    public class CartController : Controller
    {
        private readonly OrderCartRepository _orderCartRepository;
        private readonly CartCache _cartCache;

        public CartController(OrderCartRepository orderCartRepository,
                              CartCache cartCache)
        {
            _orderCartRepository = orderCartRepository;
            _cartCache = cartCache;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var pagedCarts = await _cartCache.Get(page);
            var nextPageData = await _cartCache.Get(page + 1);

            ViewBag.CurrentPage = page;
            ViewBag.HasNextPage = nextPageData.Count == _cartCache.GetIncrementCount();

            int totalItems = await _cartCache.GetTotalCount();
            int itemsPerPage = _cartCache.GetIncrementCount();
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);

            return View(pagedCarts);
        }



        
        [HttpPost]
        public async Task<IActionResult> UpdateCache()
        {
            await _cartCache.Update();
            return RedirectToAction("Index");
        }
        
        public async Task<IActionResult> Details(int id)
        {
            var cart = await _orderCartRepository.GetAsync(id);
            if (cart == null) return NotFound();

            return View(cart);
        }

        public IActionResult Create()
        {
            return View(new OrderCart());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderCart cart)
        {
            if (!ModelState.IsValid)
                return View(cart);

            await _orderCartRepository.Add(cart);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var cart = await _orderCartRepository.GetAsync(id);
            if (cart == null) return NotFound();

            return View(cart);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OrderCart cart)
        {
            if (id != cart.Id) return NotFound();

            if (!ModelState.IsValid)
                return View(cart);

            await _orderCartRepository.Update(cart);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var cart = await _orderCartRepository.GetAsync(id);
            if (cart == null) return NotFound();

            return View(cart);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _orderCartRepository.DeleteAsync(id);
            await _cartCache.Update();
            return RedirectToAction(nameof(Index));
        }
    }
}
