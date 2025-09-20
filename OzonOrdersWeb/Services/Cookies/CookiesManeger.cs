using Newtonsoft.Json;

namespace OzonOrdersWeb.Services.Cookies
{
    public class CookiesManeger
    {
        public void SaveToCookies<T>(IResponseCookies response, string key, T filterData)
        {
            string filterDataJson = JsonConvert.SerializeObject(filterData);
            response.Append(key, filterDataJson);
        }

        public T GetFromCookies<T>(IRequestCookieCollection request, string key)
        {
            if (request.TryGetValue(key, out string filterValue))
            {
                T filterData = JsonConvert.DeserializeObject<T>(filterValue);

                return filterData;
            }
            return default(T);
        }
    }
}
