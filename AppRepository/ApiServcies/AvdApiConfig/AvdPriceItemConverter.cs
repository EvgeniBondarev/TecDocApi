using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Servcies.ApiServcies.TradesoftApi.Models.Response;

public static class AvdPriceItemConverter
{
    public static List<PreOrderItem> ConvertToPreOrderItems(IEnumerable<AvdPriceItem> sourceItems)
    {
        if (sourceItems == null)
            return new List<PreOrderItem>();

        return sourceItems
            .Where(item => item != null)
            .Select(ConvertSingleItem)
            .ToList();
    }

    public static PreOrderItem ConvertSingleItem(AvdPriceItem source)
    {
        if (source == null)
            return null;

        return new PreOrderItem
        {
            Caption = source.ItemName ?? string.Empty,
            Rest = source.Quantity.ToString(),
            DeliveryDays = source.SupplierPeriod ?? "0",
            DeliveryDaysMin = ParseDeliveryDays(source.SupplierPeriod),
            DeliveryDaysMax = ParseDeliveryDays(source.SupplierPeriod),
            CurrencyCode = "RUB",
            Producer = source.CatalogName ?? string.Empty,
            Code = source.ItemNumber ?? string.Empty,
            Price = source.Price.ToString(CultureInfo.InvariantCulture),
            Direction = source.SupplierName ?? string.Empty,
            ItemHash = source.Hash ?? GenerateItemHash(source),
            ItemId = GenerateItemId(source),
            SiteUrl = "https://www.avdmotors.ru/",
            Title = "AVD",
            Description = BuildDescription(source)
        };
    }

    private static int ParseDeliveryDays(string supplierPeriod)
    {
        if (string.IsNullOrEmpty(supplierPeriod))
            return 0;
        
        if (int.TryParse(supplierPeriod, out int days))
            return days;
        
        return 0;
    }

    private static string BuildDescription(AvdPriceItem item)
    {
        var description = new System.Text.StringBuilder();
        
        description.AppendLine(item.ItemName);
        description.AppendLine($"Артикул: {item.ItemNumber}");
        description.AppendLine($"Бренд: {item.CatalogName}");
        description.AppendLine($"Поставщик: {item.SupplierName}");
        description.AppendLine($"Регион: {item.SupplierRegion}");
        description.AppendLine($"Оригинал: {(item.IsOriginal ? "Да" : "Нет")}");
        description.AppendLine($"Цена: {item.Price} руб.");
        description.AppendLine($"Количество: {item.Quantity} шт.");
        description.AppendLine($"Срок поставки: {item.SupplierPeriod} дн.");
        
        if (!string.IsNullOrEmpty(item.SupplierInfo))
            description.AppendLine($"Инфо: {item.SupplierInfo}");
        
        if (!string.IsNullOrEmpty(item.SupplierReturnDescription))
            description.AppendLine($"Условия возврата: {item.SupplierReturnDescription}");
        
        description.AppendLine($"Дата обновления: {item.DatePrice?.ToString("dd.MM.yyyy")}");

        return description.ToString();
    }

    private static string GenerateItemHash(AvdPriceItem item)
    {
        return $"{item.SupplierName}_{item.ItemNumber}_{item.CatalogName}".GetHashCode().ToString("X");
    }

    private static string GenerateItemId(AvdPriceItem item)
    {
        return $"{item.SupplierName}_{item.ItemNumber}_{item.CatalogName}";
    }
}