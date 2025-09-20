using ConsoleApp1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using TreeGrouping.Application.CategoryService;
using TreeGrouping.Application.DbService;
using TreeGrouping.Application.DbService.Models;
using TreeGrouping.Web.Models;

namespace TreeGrouping.Web.Controllers;

[Area("CategoryTree")]
public class CategoryController : Controller
{
    private readonly IDatabaseService _dbService;
    private readonly CategoryCacheService _cacheService;
    private readonly CategoryFilterService _filterService;
    private readonly CategoryTreeService _treeService;

    public CategoryController(
        IDatabaseService dbService,
        CategoryCacheService cacheService,
        CategoryFilterService filterService,
        CategoryTreeService treeService)
    {
        _dbService = dbService;
        _cacheService = cacheService;
        _filterService = filterService;
        _treeService = treeService;
    }

    public async Task<IActionResult> Index()
    {
        return View();
    }
    
    private async Task<List<CategoryModel>> GetCategoriesWithCache(string cacheKey, StoredProcedureType storedProcedureType, string name = null)
    {
        var categories = await _cacheService.GetCategoriesFromCache(cacheKey, storedProcedureType);
        if (!string.IsNullOrEmpty(name))
        {
            categories = _filterService.FilterCategoriesByName(categories, name);
        }
        return _filterService.OrderCategoriesByName(categories);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetVolnaCategories(string name = null)
    {
        var categories = await GetCategoriesWithCache("volna_categories", StoredProcedureType.GetVolnaCategories, name);
        var tree = _treeService.BuildTree(categories);
        var model = Tuple.Create(tree, "Volna");

        return PartialView("_CategoryTree", model);
    }

    [HttpGet]
    public async Task<IActionResult> GetVolnaCategoriesById(int id)
    {
        var categories = await _dbService.ExecuteStoredProcedureAsync(StoredProcedureType.SearchVolnaCategoryById, id);
        foreach (var category in categories)
        {
            category.IsFiltred = true;
        }
        var orderedCategories = _filterService.OrderCategoriesByName(categories.ToList());
        var tree = _treeService.BuildTree(orderedCategories);
        var model = Tuple.Create(tree, "Volna");

        return PartialView("_CategoryTree", model);
    }

    [HttpGet]
    public async Task<IActionResult> UpdateVolnaCategories()
    {
        var categories = await _cacheService.UpdateCache("volna_categories", StoredProcedureType.GetVolnaCategories);
        var orderedCategories = _filterService.OrderCategoriesByName(categories);
        var tree = _treeService.BuildTree(orderedCategories);
        var model = Tuple.Create(tree, "Volna");

        return PartialView("_CategoryTree", model);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetOzonCategories(string name = null)
    {
        var categories = await GetCategoriesWithCache("ozon_categories", StoredProcedureType.GetOzonCategories, name);
        var tree = _treeService.BuildTree(categories);
        var model = Tuple.Create(tree, "Ozon");

        return PartialView("_CategoryTree", model);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetOzonCategoriesById(int id)
    {
        var categories = await _dbService.ExecuteStoredProcedureAsync(StoredProcedureType.SearchOzonCategoryById, id);
        foreach (var category in categories)
        {
            category.IsFiltred = true;
        }
        var orderedCategories = _filterService.OrderCategoriesByName(categories.ToList());
        var tree = _treeService.BuildTree(orderedCategories);
        var model = Tuple.Create(tree, "Ozon");

        return PartialView("_CategoryTree", model);
    }
    
    [HttpGet]
    public async Task<IActionResult> UpdateOzonCategories()
    {
        var categories = await _cacheService.UpdateCache("ozon_categories", StoredProcedureType.GetOzonCategories);
        var orderedCategories = _filterService.OrderCategoriesByName(categories);
        var tree = _treeService.BuildTree(orderedCategories);
        var model = Tuple.Create(tree, "Ozon");

        return PartialView("_CategoryTree", model);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetCtCategories(string name = null)
    {
        var categories = await _cacheService.GetCategoriesFromCache("ct_categories", StoredProcedureType.GetCtCategories);
        var links = await _dbService.GetAllCategoryLinksAsync();
        var newCategories = await _treeService.CategoryLinkToModel(_dbService, categories, links.ToList());
        newCategories = _filterService.FilterCategoriesByName(newCategories, name);
        var orderedCategories = _filterService.OrderCategoriesByName(newCategories);
        var tree = _treeService.BuildTree(orderedCategories);
        var model = Tuple.Create(tree, "CT");

        return PartialView("_CategoryTree", model);
    }
    
    [HttpGet]
    public async Task<IActionResult> UpdateCtCategories()
    {
        var categories = await _cacheService.UpdateCache("ct_categories", StoredProcedureType.GetCtCategories);
        var links = await _dbService.GetAllCategoryLinksAsync();
        var newCategories = await _treeService.CategoryLinkToModel(_dbService, categories, links.ToList());
        var orderedCategories = _filterService.OrderCategoriesByName(newCategories);
        var tree = _treeService.BuildTree(orderedCategories);
        var model = Tuple.Create(tree, "CT");

        return PartialView("_CategoryTree", model);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetCtCategoriesById(int id)
    {
        var categories = await _dbService.ExecuteStoredProcedureAsync(StoredProcedureType.SearchCtCategoryById, id);
        var orderedCategories = _filterService.OrderCategoriesByName(categories.ToList());
        var tree = _treeService.BuildTree(orderedCategories);
        var model = Tuple.Create(tree, "CT");

        return PartialView("_CategoryTree", model);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetCatTreeCategories(string name = null)
    {
        var categories = await GetCategoriesWithCache("cat_tree_categories", StoredProcedureType.GetCatTreeCategories, name);
        var tree = _treeService.BuildTree(categories);
        var model = Tuple.Create(tree, "CatTree");
        return PartialView("_CategoryTree", model);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetCatTreeCategoriesById(int id)
    {
        var categories = await _dbService.ExecuteStoredProcedureAsync(StoredProcedureType.SearchCatTreeCategoryById, id);
        var orderedCategories = _filterService.OrderCategoriesByName(categories.ToList());
        var tree = _treeService.BuildTree(orderedCategories);
        var model = Tuple.Create(tree, "CatTree");

        return PartialView("_CategoryTree", model);
    }

    [HttpGet]
    public async Task<IActionResult> UpdateCatTreeCategories()
    {
        var categories = await _cacheService.UpdateCache("cat_tree_categories", StoredProcedureType.GetCatTreeCategories);
        var orderedCategories = _filterService.OrderCategoriesByName(categories);
        var tree = _treeService.BuildTree(orderedCategories);
        var model = Tuple.Create(tree, "CatTree");

        return PartialView("_CategoryTree", model);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetICGroupsCategories(string name = null)
    {
        var categories = await GetCategoriesWithCache("ic_groups", StoredProcedureType.GetICGroups, name);
        var tree = _treeService.BuildTree(categories);
        var model = Tuple.Create(tree, "IC");

        return PartialView("_CategoryTree", model);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetICGroupsCategoriesById(int id)
    {
        var categories = await _dbService.ExecuteStoredProcedureAsync(StoredProcedureType.SearchICGroupById, id);
        var orderedCategories = _filterService.OrderCategoriesByName(categories.ToList());
        var tree = _treeService.BuildTree(orderedCategories);
        var model = Tuple.Create(tree, "IC");

        return PartialView("_CategoryTree", model);
    }
    
    [HttpGet]
    public async Task<IActionResult> UpdateICGroupsCategories()
    {
        var categories = await _cacheService.UpdateCache("ic_groups", StoredProcedureType.GetICGroups);
        var orderedCategories = _filterService.OrderCategoriesByName(categories);
        var tree = _treeService.BuildTree(orderedCategories);
        var model = Tuple.Create(tree, "IC");

        return PartialView("_CategoryTree", model);
    }

    // Остальные методы остаются без изменений
    [HttpPost]
    public async Task<IActionResult> LinkCategories([FromBody] Dictionary<string, int> selectedCategories)
    {
        try
        {
            if (selectedCategories == null || selectedCategories.Count <= 1 || !selectedCategories.ContainsKey("CT"))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Ошибка: Должен быть выбран элемент с ключом 'CT' и должно быть более одной категории."
                });
            }

            var ctValue = selectedCategories["CT"];
            
            var categoryPairs = new List<Dictionary<string, int>>();
            
            foreach (var key in selectedCategories.Keys.Where(k => k != "CT"))
            {
                var pair = new Dictionary<string, int>
                {
                    { "CT", ctValue },
                    { key, selectedCategories[key] }
                };

                categoryPairs.Add(pair);
            }
            foreach (var pair in categoryPairs)
            {
                int ct = pair.ContainsKey("CT") ? pair["CT"] : 0;
                
                var secondKey = pair.Keys.FirstOrDefault(k => k != "CT");
                var secondValue = secondKey != null ? pair[secondKey] : 0;
                await _dbService.ExecuteStoredProcedureAsync(
                    StoredProcedureType.AddCategoryLink,
                    (ct, secondValue, secondKey)
                );
            }
            return Ok(new { success = true, message = "Категории успешно связаны!", data = categoryPairs });
        }
        catch (Exception ex)
        {
            return BadRequest(new
                { success = false, message = "Ошибка при связывании категорий!", error = ex.Message });
        }
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken] 
    public async Task<IActionResult> Unlink(int id)
    {
        await _dbService.ExecuteStoredProcedureAsync(StoredProcedureType.DeleteCategoryLink, id);
        return Ok();
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateUnifiedCategory([FromBody] Dictionary<string, string> categoryNames)
    {
        int? ozonId = TryGetIntValue(categoryNames, "Ozon");
        int? volnaId = TryGetIntValue(categoryNames, "Volna");
        int? icId = TryGetIntValue(categoryNames, "IC");
        await _dbService.ExecuteStoredProcedureAsync(StoredProcedureType.InsertUnifiedCategoryIfNotExists, (ozonId, volnaId, icId));

        return Ok(new { ozonId, volnaId, icId });
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategoryTranslation([FromBody] TranslationRequest request)
    {
        try
        {
            var results = new List<object>();
        
            foreach (var translation in request.CategoryNames)
            {
                var result = await _dbService.ExecuteStoredProcedureAsync(
                    StoredProcedureType.AddCategoryTranslations, 
                    (translation.Value, // category_id
                        translation.Key,             // link_type_name
                        request.Translation));
            
                results.Add(new {
                    CategoryId = translation.Value,
                    LinkType = translation.Key,
                    Result = result
                });
            }

            return Ok(new {
                Success = true,
                Results = results
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new {
                Success = false,
                Error = ex.Message
            });
        }
    }

    private int? TryGetIntValue(Dictionary<string, string> dict, string key)
    {
        if (dict.TryGetValue(key, out var value) && int.TryParse(value, out var result))
            return result;

        return null;
    }
}