using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using OzonOrdersWeb.Areas.PartsInfo.ModelBuilders;
using OzonOrdersWeb.Areas.PartsInfo.Models;
using PartsInfo.HttpUtils;

namespace OzonOrdersWeb.Areas.PartsInfo.Controllers;

[Area("PartsInfo")]
public class InfoController : Controller
{
    private readonly ProxyHttpClientService _proxyHttpClientService;
    private readonly SupplierModelBuilder _supplierModelBuilder;
    private readonly ArticleFullModelBuilder _articleFullModelBuilder;
    private readonly SubstituteBuilder _substituteBuilder;

    public InfoController(ProxyHttpClientService proxyHttpClientService,
                          SupplierModelBuilder supplierModelBuilder,
                          ArticleFullModelBuilder articleFullModelBuilder,
                          SubstituteBuilder substituteBuilder)
    {
        _proxyHttpClientService = proxyHttpClientService;
        _supplierModelBuilder = supplierModelBuilder;
        _articleFullModelBuilder = articleFullModelBuilder;
        _substituteBuilder = substituteBuilder;
    }
    
    [HttpGet]
    public async Task<IActionResult> CodeInfo(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return View();

        // Удаляем все символы, кроме латинских букв и цифр
        code = Regex.Replace(code, @"[^a-zA-Z0-9]", "");

        if (string.IsNullOrEmpty(code))
            return View();

        string url = $"https://api.interparts.ru/suppliers/{code}";
        var json = await _proxyHttpClientService.GetJsonAsync(url);

        if (json == null)
            return View();

        var model = await _supplierModelBuilder.BuildModel(code, json);
        ViewData["SearchCode"] = code;

        RunBackgroundCacheTask(model, code);

        return View(model);
    }

    private void RunBackgroundCacheTask(IEnumerable<SupplierModel> model, string code)
    {
        _ = Task.Run(async () =>
        {
            foreach (var item in model)
            {
                try
                {
                    var jsonFullDta = await _proxyHttpClientService.GetJsonAsync(
                        $"https://api.interparts.ru/detail-full-info/{item.Name}/{code}");
                    if (jsonFullDta != null)
                    {
                        await _articleFullModelBuilder.BuildModel(jsonFullDta, code, item.Name);
                    }
                    
                    string url = $"https://api.interparts.ru/substitute/{item.Name}/{code}";
                    _ = await _proxyHttpClientService.GetJsonAsync(url);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка в фоне: {ex.Message}");
                }
            }
        });
    }
}