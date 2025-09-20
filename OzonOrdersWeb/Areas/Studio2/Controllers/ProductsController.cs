using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OServcies.FiltersServcies.FilterModels;
using OzonDomains.Models;
using OzonDomains.ViewModels;

using OzonOrdersWeb.Services.Cookies;
using OzonRepositories.Context;
using Servcies.CacheServcies.Cache;
using Servcies.FiltersServcies.DataFilterManagers;
using Services.CacheServcies.Cache;
using Services.CacheServcies.Cache.OzonOrdersCache;


namespace OzonOrdersWeb.Controllers
{
    [Authorize(Roles = "User,Admin")]
    [Area("Studio2")]
    public class ProductsController : Controller
    {
        private readonly OzonOrderContext _context;
        private readonly IWebHostEnvironment _appEnvironment;
        private readonly ProductCache _cache;
        private readonly CookiesManeger _cookies;
        private readonly ProductDataFilterManager _dataFilterManager; 
        private readonly CacheUpdater<Product> _cacheUpdater;

        public ProductsController(OzonOrderContext context, 
                                  IWebHostEnvironment appEnvironment, 
                                  ProductCache cache,
                                  CookiesManeger cookiesManeger,
                                  ProductDataFilterManager productDataFilter,
                                  CacheUpdater<Product> cacheUpdater)
        {
            _context = context;
            _appEnvironment = appEnvironment;
            _cache = cache;
            _cookies = cookiesManeger;
            _dataFilterManager = productDataFilter;
            _cacheUpdater = cacheUpdater;
        }

        // GET: Products
        public async Task<IActionResult> Index(int page = 1)
        {
            if (TempData.ContainsKey("UploadResult") && TempData["UploadResult"] != null)
            {
                int[] result = (int[])TempData["UploadResult"];
                ViewData["UploadResult"] = result;
            }

            var data = await _cache.Get();

            ProductFilterModel filterData = _cookies.GetFromCookies<ProductFilterModel>(Request.Cookies, "ProductFilterState");

            int pageSize = 15;

            var pageViewModel = new PageViewModel<Product, ProductFilterModel>(data, page, pageSize, filterData);
            return View(pageViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index(ProductFilterModel filterData, int page = 1)
        {
            _cookies.SaveToCookies(Response.Cookies, "ProductFilterState", filterData);

            var data = await _cache.Get();

            data = _dataFilterManager.FilterByFilterData(data, filterData);

            int pageSize = 15;
            var pageViewModel = new PageViewModel<Product, ProductFilterModel>(data, page, pageSize, filterData);
            return View(pageViewModel);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Article,OzonProductId,FboOzonSkuId,FbsOzonSkuId,CommercialCategory,Volume,VolumetricWeight,CurrentPriceWithDiscount")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                _cache.Set();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Article,OzonProductId,FboOzonSkuId,FbsOzonSkuId,CommercialCategory,Volume,VolumetricWeight,CurrentPriceWithDiscount")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    _cache.Set();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            _cache.Set();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
