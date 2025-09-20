using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OzonDomains.Models.BitrixModels;
using OzonOrdersWeb.Areas.Studio2.ViewModels.Bitrix;
using OzonRepositories.Data.Bitrix;

[Area("Studio2")]
[Authorize(Roles = "User,Admin")]
public class BitrixController : Controller
{
    private readonly IBitrixRepository _repository;

    public BitrixController(IBitrixRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<IActionResult> GetBitrixData(string article)
    {
        List<BitrixModel> resultModels = new List<BitrixModel>();
        var articleInfo = _repository.GetBIblockElementPropertiesByArticle(article);
        if (articleInfo == null)
            return NotFound();
        
        foreach (var item in articleInfo)
        {
            BitrixModel model = new BitrixModel();
            var productInfo = await _repository.GetBCatalogPriceByIblockElementId(item.IblockElementId);
            if (productInfo == null)
                continue;
            
            var group = await _repository.GetBCatalogGroupById(productInfo.CatalogGroupId);
        
            model.Price = productInfo.Price;
            model.Currency = productInfo.Currency;
            model.Group = group.DisplayNameRu;
        
            var storesLinks = await _repository.GetBCatalogStoreProductsByProductId(productInfo.ProductId);
            if (storesLinks == null)
                continue;

            foreach (var store in storesLinks)
            {
                var stor = await _repository.GetBCatalogStoreById(store.StoreId);
                if (stor == null)
                    continue;
                model.Stores.Add(stor.Title, store.Amount);
            }
            resultModels.Add(model);
        }
        if (articleInfo == null)
        {
            return NotFound();
        }

        return Ok(resultModels);
    }
}
