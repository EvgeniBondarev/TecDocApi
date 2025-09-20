namespace Servcies.ParserServcies.HelpDictEnum
{
    public static class OzonStatus
    {
        public static Dictionary<string, string> OrderStatuses = new Dictionary<string, string>
        {
            { "acceptance_in_progress", "Идёт приёмка" },
            { "arbitration", "Арбитраж" },
            { "awaiting_approve", "Ожидает подтверждения" },
            { "awaiting_deliver", "Ожидает отгрузки" },
            { "awaiting_packaging", "Ожидает упаковки" },
            { "awaiting_registration", "Ожидает регистрации" },
            { "awaiting_verification", "Создано" },
            { "cancelled", "Отменён" },
            { "cancelled_from_split_pending", "Отменён из-за разделения отправления" },
            { "client_arbitration", "Клиентский арбитраж доставки" },
            { "delivered", "Доставлен" },
            { "delivering", "Доставляется" },
            { "driver_pickup", "У водителя" },
            { "not_accepted", "Не принят на сортировочном центре" },
            { "sent_by_seller", "Отправлено продавцом" }
        };
    }

}
