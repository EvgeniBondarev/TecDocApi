using System.Globalization;
using Microsoft.Extensions.Logging;
using OzonDomains.Models;
using OzonDomains.Models.Extensions;
using SlqStudio.Application.Services.EmailService;

namespace Servcies.PriceСlculationServcies
{
    public class OrderPriceManager
    {
        private const decimal MaxReasonableValue = 10_000_000m;
        private const decimal MinReasonableRate = 0.0001m;
        private const decimal MaxReasonableRate = 1000m;
        
        private readonly CurrencyRateFetcher _currencyRateFetcher;
        private readonly IEmailService _emailService;
        private readonly ILogger<OrderPriceManager> _logger;

        public OrderPriceManager(
            CurrencyRateFetcher currencyRateFetcher,
            IEmailService emailService,
            ILogger<OrderPriceManager> logger)
        {
            _currencyRateFetcher = currencyRateFetcher;
            _emailService = emailService;
            _logger = logger;
        }

       public async Task<Order> SetPurchasePriceToRUB(Order order)
       {
            if (order?.Supplier == null)
            {
                _logger.LogWarning("Order or Supplier is null in SetPurchasePriceToRUB");
                return order;
            }

            try
            {
                if (order.PurchasePrice <= 0 && order.OriginalPurchasePrice <= 0)
                {
                    _logger.LogWarning($"Invalid PurchasePrice: {order.PurchasePrice}, OriginalPurchasePrice: {order.OriginalPurchasePrice} for order {order?.Id}");
                    return order;
                }

                decimal rate = 1m;
                bool shouldConvert = false;

                if (order.Supplier.CurrencyCode == null)
                {
                    _logger.LogWarning($"Supplier CurrencyCode is null for order {order?.Id}");
                    return order;
                }

                switch (order.Supplier.CurrencyCode)
                {
                    case OzonDomains.CurrencyCode.USD:
                    case OzonDomains.CurrencyCode.EUR:
                    case OzonDomains.CurrencyCode.BYN:
                        // ✅ Если OriginalPurchasePrice ещё не задан — сохраняем его один раз
                        if (!order.OriginalPurchasePrice.HasValue || order.OriginalPurchasePrice == 0)
                        {
                            order.OriginalPurchasePrice = order.PurchasePrice;
                        }

                        rate = await GetValidatedCurrencyRateAsync(order.Supplier.CurrencyCode);
                        shouldConvert = true;
                        break;

                    case OzonDomains.CurrencyCode.RUB:
                        // ✅ В рублях не нужно хранить OriginalPurchasePrice
                        order.OriginalPurchasePrice = null;
                        break;

                    default:
                        _logger.LogWarning($"Unsupported currency code: {order.Supplier.CurrencyCode} for order {order?.Id}");
                        return order;
                }

                if (shouldConvert)
                {
                    if (order.Supplier.CostFactor == null)
                    {
                        _logger.LogWarning($"Supplier CostFactor is null for order {order?.Id}");
                        return order;
                    }

                    ValidateCostFactor(order.Supplier.CostFactor);

                    // ✅ Считаем всегда из OriginalPurchasePrice, а не из уже пересчитанного PurchasePrice
                    order.PurchasePrice = SafeCalculate(
                        () => (order.OriginalPurchasePrice ?? 0) * (order.Supplier.CostFactor ?? 1) * rate,
                        $"PurchasePrice calculation for order {order?.Id}");

                    await LogIfAbnormal(order.PurchasePrice, nameof(SetPurchasePriceToRUB), order);
                }

                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in SetPurchasePriceToRUB for order {order?.Id}");
                throw new OrderPriceCalculationException("Failed to convert purchase price to RUB", ex);
            }
        }


        public async Task<Order> CalculateProfit(Order order)
        {
            if (order?.Price == null || order?.CostPrice == null || order.CostPrice <= 0)
            {
                _logger.LogWarning($"Invalid input for CalculateProfit for order {order?.Id} - " +
                                 $"Price: {order?.Price}, CostPrice: {order?.CostPrice}");
                return order;
            }

            try
            {
                order.MaxProfit = null;
                order.MinProfit = null;

                // Проверяем, что оба значения существуют перед расчетом
                if (!order.Price.HasValue || !order.CostPrice.HasValue)
                {
                    _logger.LogWarning($"Missing price data for order {order?.Id} - " +
                                      $"Price: {order?.Price}, CostPrice: {order?.CostPrice}");
                    return order;
                }

                decimal startCommission = SafeCalculate(
                    () => order.Price.Value - order.CostPrice.Value,
                    $"StartCommission calculation for order {order?.Id}");

                if (startCommission < 0)
                {
                    _logger.LogWarning($"Negative start commission ({startCommission}) for order {order?.Id}");
                }

                await LogIfAbnormal(startCommission, $"{nameof(CalculateProfit)}_StartCommission", order);

                // Расчет максимальной прибыли (с минимальной комиссией Ozon)
                if (order.MinOzonCommission.HasValue)
                {
                    order.MaxProfit = SafeCalculate(
                        () => startCommission - order.MinOzonCommission.Value,
                        $"MaxProfit calculation for order {order?.Id}");

                    if (order.MaxProfit < 0)
                    {
                        _logger.LogWarning($"Negative MaxProfit ({order.MaxProfit}) for order {order?.Id}");
                    }
                    
                    await LogIfAbnormal(order.MaxProfit, $"{nameof(CalculateProfit)}_MaxProfit", order);
                }
                else
                {
                    _logger.LogDebug($"MinOzonCommission not set for order {order?.Id}");
                }

                // Расчет минимальной прибыли (с максимальной комиссией Ozon)
                if (order.MaxOzonCommission.HasValue)
                {
                    order.MinProfit = SafeCalculate(
                        () => startCommission - order.MaxOzonCommission.Value,
                        $"MinProfit calculation for order {order?.Id}");

                    if (order.MinProfit < 0)
                    {
                        _logger.LogWarning($"Negative MinProfit ({order.MinProfit}) for order {order?.Id}");
                    }
                    
                    await LogIfAbnormal(order.MinProfit, $"{nameof(CalculateProfit)}_MinProfit", order);
                }
                else
                {
                    _logger.LogDebug($"MaxOzonCommission not set for order {order?.Id}");
                }

                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in CalculateProfit for order {order?.Id}");
                throw new OrderPriceCalculationException("Failed to calculate profit", ex);
            }
        }

        public async Task<Order> CalculateDiscount(Order order)
        {
            if (order?.CostPrice == null || order.CostPrice <= 0)
            {
                _logger.LogWarning($"Invalid CostPrice for CalculateDiscount for order {order?.Id}. " +
                                  $"CostPrice: {order?.CostPrice}");
                return order;
            }

            try
            {
                order.MaxDiscount = null;
                order.MinDiscount = null;

                // Проверка на наличие CostPrice.Value перед расчетами
                if (!order.CostPrice.HasValue || order.CostPrice.Value <= 0)
                {
                    _logger.LogWarning($"Invalid CostPrice value for order {order?.Id}. " +
                                     $"Value: {order?.CostPrice}");
                    return order;
                }

                decimal costPrice = order.CostPrice.Value;

                // Расчет минимальной скидки
                if (order.MinProfit.HasValue)
                {
                    if (costPrice == 0)
                    {
                        _logger.LogError($"Division by zero avoided (CostPrice=0) for order {order?.Id}");
                        return order;
                    }

                    order.MinDiscount = SafeCalculate(
                        () => (order.MinProfit.Value / costPrice) * 100,
                        $"MinDiscount calculation for order {order?.Id}");

                    LogDiscountValue(order.MinDiscount, "MinDiscount", order?.Id.ToString());
                    
                    await LogIfAbnormal(order.MinDiscount, $"{nameof(CalculateDiscount)}_MinDiscount", order);
                }
                else
                {
                    _logger.LogDebug($"MinProfit not available for order {order?.Id}");
                }

                // Расчет максимальной скидки
                if (order.MaxProfit.HasValue)
                {
                    if (costPrice == 0)
                    {
                        _logger.LogError($"Division by zero avoided (CostPrice=0) for order {order?.Id}");
                        return order;
                    }

                    order.MaxDiscount = SafeCalculate(
                        () => (order.MaxProfit.Value / costPrice) * 100,
                        $"MaxDiscount calculation for order {order?.Id}");

                    LogDiscountValue(order.MaxDiscount, "MaxDiscount", order?.Id.ToString());
                    
                    await LogIfAbnormal(order.MaxDiscount, $"{nameof(CalculateDiscount)}_MaxDiscount", order);
                }
                else
                {
                    _logger.LogDebug($"MaxProfit not available for order {order?.Id}");
                }

                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in CalculateDiscount for order {order?.Id}. " +
                                   $"CostPrice: {order?.CostPrice}, " +
                                   $"MinProfit: {order?.MinProfit}, " +
                                   $"MaxProfit: {order?.MaxProfit}");
                throw new OrderPriceCalculationException("Failed to calculate discount", ex);
            }
        }

        private void LogDiscountValue(decimal? discount, string discountType, string orderId)
        {
            if (!discount.HasValue) return;
            
            if (discount.Value < 0)
            {
                _logger.LogWarning($"Negative {discountType} ({discount.Value}%) for order {orderId}");
            }
            else if (discount.Value > 100)
            {
                _logger.LogWarning($"Unusually high {discountType} ({discount.Value}%) for order {orderId}");
            }
        }

        public async Task<Order> CalculateCostPrice(Order order)
        {
            if (order?.Supplier == null || order?.ProductInfo == null)
            {
                _logger.LogWarning($"Invalid input for CalculateCostPrice for order {order?.Id}");
                return order;
            }

            try
            {
                decimal weightFactorInRub = 0m;

                if (order.Supplier?.WeightFactor > 0)
                {
                    if (order.Supplier.WeightFactorCurrencyCode == null)
                    {
                        _logger.LogWarning($"WeightFactorCurrencyCode is null for order {order?.Id}");
                        return order;
                    }

                    var currencyRate = order.Supplier.WeightFactorCurrencyCode != OzonDomains.CurrencyCode.RUB
                        ? await GetValidatedCurrencyRateAsync(order.Supplier.WeightFactorCurrencyCode)
                        : 1m;

                    weightFactorInRub = SafeCalculate(
                        () => (order.Supplier?.WeightFactor ?? 0) * currencyRate,
                        $"WeightFactor calculation for order {order?.Id}");
                }

                if (order.Supplier?.WeightFactor > 0 && order.ProductInfo?.Weight.HasValue == true)
                {
                    order.CostPrice = SafeCalculate(
                        () => (order.ProductInfo.Weight.Value * weightFactorInRub) + (order.PurchasePrice ?? 0),
                        $"CostPrice calculation with weight for order {order?.Id}");
                }
                else if (order.PurchasePrice.HasValue)
                {
                    order.CostPrice = NormalizeDecimal(order.PurchasePrice.Value);
                }
                else
                {
                    _logger.LogWarning($"No valid price data available for order {order?.Id}");
                    return order;
                }

                if (order.CostPrice.HasValue)
                {
                    await LogIfAbnormal(order.CostPrice.Value, nameof(CalculateCostPrice), order);
                }
                else
                {
                    _logger.LogWarning($"CostPrice calculation failed for order {order?.Id}");
                }

                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in CalculateCostPrice for order {order?.Id}");
                throw new OrderPriceCalculationException("Failed to calculate cost price", ex);
            }
        }

        private async Task<decimal> GetValidatedCurrencyRateAsync(OzonDomains.CurrencyCode currencyCode)
        {
            decimal rate = currencyCode switch
            {
                OzonDomains.CurrencyCode.USD => await _currencyRateFetcher.GetUSDRateAsync(),
                OzonDomains.CurrencyCode.EUR => await _currencyRateFetcher.GetEURRateAsync(),
                OzonDomains.CurrencyCode.BYN => await _currencyRateFetcher.GetBYNRateAsync(),
                OzonDomains.CurrencyCode.RUB => 1m,
                _ => throw new ArgumentOutOfRangeException(nameof(currencyCode), $"Unsupported currency: {currencyCode}")
            };

            if (rate < MinReasonableRate || rate > MaxReasonableRate)
            {
                throw new InvalidOperationException(
                    $"Abnormal currency rate {rate} for {currencyCode}. Expected between {MinReasonableRate} and {MaxReasonableRate}");
            }

            return rate;
        }

        private void ValidateCostFactor(decimal? costFactor)
        {
            if (!costFactor.HasValue)
            {
                throw new ArgumentNullException(nameof(costFactor), "Cost factor cannot be null");
            }

            if (costFactor <= 0m || costFactor > 100m)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(costFactor), 
                    $"Invalid cost factor {costFactor}. Expected value between 0 and 100");
            }
        }

        private decimal SafeCalculate(Func<decimal?> calculation, string context)
        {
            try
            {
                checked
                {
                    decimal? result = calculation();
                    
                    if (!result.HasValue)
                    {
                        throw new InvalidOperationException(
                            $"Calculation returned null in context: {context}");
                    }

                    decimal normalized = NormalizeDecimal(result.Value);
                    
                    if (Math.Abs(normalized) > MaxReasonableValue)
                    {
                        _logger.LogError(
                            $"Calculated value {normalized} exceeds reasonable limit {MaxReasonableValue} in {context}");
                    }
                    
                    return normalized;
                }
            }
            catch (OverflowException ex)
            {
                _logger.LogError(ex, $"Overflow in calculation: {context}");
                throw;
            }
            catch (DivideByZeroException ex)
            {
                _logger.LogError(ex, $"Division by zero in: {context}");
                throw;
            }
            catch (NullReferenceException ex)
            {
                _logger.LogError(ex, $"Null reference in calculation: {context}");
                throw new InvalidOperationException("Calculation returned null value", ex);
            }
        }

        private decimal NormalizeDecimal(decimal? value)
        {
            try
            {
                return Math.Round(value.Value, 2, MidpointRounding.AwayFromZero);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logger.LogError(ex, $"Rounding error occurred for value: {value}");
                return value.Value;
            }
        }

        private async Task LogIfAbnormal(decimal? value, string context, Order order)
        {
            if (!value.HasValue) 
            {
                _logger.LogWarning($"Null value detected in {context} for order {order?.Id}");
                return;
            }
            
            if (Math.Abs(value.Value) >= MaxReasonableValue)
            {
                string message = 
                    $"Abnormal value detected in {context}: {value.Value.ToString(CultureInfo.InvariantCulture)}\n" +
                    $"Order: {order?.Print() ?? "NULL_ORDER"}";
                
                try
                {
                    await _emailService.SendEmailAsync("Abnormal value detected", message);
                    _logger.LogWarning($"Sent abnormal value alert: {message}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to send abnormal value email for order {order?.Id}");
                }
            }
        }
    }

    public class OrderPriceCalculationException : Exception
    {
        public OrderPriceCalculationException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}