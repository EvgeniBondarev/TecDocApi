using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OzonDomains.Models;
using OzonOrdersWeb.Areas.Studio2.ViewModels.ExcludedArticles;
using Servcies.DataServcies;

namespace OzonWebApp.Controllers
{
    [Area("Studio2")]
    [Authorize(Roles = "User,Admin")]
    public class ExcludedArticlesController : Controller
    {
        private readonly ExcludedArticleDataServcies _excludedArticleService;
        private readonly OzonClientServcies _ozonClientService;
        private readonly OrdersDataServcies _ordersService;

        public ExcludedArticlesController(
            ExcludedArticleDataServcies excludedArticleService,
            OzonClientServcies ozonClientService,
            OrdersDataServcies ordersService)
        {
            _excludedArticleService = excludedArticleService;
            _ozonClientService = ozonClientService;
            _ordersService = ordersService;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 20;
            var (articles, totalCount) = await _excludedArticleService.GetExcludedArticlesPaged(page, pageSize);

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;

            return View(articles);
        }


        [HttpGet]
        public async Task<IActionResult> Create(int orderId)
        {
            var clients = await _ozonClientService.GetOzonClients();
            ViewBag.OzonClients = clients ?? new List<OzonClient>();

            if (orderId != 0)
            {
                var order = await _ordersService.GetOrder(orderId);
                return View(new ExcludedArticleViewModel
                {
                    Article = order.Article ?? string.Empty,
                    OzonClientId = order.OzonClient?.Id ?? 0
                });
            }
            return View(new ExcludedArticleViewModel());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExcludedArticleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var article = new ExcludedArticle
                {
                    Article = model.Article,
                    OzonClientId = model.OzonClientId
                };

                await _excludedArticleService.AddExcludedArticle(article);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.OzonClients = await _ozonClientService.GetOzonClients();
            return View(model);
        }
        
        public async Task<IActionResult> Edit(int id)
        {
            var article = await _excludedArticleService.GetExcludedArticleAsync(id);
            if (article == null) return NotFound();

            ViewBag.OzonClients = await _ozonClientService.GetOzonClients();
            return View(article);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ExcludedArticle article)
        {
            if (id != article.Id) return NotFound();

            if (ModelState.IsValid)
            {
                await _excludedArticleService.UpdateExcludedArticle(article);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.OzonClients = await _ozonClientService.GetOzonClients();
            return View(article);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var article = await _excludedArticleService.GetExcludedArticleAsync(id);
            if (article == null) return NotFound();

            return View(article);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var article = await _excludedArticleService.GetExcludedArticleAsync(id);
            if (article != null)
            {
                await _excludedArticleService.DeleteExcludedArticle(article);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
