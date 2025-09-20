using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OzonDomains;
using OzonDomains.Models;
using OzonOrdersWeb.ViewModels.OzonClientViewModels;
using Servcies.ApiServcies.OzonApi;
using Servcies.ApiServcies.YandexApi;
using Servcies.DataServcies;
using Servcies.FiltersServcies.DataFilterManagers;
using Servcies.FiltersServcies.FilterModels;
using Servcies.FiltersServcies.SortModels;


namespace OzonOrdersWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Studio2")]
    public class OzonClientsController : Controller, ISortLogicContriller<OzonClient, OzonClientSortState>
    {

        private readonly OzonClientServcies _ozonClientServcies;
        private readonly OzonJsonDataBuilder _jsonDataBuilder;
        private readonly OzonClientDataFilterManager _dataFilterManager;
        private readonly YandexDataManager _yandexDataManager;
        public OzonClientsController(OzonClientServcies ozonClientServcies,
                                     OzonJsonDataBuilder jsonDataBuilder,
                                     OzonClientDataFilterManager ozonClientDataFilterManager,
                                     YandexDataManager yandexDataManager)
        {
            _ozonClientServcies = ozonClientServcies;
            _jsonDataBuilder = jsonDataBuilder;
            _dataFilterManager = ozonClientDataFilterManager;
            _yandexDataManager = yandexDataManager;
        }

        // GET: OzonClients
        public async Task<IActionResult> Index(OzonClientSortState sortOrder = OzonClientSortState.StandardState, int page = 1)
        {
            SaveSortStateCookie(sortOrder);

            List<OzonClient> ozonClients = await _ozonClientServcies.GetOzonClients();

            var filterDataString = HttpContext.Request.Cookies["SupplierFilterData"];

            var filterData = new OzonClientFilterModel();
            if (!string.IsNullOrEmpty(filterDataString))
            {
                filterData = JsonConvert.DeserializeObject<OzonClientFilterModel>(filterDataString);
                ozonClients = _dataFilterManager.FilterByFilterData(ozonClients, filterData);
            }

            SetSortOrderViewData(sortOrder);

            ozonClients = (await ApplySortOrder(ozonClients, sortOrder)).ToList();

            foreach (var client in ozonClients)
            {
                client.DecryptApiKey = new string('•', client.DecryptApiKey?.Length ?? 0);
            }

            var ozonClientViewModel = new OzonClientViewModel<OzonClient, OzonClientFilterModel>(ozonClients, page, 15, filterData)
            {
                CurrencyCodes = [(CurrencyCode.RUB, "RUB"),
                    (CurrencyCode.USD, "USD"),
                    (CurrencyCode.EUR, "EUR"),
                    (CurrencyCode.BYN, "BYN")],
            };

            return View(ozonClientViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index(OzonClientFilterModel filterData, int page = 1)
        {
            List<OzonClient> ozonClients = await _ozonClientServcies.GetOzonClients();
            ozonClients = _dataFilterManager.FilterByFilterData(ozonClients, filterData);

            var serializedFilterData = JsonConvert.SerializeObject(filterData);
            HttpContext.Response.Cookies.Append("SupplierFilterData", serializedFilterData);

            var ozonClientViewModel = new OzonClientViewModel<OzonClient, OzonClientFilterModel>(ozonClients, page, 15, filterData)
            {
                CurrencyCodes = [(CurrencyCode.RUB, "RUB"),
                    (CurrencyCode.USD, "USD"),
                    (CurrencyCode.EUR, "EUR"),
                    (CurrencyCode.BYN, "BYN")],
            };

            return View(ozonClientViewModel);
        }
        
        public async Task<IActionResult> Details(int id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var ozonClient = await _ozonClientServcies.GetOzonClientAsync(id);
            ozonClient.DecryptApiKey = new string('•', ozonClient.DecryptApiKey?.Length ?? 0);
            if (ozonClient == null)
            {
                return NotFound();
            }

            return View(ozonClient);
        }

        // GET: OzonClients/Create
        public async Task<IActionResult> Create()
        {
            if (TempData.ContainsKey("CreateInfo") && TempData["CreateInfo"] != null)
            {
                string result = (string)TempData["CreateInfo"];
                ViewData["CreateInfo"] = result;
            }

            var viewModel = new CreateOzonClientViewModel()
            {
                CurrencyCodes = new List<SelectListItem>
                                {
                                    new SelectListItem { Value = CurrencyCode.RUB.ToString(), Text = "RUB" },
                                    new SelectListItem { Value = CurrencyCode.USD.ToString(), Text = "USD" },
                                    new SelectListItem { Value = CurrencyCode.EUR.ToString(), Text = "EUR" },
                                    new SelectListItem { Value = CurrencyCode.BYN.ToString(), Text = "BYN" },
                                },
                ClientTypes = new List<SelectListItem>
                                {
                                    new SelectListItem {Value = ClientType.ALL.ToString(), Text = "Другое"},
                                    new SelectListItem {Value = ClientType.OZON.ToString(), Text = "Ozon"},
                                    new SelectListItem {Value = ClientType.YANDEX.ToString(), Text = "Yandex"}
                                },
                YandexClients = await _yandexDataManager.GetAccountCampaigns(),
                YandexApiKey = await _yandexDataManager.GetApiKey(),
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ClientId,ApiKey,CurrencyCode,ClientType")] OzonClient ozonClient)
        {

            if (await _ozonClientServcies.GetOzonClientAsync(ozonClient) == null)
            {
                if (ozonClient.ClientId != null && ozonClient.ApiKey != null)
                {
                    var clients = await _ozonClientServcies.GetOzonClients();
                    if (ozonClient.ClientType == ClientType.OZON)
                    {

                        if (!clients.Where(c => c.DecryptApiKey == ozonClient.ApiKey).Any())
                        {
                            if (await _jsonDataBuilder.GetTestReques(ozonClient.ClientId, ozonClient.ApiKey))
                            {
                                await _ozonClientServcies.AddOzonClient(ozonClient);
                                return RedirectToAction(nameof(Index));
                            }
                            else
                            {
                                TempData["CreateInfo"] = $"Api данные некорректны. Ошибка при отправке тестового запроса " +
                                                         $"(ClientId: `{ozonClient.ClientId}`, Api Key: `{ozonClient.ApiKey}`)";
                                return RedirectToAction(nameof(Create));
                            }
                        }
                        else
                        {
                            TempData["CreateInfo"] = $"Api Key `{ozonClient.ApiKey}` уже существует.";
                            return RedirectToAction(nameof(Create));
                        }
                    }
                    else if (ozonClient.ClientType == ClientType.YANDEX)
                    {
                        if (!clients.Where(c => c.DecryptClientId == ozonClient.ClientId).Any())
                        {
                            if (await _yandexDataManager.GetTestRequest(ozonClient.ClientId, ozonClient.ApiKey))
                            {
                                await _ozonClientServcies.AddOzonClient(ozonClient);
                                return RedirectToAction(nameof(Index));
                            }
                            else
                            {
                                TempData["CreateInfo"] = $"Api данные некорректны. Ошибка при отправке тестового запроса " +
                                                         $"(ClientId: `{ozonClient.ClientId}`, Api Key: `{ozonClient.ApiKey}`)";
                                return RedirectToAction(nameof(Create));
                            }
                        }
                        else
                        {
                            TempData["CreateInfo"] = $"Api Key `{ozonClient.ClientId}` уже существует.";
                            return RedirectToAction(nameof(Create));
                        }
                    }
                    else
                    {
                        await _ozonClientServcies.AddOzonClient(ozonClient);
                        return RedirectToAction(nameof(Index));
                    }

                }
                else
                {
                    await _ozonClientServcies.AddOzonClient(ozonClient);
                    return RedirectToAction(nameof(Index));
                }

            }
            else
            {
                TempData["CreateInfo"] = $"Клиент `{ozonClient.Name}` уже существует.";
                return RedirectToAction(nameof(Create));
            }
        }

        public async Task<IActionResult> Edit(int id, string info = null)
        {
            if (TempData.ContainsKey("EditInfo") && TempData["EditInfo"] != null)
            {
                string result = (string)TempData["EditInfo"];
                ViewData["EditInfo"] = result;
            }

            if (info != null)
            {
                ViewData["EditInfo"] = info;
            }

            var ozonClient = await _ozonClientServcies.GetOzonClientAsync(id);
            if (ozonClient == null)
            {
                return NotFound();
            }

            // Проверка ролей API для клиента Ozon
            List<Role> apiRoles = new List<Role>();
            bool hasApiAccess = false;
    
            if (ozonClient.ClientType == ClientType.OZON && 
                !string.IsNullOrEmpty(ozonClient.DecryptClientId) && 
                !string.IsNullOrEmpty(ozonClient.DecryptApiKey))
            {
                try
                {
                    var ozonApiManager = new OzonApiDataManager(ozonClient.DecryptClientId, ozonClient.DecryptApiKey);
                    var rolesResponse = await ozonApiManager.GetApiRoles();
            
                    if (rolesResponse?.Roles != null)
                    {
                        apiRoles = rolesResponse.Roles;
                        hasApiAccess = true;
                    }
                }
                catch (Exception ex)
                {
                    apiRoles.Add(new Role 
                    { 
                        Name = "Ошибка", 
                        Methods = new List<string> { $"Ошибка при проверке прав: {ex.Message}" } 
                    });
                }
            }

            var viewModel = new EditOzonClientViewModel
            {
                OzonClient = ozonClient,
                CurrencyCodes = new List<SelectListItem>
                                {
                                    new SelectListItem { Value = CurrencyCode.RUB.ToString(), Text = "RUB" },
                                    new SelectListItem { Value = CurrencyCode.USD.ToString(), Text = "USD" },
                                    new SelectListItem { Value = CurrencyCode.EUR.ToString(), Text = "EUR" },
                                    new SelectListItem { Value = CurrencyCode.BYN.ToString(), Text = "BYN" },
                                },

                ClientTypes = new List<SelectListItem>
                                {
                                    new SelectListItem {Value = ClientType.ALL.ToString(), Text = "Другое"},
                                    new SelectListItem {Value = ClientType.OZON.ToString(), Text = "Ozon"},
                                    new SelectListItem {Value = ClientType.YANDEX.ToString(), Text = "Yandex"}
                                },
                ApiRoles = apiRoles,
                HasApiAccess = hasApiAccess
            };

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OzonClient ozonClient)
        {
            if (id != ozonClient.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                        if (ozonClient.ClientId != null && ozonClient.ApiKey != null)
                        {
                            if (await _jsonDataBuilder.GetTestReques(ozonClient.ClientId, ozonClient.ApiKey))
                            {
                                await _ozonClientServcies.Update(ozonClient);
                                return RedirectToAction(nameof(Index));
                            }
                            else
                            {
                                return RedirectToAction(nameof(Edit), new
                                {
                                    id = id,
                                    info = $"Api данные некорректны. Ошибка при отправке тестового запроса " +
                                                        $"(ClientId: `{ozonClient.ClientId}`, Api Key: `{ozonClient.ApiKey}`)"
                                });
                            }
                        }
                        else
                        {
                            await _ozonClientServcies.Update(ozonClient);
                            return RedirectToAction(nameof(Index));
                        }
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Edit), new { id });
        }


        // GET: OzonClients/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ozonClient = await _ozonClientServcies.GetOzonClientAsync(id);
            ozonClient.DecryptApiKey = new string('•', ozonClient.DecryptApiKey?.Length ?? 0);
            if (ozonClient == null)
            {
                return NotFound();
            }

            return View(ozonClient);
        }

        // POST: OzonClients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ozonClient = await _ozonClientServcies.GetOzonClientAsync(id);
            if (ozonClient != null)
            {
                await _ozonClientServcies.DeleteOzonClient(ozonClient);
            }
            return RedirectToAction(nameof(Index));
        }

        public void SetSortOrderViewData(OzonClientSortState ozonClientSortState)
        {
            ViewData["OzonClientNameSort"] = ozonClientSortState == OzonClientSortState.NameAsc ? OzonClientSortState.NameDesc : OzonClientSortState.NameAsc;
            ViewData["OzonClientCurrencyCodeSort"] = ozonClientSortState == OzonClientSortState.CurrencyCodeAsc ? OzonClientSortState.CurrencyCodeDesc : OzonClientSortState.CurrencyCodeAsc;
        }

        public async Task<IEnumerable<OzonClient>> ApplySortOrder(IEnumerable<OzonClient> ozonClients, OzonClientSortState ozonClientSort)
        {
            return ozonClientSort switch
            {
                OzonClientSortState.NameAsc => ozonClients.OrderBy(o => o.Name),
                OzonClientSortState.NameDesc => ozonClients.OrderByDescending(o => o.Name),
                OzonClientSortState.CurrencyCodeAsc => ozonClients.OrderBy(o => o.CurrencyCode),
                OzonClientSortState.CurrencyCodeDesc => ozonClients.OrderByDescending(o => o.CurrencyCode),
                _ => ozonClients
            };
        }

        public void SaveSortStateCookie(OzonClientSortState ozonClientSort)
        {
            if (ozonClientSort != OzonClientSortState.StandardState)
            {
                Response.Cookies.Delete("OzonClientSortState");
                Response.Cookies.Append("OzonClientSortState", ozonClientSort.ToString());
            }
        }

        [HttpPost]
        public async Task<IActionResult> DelSortStateCookie()
        {
            Response.Cookies.Delete("OzonClientSortState");
            Response.Cookies.Append("OzonClientSortState", OzonClientSortState.StandardState.ToString());
            return RedirectToAction("Index");
        }

    }
}