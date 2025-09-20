using OzonDomains;
using Servcies.ApiServcies.InterpartsApi.Filters;
using Servcies.ApiServcies.InterpartsApi.Models;
using System.Globalization;

namespace Servcies.ApiServcies.InterpartsApi
{
    public class InterpartsApiDataManager
    {
        private InterpartsApiClient _interpartsApiClient;
        public string _key;

        public InterpartsApiDataManager()
        {
            _interpartsApiClient = new InterpartsApiClient();
        }
        public InterpartsApiDataManager SetClient(string key)
        {
            _key = key;
            return this;
        }

        public async Task<string> GetBrandByCode(string code)
        {
            if(string.IsNullOrEmpty(code))
            {
                return null;
            }
            SearchBrandRequest searchBrandRequest = new SearchBrandRequest()
            {
                Code = code,
                RequestUrl = InterpartsApiUrl.SEARCH_BASE_URL + "?token=" + _key + "&code=" + code
            };

            var result = _interpartsApiClient.MakeRequest(searchBrandRequest);

            if (result != null && result["result"] != null && result["result"].HasValues)
            {
                return result["result"][0]["brand"].ToString();
            }
            return null;
        }

        public async Task<DetailInformation> GetDetailByArticle(string article)
        {
            if (string.IsNullOrEmpty(article))
            {
                return null;
            }

            GetDetailRequest getDetailRequest = new GetDetailRequest()
            {
                Article = article,
                RequestUrl = InterpartsApiUrl.GET_DETAIL_URL + "?token=" + _key + "&article=" + article
            };

            var result = _interpartsApiClient.MakeRequest(getDetailRequest);

            if (result != null && result["result"] != null && result["result"].HasValues)
            {
                var pair = result["result"][0];

                return new DetailInformation
                {
                    DetailID = pair["detailID"]?.ToString(),
                    DetailExternalID = pair["detailExternalID"]?.ToString(),
                    Article = pair["article"]?.ToString(),
                    ArticleDisplay = pair["articleDisplay"]?.ToString(),
                    BrandID = pair["brandID"]?.ToString(),
                    BrandExternalID = pair["brandExternalID"]?.ToString(),
                    BrandName = pair["brandName"]?.ToString(),
                    Description = pair["description"]?.ToString(),
                    Weight = pair["weight"] != null &&
                             decimal.TryParse(pair["weight"].ToString(), out var weight) ? weight : (decimal?)null,
                    Volume = pair["volume"] != null &&
                             decimal.TryParse(pair["volume"].ToString(), out var volume) ? volume : (decimal?)null
                };
            }
            return null;
        }


        public async Task<List<SupplierDetailedInformation>> GetSupplierNameAndDirection(string code, string brand)
        {
            SearchBrandRequest searchBrandRequest = new SearchBrandRequest()
            {
                Code = code,
                RequestUrl = InterpartsApiUrl.SEARCH_BASE_URL + "?token=" + _key + "&code=" + code + "&brand=" + brand
            };

            List<SupplierDetailedInformation> supplierNameAndDirectionModels = [];
            var result = _interpartsApiClient.MakeRequest(searchBrandRequest);

            if(result["result"] != null)
            {
                foreach (var pair in result["result"])
                {
                    var culture = new CultureInfo("en-US");
                    supplierNameAndDirectionModels.Add(new SupplierDetailedInformation()
                    {
                        SupplierName = pair["supplierName"]?.ToString() ?? string.Empty,
                        Direction = pair["direction"]?.ToString() ?? string.Empty,
                        DeliveryDaysMax = pair["deliveryDaysMax"] != null &&
                                          int.TryParse(pair["deliveryDaysMax"].ToString(), NumberStyles.Integer, culture, out var deliveryDaysMax)
                                              ? deliveryDaysMax
                                              : 0,
                        DeliveryDaysMid = pair["deliveryDaysMid"] != null &&
                                          int.TryParse(pair["deliveryDaysMid"].ToString(), NumberStyles.Integer, culture, out var deliveryDaysMid)
                                              ? deliveryDaysMid
                                              : 0,
                        Wight = pair["weight"] != null &&
                                decimal.TryParse(pair["weight"].ToString(), NumberStyles.Number, culture, out var weight)
                                    ? weight
                                    : 0.0m,
                        Code = pair["code"]?.ToString() ?? string.Empty,
                        Price = pair["price"] != null &&
                                        decimal.TryParse(pair["price"].ToString(), NumberStyles.Currency, culture, out var price)
                                            ? price
                                            : 0.0m,
                        SupplierPrice = pair["supplierPrice"] != null &&
                                        decimal.TryParse(pair["supplierPrice"].ToString(), NumberStyles.Currency, culture, out var supplierPrice)
                                            ? supplierPrice
                                            : 0.0m,
                        DeliveryWeightTax = pair["deliveryWeightTax "] != null &&
                                        decimal.TryParse(pair["deliveryWeightTax "].ToString(), NumberStyles.Currency, culture, out var deliveryWeightTax)
                                            ? deliveryWeightTax
                                            : 0.0m,
                        Description = pair["description"]?.ToString() ?? string.Empty,
                        Packing = pair["packing"]?.ToString() ?? string.Empty,
                        Brand = pair["brand"]?.ToString() ?? string.Empty,
                    });
                }
            }
            return supplierNameAndDirectionModels;
        }
    }
}
