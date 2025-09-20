using System.Data.Entity.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OzonDomains.Models;
using OzonDomains.ViewModels;
using Servcies.DataServcies;
using Servcies.FiltersServcies.DataFilterManagers;
using Servcies.FiltersServcies.FilterModels;
using Servcies.FiltersServcies.SortModels;
using Services.CacheServcies.Cache.EtProducerCache;

namespace OzonOrdersWeb.Controllers;

[Authorize(Roles = "Admin")]
[Area("Studio2")]
public class ProducersController : Controller, ISortLogicContriller<EtProducer, EtProducerSortState>
{
        private readonly EtProducerDataServices _producerDataService;
        private readonly EtProducerFilterManager _dataFilterManager;
        private readonly EtProducerCache _producerCache;

        public ProducersController(EtProducerDataServices producerDataService, 
                                    EtProducerFilterManager producerFilterManager,
                                    EtProducerCache producerCache)
        {
            _producerDataService = producerDataService;
            _dataFilterManager = producerFilterManager;
            _producerCache = producerCache;
        }

        public async Task<IActionResult> Index(EtProducerSortState sortOrder = EtProducerSortState.StandardState, int page = 1)
        {
            SaveSortStateCookie(sortOrder);

            List<EtProducer> producers = await _producerCache.Get();

            var filterDataString = HttpContext.Request.Cookies["ProducerFilterData"];
            var filterData = new EtProducerFilterModel();

            if (!string.IsNullOrEmpty(filterDataString))
            {
                filterData = JsonConvert.DeserializeObject<EtProducerFilterModel>(filterDataString);
                producers = await _dataFilterManager.FilterByFilterDataAsync(filterData);
            }

            SetSortOrderViewData(sortOrder);
            producers = (await ApplySortOrder(producers, sortOrder)).ToList();

            return View(new PageViewModel<EtProducer, EtProducerFilterModel>(producers, page, 15, filterData));
        }

        [HttpPost]
        public async Task<IActionResult> Index(EtProducerFilterModel filterData, int page = 1)
        {
            List<EtProducer> producers =  await _producerCache.Get();
            producers = await _dataFilterManager.FilterByFilterDataAsync(filterData);

            var serializedFilterData = JsonConvert.SerializeObject(filterData);
            HttpContext.Response.Cookies.Append("ProducerFilterData", serializedFilterData);

            return View(new PageViewModel<EtProducer, EtProducerFilterModel>(producers, page, 15, filterData));
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,RealId,Prefix,Name,Address,Www,Rating,ExistName,ExistId,Domain,TecdocSupplierId,MarketPrefix")] EtProducer producer)
        {
            EtProducer isOrig = (await _producerDataService.GetEtProducers()).FirstOrDefault(p => p.Name?.ToLower() == producer?.Name?.ToLower() &&
                                                                                                p.Prefix?.ToLower() == producer?.Prefix?.ToLower());
            if (isOrig == null)
            {
                if (ModelState.IsValid)
                {
                    _producerDataService.AddEtProducer(producer);
                    return RedirectToAction(nameof(Index));
                }
                return View(producer);
            }
            else
            {
                throw new Exception("Producer already exists");
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var producer = await _producerDataService.GetEtProducerAsync(int.Parse(id));
            if (producer == null)
            {
                return NotFound();
            }
            return View(producer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EtProducer producer, int page = 1)
        {
            if (int.Parse(id) != producer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                EtProducer isOrig = (await _producerDataService.GetEtProducers()).FirstOrDefault(p => p.Name?.ToLower() == producer?.Name?.ToLower() &&
                                                                                                   p.Prefix?.ToLower() == producer?.Prefix?.ToLower());

                try
                {
                    if (isOrig == null || isOrig.Id == producer.Id)
                    {
                        await _producerDataService.UpdateEtProducer(producer);
                        await _producerCache.Update();
                    }
                    else
                    {
                        throw new Exception("Duplicate producer data");
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (await _producerDataService.GetEtProducerAsync(int.Parse(id)) == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { sortOrder = GetSortStateCookie(), page = page });
            }
            return View(producer);
        }

        public void SetSortOrderViewData(EtProducerSortState producerSort)
        {
            ViewData["ProducerNameSort"] = producerSort == EtProducerSortState.NameAsc ? EtProducerSortState.NameDesc : EtProducerSortState.NameAsc;
            ViewData["MarketPrefixSort"] = producerSort == EtProducerSortState.MarketPrefixAsc ? EtProducerSortState.MarketPrefixDesc : EtProducerSortState.MarketPrefixAsc;
            ViewData["ProducerRatingSort"] = producerSort == EtProducerSortState.RatingAsc ? EtProducerSortState.RatingDesc : EtProducerSortState.RatingAsc;
            ViewData["PrefixSort"] = producerSort == EtProducerSortState.PrefixAsc ? EtProducerSortState.PrefixDesc : EtProducerSortState.PrefixAsc;
            ViewData["AddressSort"] = producerSort == EtProducerSortState.AddressAsc ? EtProducerSortState.AddressDesc : EtProducerSortState.AddressAsc;
            ViewData["WwwSort"] = producerSort == EtProducerSortState.WwwAsc ? EtProducerSortState.WwwDesc : EtProducerSortState.WwwAsc;
            ViewData["ExistNameSort"] = producerSort == EtProducerSortState.ExistNameAsc ? EtProducerSortState.ExistNameDesc : EtProducerSortState.ExistNameAsc;
            ViewData["DomainSort"] = producerSort == EtProducerSortState.DomainAsc ? EtProducerSortState.DomainDesc : EtProducerSortState.DomainAsc;
            ViewData["IdSort"] = producerSort == EtProducerSortState.IdAsc ? EtProducerSortState.IdDesc : EtProducerSortState.IdAsc;
            ViewData["RealIdSort"] = producerSort == EtProducerSortState.RealIdAsc ? EtProducerSortState.RealIdDesc : EtProducerSortState.RealIdAsc;
        }

        public async Task<IEnumerable<EtProducer>> ApplySortOrder(IEnumerable<EtProducer> producers, EtProducerSortState producerSort)
        {
            return producerSort switch
            {
                EtProducerSortState.NameAsc => producers.OrderBy(p => p.Name),
                EtProducerSortState.NameDesc => producers.OrderByDescending(p => p.Name),

                EtProducerSortState.MarketPrefixAsc => producers.OrderBy(p => p.MarketPrefix),
                EtProducerSortState.MarketPrefixDesc => producers.OrderByDescending(p => p.MarketPrefix),
                
                EtProducerSortState.RatingAsc => producers.OrderBy(p => p.Rating),
                EtProducerSortState.RatingDesc => producers.OrderByDescending(p => p.Rating),
                
                EtProducerSortState.PrefixAsc => producers.OrderBy(p => p.Prefix),
                EtProducerSortState.PrefixDesc => producers.OrderByDescending(p => p.Prefix),
                
                EtProducerSortState.AddressAsc => producers.OrderBy(p => p.Address),
                EtProducerSortState.AddressDesc => producers.OrderByDescending(p => p.Address),
                
                EtProducerSortState.WwwAsc => producers.OrderBy(p => p.Www),
                EtProducerSortState.WwwDesc => producers.OrderByDescending(p => p.Www),
                
                EtProducerSortState.ExistNameAsc => producers.OrderBy(p => p.ExistName),
                EtProducerSortState.ExistNameDesc => producers.OrderByDescending(p => p.ExistName),
                
                EtProducerSortState.DomainAsc => producers.OrderBy(p => p.Domain),
                EtProducerSortState.DomainDesc => producers.OrderByDescending(p => p.Domain),
                
                EtProducerSortState.IdAsc => producers.OrderBy(p => p.Id),
                EtProducerSortState.IdDesc => producers.OrderByDescending(p => p.Id),
                
                EtProducerSortState.RealIdAsc => producers.OrderBy(p => p.RealId),
                EtProducerSortState.RealIdDesc => producers.OrderByDescending(p => p.RealId),
                
                _ => producers
            };
        }

        private EtProducerSortState GetSortStateCookie()
        {
            var sortStateCookie = Request.Cookies["ProducerSortState"];
            if (!string.IsNullOrEmpty(sortStateCookie) && Enum.TryParse<EtProducerSortState>(sortStateCookie, out var savedSortState))
            {
                return savedSortState;
            }
            return EtProducerSortState.StandardState;
        }

        public void SaveSortStateCookie(EtProducerSortState producerSort)
        {
            if (producerSort != EtProducerSortState.StandardState)
            {
                Response.Cookies.Delete("ProducerSortState");
                Response.Cookies.Append("ProducerSortState", producerSort.ToString());
            }
        }

        [HttpPost]
        public async Task<IActionResult> DelSortStateCookie()
        {
            Response.Cookies.Delete("ProducerSortState");
            Response.Cookies.Append("ProducerSortState", EtProducerSortState.StandardState.ToString());
            return RedirectToAction("Index");
        }
    }