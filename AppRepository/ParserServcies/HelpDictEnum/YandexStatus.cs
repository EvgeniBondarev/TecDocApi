namespace Servcies.ParserServcies.HelpDictEnum
{
    public static class YandexStatus
    {
        public static Dictionary<string, string> OrderStatuses = new Dictionary<string, string>
        {
            { "CANCELLED", "Отменен" },
            { "DELIVERED", "Получен покупателем" },
            { "DELIVERY", "Передан в службу доставки" },
            { "PICKUP", "Доставлен в пункт самовывоза" },
            { "PROCESSING", "Находится в обработке" },
            { "PENDING", "Ожидает обработки со стороны продавца" },
            { "UNPAID", "Оформлен, но еще не оплачен (если выбрана оплата при оформлении)" },
            { "PLACING", "Оформляется, подготовка к резервированию" },
            { "RESERVED", "Зарезервирован, но недооформлен" },
            { "PARTIALLY_RETURNED", "Возвращен частично" },
            { "RETURNED", "Возвращен полностью" },
            { "UNKNOWN", "Неизвестный статус" }
        };
    }
}
