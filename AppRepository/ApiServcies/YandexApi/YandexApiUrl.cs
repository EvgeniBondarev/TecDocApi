namespace Servcies.ApiServcies.YandexApi
{
    public class YandexApiUrl
    {

        public const string CAMPAIGNS = "https://api.partner.market.yandex.ru/campaigns";
        public const string CAMPAIGN = CAMPAIGNS + "/{0}";
        public const string ORDERS = CAMPAIGNS + "/{0}/orders";
    }
}
