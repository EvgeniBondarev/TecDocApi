using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using OzonOrdersWeb.Areas.PartsInfo.ModelBuilders;
using OzonOrdersWeb.Areas.PartsInfo.Models.ABCP;
using PartsInfo.HttpUtils;

namespace OzonOrdersWeb.Areas.PartsInfo.Controllers;

[Area("PartsInfo")]
public class AbcpController : Controller
{
    private readonly ProxyHttpClientService _proxyHttpClientService;
    private readonly TipsModelBuilder _tipsModelBuilder;
    private readonly ArticleSearchModelBuilder  _articleSearchModelBuilder;

    public AbcpController(
        ProxyHttpClientService proxyHttpClientService,
        TipsModelBuilder tipsModelBuilder,
        ArticleSearchModelBuilder articleSearchModelBuilder)
    {
        _proxyHttpClientService = proxyHttpClientService;
        _tipsModelBuilder = tipsModelBuilder;
        _articleSearchModelBuilder = articleSearchModelBuilder;
    }
        
    [HttpGet]
    public async Task<IActionResult> GetTips(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
            return BadRequest("Number parameter is required");
        number = Regex.Replace(number, @"[^a-zA-Z0-9]", "");
        if (string.IsNullOrEmpty(number))
            return BadRequest("Invalid number format");

        string url = $"https://api.interparts.ru/abcp/tips/?number={number}";
        var json = await _proxyHttpClientService.GetJsonAsync(url);

        if (json == null)
            return NotFound("No tips found for this number");
        var tips = _tipsModelBuilder.BuildModel(json, number);
        return Ok(new TipsResponseModel
        {
            Tips = tips,
            OriginalNumber = number
        });
    }
    
    [HttpGet]
    public async Task<IActionResult> SearchArticles(
        [FromQuery] string brand,
        [FromQuery] string number)
    {
        if (string.IsNullOrWhiteSpace(brand) || string.IsNullOrWhiteSpace(number))
            return BadRequest("Brand and number parameters are required");
        
        brand = Regex.Replace(brand, @"[^a-zA-Z0-9]", "");
        number = Regex.Replace(number, @"[^a-zA-Z0-9]", "");

        if (string.IsNullOrEmpty(brand) || string.IsNullOrEmpty(number))
            return BadRequest("Invalid brand or number format");

        string url = $"/sehttps://api.interparts.ru/abcparch/?brand={brand}&number={number}";
        var json = await _proxyHttpClientService.GetJsonAsync(url);

        if (json == null)
            return NotFound("No articles found");

        var articles = _articleSearchModelBuilder.BuildModel(json, brand, number);

        return Ok(new 
        {
            OriginalBrand = brand,
            OriginalNumber = number,
            Articles = articles
        });
    }
    public async Task<IActionResult> SearchFirstArticle(
        [FromQuery] string brand,
        [FromQuery] string number)
    {
        if (string.IsNullOrWhiteSpace(brand) || string.IsNullOrWhiteSpace(number))
            return BadRequest("Brand and number parameters are required");

        // Очистка параметров
        brand = Regex.Replace(brand, @"[^a-zA-Z0-9]", "");
        number = Regex.Replace(number, @"[^a-zA-Z0-9]", "");

        if (string.IsNullOrEmpty(brand) || string.IsNullOrEmpty(number))
            return BadRequest("Invalid brand or number format");

        string url = $"https://api.interparts.ru/abcp/search/?brand={brand}&number={number}";
        var json = await _proxyHttpClientService.GetJsonAsync(url);

        if (json == null)
            return NotFound("No articles found");

        var articles = _articleSearchModelBuilder.BuildModel(json, brand, number);
        var firstArticle = articles.FirstOrDefault();

        if (firstArticle == null)
            return NotFound("No matching articles found");

        return Ok(new 
        {
            OriginalBrand = brand,
            OriginalNumber = number,
            Article = firstArticle
        });
    }
}