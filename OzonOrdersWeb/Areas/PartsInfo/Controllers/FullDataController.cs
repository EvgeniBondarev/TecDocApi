using Microsoft.AspNetCore.Mvc;
using OzonOrdersWeb.Areas.PartsInfo.ModelBuilders;
using OzonOrdersWeb.Areas.PartsInfo.Models.CrossCode;
using OzonOrdersWeb.Areas.PartsInfo.Models.ProductInformation;
using OzonOrdersWeb.Areas.PartsInfo.Models.Substitute;
using PartsInfo.HttpUtils;

namespace OzonOrdersWeb.Areas.PartsInfo.Controllers;

[Area("PartsInfo")]
public class FullDataController : Controller
{
    private readonly ProxyHttpClientService _proxyHttpClientService;
    private readonly ArticleFullModelBuilder _articleFullModelBuilder;
    private readonly CrossCodeBuilder _crossCodeBuilder;
    private readonly SubstituteBuilder _substituteBuilder;
    private readonly CrossArticleModelBuilder _crossArticleModelBuilder;
    private readonly ProductInformationModelBuilder _productInformationModelBuilder;
    public FullDataController(ProxyHttpClientService proxyHttpClientService,
        ArticleFullModelBuilder articleFullModelBuilder,
        CrossCodeBuilder crossCodeBuilder,
        SubstituteBuilder substituteBuilder,
        CrossArticleModelBuilder crossArticleModelBuilder,
        ProductInformationModelBuilder productInformationModelBuilder)
    {
        _proxyHttpClientService = proxyHttpClientService;
        _articleFullModelBuilder = articleFullModelBuilder;
        _crossCodeBuilder = crossCodeBuilder;
        _substituteBuilder = substituteBuilder;
        _crossArticleModelBuilder = crossArticleModelBuilder;
        _productInformationModelBuilder = productInformationModelBuilder;
    }
    
    [HttpGet]
    public async Task<IActionResult> Info(string supplier , string code)
    
    {
        if (string.IsNullOrEmpty(supplier))
            return RedirectToAction(
                actionName: "CodeInfo",
                controllerName: "Info",
                routeValues: new { area = "PartsInfo", code = code }
            );
        
        if (string.IsNullOrEmpty(code))
            return View();
        
        string supplierEncoded = Uri.EscapeDataString(supplier);
        string articleEncoded = Uri.EscapeDataString(code);
        string url = $"https://api.interparts.ru/detail-full-info/detail-full-info?supplier={supplierEncoded}&article={articleEncoded}";
        var json = await _proxyHttpClientService.GetJsonAsync(url);

        
        if (json == null)
            return View();
        
        var model = await _articleFullModelBuilder.BuildModel(json, code, supplier);
        
        ViewData["SearchCode"] = code; 
        ViewData["SearchSupplier"] = supplier; 
        return View(model);
    }
    
    [HttpGet]
    public async Task<ArticleFullModel> GetInfo(string supplier , string code)
    {
        if (string.IsNullOrEmpty(supplier))
            return new ArticleFullModel();
        
        if (string.IsNullOrEmpty(code))
            return new ArticleFullModel();
        
        string supplierEncoded = Uri.EscapeDataString(supplier);
        string articleEncoded = Uri.EscapeDataString(code);
        string url = $"https://api.interparts.ru/detail-full-info/?supplier={supplierEncoded}&article={articleEncoded}";
        var json = await _proxyHttpClientService.GetJsonAsync(url);

        
        if (json == null)
            return new ArticleFullModel();
        
        var model = await _articleFullModelBuilder.BuildModel(json, code, supplier);
        return model;
    }
    
    [HttpGet]
    public async Task<List<CrossCodeModel>> GetJcCross(string code, int jcSupplierId)
    {
        if (string.IsNullOrEmpty(code))
            return new List<CrossCodeModel>();
        
        string url = $"https://api.interparts.ru/cr-t-cross/crosscode-by-jc-producer/{code}/{jcSupplierId}";
        var json = await _proxyHttpClientService.GetJsonAsync(url);
        
        if (json == null)
            return new List<CrossCodeModel>();
        
        var model =  _crossCodeBuilder.BuildModel(json).ToList();
        return model;
    }
    
    [HttpGet]
    public async Task<List<CrossArticleModel>> GetTdCross(string code)
    {
        if (string.IsNullOrEmpty(code))
            return new List<CrossArticleModel>();
        
        string url = $"https://api.interparts.ru/tec-doc-cross/cross/{code}";
        var json = await _proxyHttpClientService.GetJsonAsync(url);
        
        if (json == null)
            return new List<CrossArticleModel>();
        
        var model = await  _crossArticleModelBuilder.Build(json, code);
        return model;
    }
    
    [HttpGet]
    public async Task<ProductInformationModel> GetProductInformation(string code,  string manufacturer)
    {
        if (string.IsNullOrEmpty(code))
            return new ProductInformationModel();
        
        string manufacturerEncoded = Uri.EscapeDataString(manufacturer);
        string articleEncoded = Uri.EscapeDataString(code);
        string url = $"https://api.interparts.ru/product-information/product/?article_number={articleEncoded}&manufacturer={manufacturerEncoded}";
        var json = await _proxyHttpClientService.GetJsonAsync(url);
        
        if (json == null)
            return new ProductInformationModel();
        
        var model = await  _productInformationModelBuilder.Build(json, code, manufacturer);
        return model;
    }
    
    [HttpGet]
    public async Task<List<VehicleModel>> GetSubstitute(string supplier , string code)
    {
        if (string.IsNullOrEmpty(code))
            return new List<VehicleModel>();
        
        string manufacturerEncoded = Uri.EscapeDataString(supplier);
        string articleEncoded = Uri.EscapeDataString(code);
        string url = $"https://api.interparts.ru/substitute/{manufacturerEncoded}/{articleEncoded}";
        var json = await _proxyHttpClientService.GetJsonAsync(url);
        
        if (json == null)
            return new List<VehicleModel>();
        
        var model =  _substituteBuilder.BuildModel(json, code, supplier).ToList();
        return model;
    }
}