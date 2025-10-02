//Логика черновика
document.addEventListener('DOMContentLoaded', (event) => {
    // Функция для сохранения значения поля в куки
    function saveToCookies(key, value) {
        document.cookie = key + "=" + encodeURIComponent(value) + "; path=/";
        console.log(`Сохранено: поле ${key}, значение ${value}`);
    }

    // Функция для загрузки значения из куки
    function loadFromCookies(key) {
        const name = key + "=";
        const decodedCookie = decodeURIComponent(document.cookie);
        const ca = decodedCookie.split(';');
        for (let i = 0; i < ca.length; i++) {
            let c = ca[i].trim();
            if (c.indexOf(name) === 0) {
                return c.substring(name.length, c.length);
            }
        }
        return "";
    }

    // Для каждого select с классом 'status-select'
    document.querySelectorAll('.status-select').forEach(select => {
        const id = select.id;
        const savedValue = loadFromCookies(id);

        // Если есть сохраненное значение, устанавливаем его
        if (savedValue) {
            select.value = savedValue;
            console.log(`Загружено из куки: поле ${id}, значение ${savedValue}`);
        }

        // Сохраняем значение в куки при изменении
        select.addEventListener('change', function () {
            saveToCookies(id, this.value);
        });
    });

    // Для каждого select с классом 'supplier-select'
    document.querySelectorAll('.supplier-select').forEach(select => {
        const id = select.id;
        const savedValue = loadFromCookies(id);

        if (savedValue) {
            select.value = savedValue;
            console.log(`Загружено из куки: поле ${id}, значение ${savedValue}`);
        }

        select.addEventListener('change', function () {
            saveToCookies(id, this.value);
        });
    });

    // Для каждого input с классом 'orderNumberToSupplie-field'
    document.querySelectorAll('.orderNumberToSupplie-field').forEach(input => {
        const id = input.id;
        const savedValue = loadFromCookies(id);

        if (savedValue) {
            input.value = savedValue;
            console.log(`Загружено из куки: поле ${id}, значение ${savedValue}`);
        }

        input.addEventListener('input', function () {
            saveToCookies(id, this.value);
        });
    });

    // Для каждого input с классом 'product-weight-field'
    document.querySelectorAll('.product-weight-field').forEach(input => {
        const id = input.id;
        const savedValue = loadFromCookies(id);

        if (savedValue) {
            input.value = savedValue;
            console.log(`Загружено из куки: поле ${id}, значение ${savedValue}`);
        }

        input.addEventListener('input', function () {
            saveToCookies(id, this.value);
        });
    });

    // Для каждого input с классом 'purchase-price-field'
    document.querySelectorAll('.purchase-price-field').forEach(input => {
        const id = input.id;
        const savedValue = loadFromCookies(id);

        if (savedValue) {
            input.value = savedValue;
            console.log(`Загружено из куки: поле ${id}, значение ${savedValue}`);
        }

        input.addEventListener('input', function () {
            saveToCookies(id, this.value);
        });
    });

    // Для каждого input с классом 'ozon-commission-field'
    document.querySelectorAll('.ozon-commission-field').forEach(input => {
        const id = input.id;
        const savedValue = loadFromCookies(id);

        if (savedValue) {
            input.value = savedValue;
            console.log(`Загружено из куки: поле ${id}, значение ${savedValue}`);
        }

        input.addEventListener('input', function () {
            saveToCookies(id, this.value);
        });
    });
});

// Функция для извлечения числового значения из строки
function extractNumber(text) {
    if (typeof text !== 'string') {
        text = String(text); // Преобразуем в строку, если это не строка
    }
    const number = parseFloat(text.replace(/\s/g, '').replace(',', '.'));
    return isNaN(number) ? null : number;
}

document.addEventListener('DOMContentLoaded', (event) => {
    const supplierSelects = document.querySelectorAll('.supplier-select');
    const statusSelects = document.querySelectorAll('.status-select');
    const selectedSuppliers = {};

    function updateSelectedSuppliers() {
        for (const key in selectedSuppliers) {
            if (selectedSuppliers.hasOwnProperty(key)) {
                selectedSuppliers[key] = { count: 0, totalAmount: 0, totalQuantity: 0, totalPurchasePrice: 0 };
            }
        }

        document.querySelectorAll('#shipmentTable tbody tr').forEach((row) => {
            const statusSelect = row.querySelector('.status-select');
            const selectedStatus = statusSelect.options[statusSelect.selectedIndex].text.trim();

            if (selectedStatus !== "Отменен") {
                const supplierSelect = row.querySelector('.supplier-select');
                const shipmentAmount = extractNumber(row.querySelector('[id^="shipmentAmount_"]')?.textContent) || 0;
                const quantity = extractNumber(row.querySelector('[id^="Quantity_"]')?.textContent) || 0;
                const purchasePriceText = row.querySelector('[id^="PurchasePriceRub_"]')?.textContent || '0';
                const purchasePrice = extractNumber(purchasePriceText);

                const selectedOption = supplierSelect.options[supplierSelect.selectedIndex];
                const supplierName = selectedOption.text;

                if (selectedOption.value) {
                    if (selectedSuppliers[supplierName]) {
                        selectedSuppliers[supplierName].count++;
                        selectedSuppliers[supplierName].totalAmount += shipmentAmount;
                        selectedSuppliers[supplierName].totalQuantity += quantity;
                        selectedSuppliers[supplierName].totalPurchasePrice += purchasePrice;
                    } else {
                        selectedSuppliers[supplierName] = { count: 1, totalAmount: shipmentAmount, totalQuantity: quantity, totalPurchasePrice: purchasePrice };
                    }
                }
            }
        });

        const selectedSuppliersList = document.getElementById('selectedSuppliersList');
        selectedSuppliersList.innerHTML = '';

        const table = document.createElement('table');
        table.classList.add('table', 'table-striped');

        const thead = document.createElement('thead');
        const headerRow = document.createElement('tr');
        ['Поставщик', 'Заказы у поставщика', 'Количество заказов', 'Сумма отправления', 'Цена закупки RUB'].forEach(headerText => {
            const th = document.createElement('th');
            th.textContent = headerText;
            headerRow.appendChild(th);
        });
        thead.appendChild(headerRow);
        table.appendChild(thead);

        const tbody = document.createElement('tbody');
        for (const [supplier, data] of Object.entries(selectedSuppliers)) {
            if (data.count > 0) {
                const row = document.createElement('tr');
                const supplierCell = document.createElement('td');
                supplierCell.textContent = supplier;
                row.appendChild(supplierCell);

                const countCell = document.createElement('td');
                countCell.textContent = data.count;
                row.appendChild(countCell);

                const quantityCell = document.createElement('td');
                quantityCell.textContent = data.totalQuantity;
                row.appendChild(quantityCell);

                const amountCell = document.createElement('td');
                amountCell.textContent = data.totalAmount.toFixed(2);
                row.appendChild(amountCell);

                const purchasePriceCell = document.createElement('td');
                purchasePriceCell.textContent = data.totalPurchasePrice.toFixed(2);
                row.appendChild(purchasePriceCell);

                tbody.appendChild(row);
            }
        }
        table.appendChild(tbody);

        selectedSuppliersList.appendChild(table);
    }

    window.updateSupplierList = updateSelectedSuppliers;

    supplierSelects.forEach((select, index) => {
        select.addEventListener('change', updateSelectedSuppliers);
    });

    statusSelects.forEach((select) => {
        select.addEventListener('change', updateSelectedSuppliers);
    });

    updateSelectedSuppliers();
})

function calculateSumAndDisplay() {
    var total = 0;
    var shipmentAmountElements = document.querySelectorAll('[id^="shipmentAmount_"]');
    var totalDiv = document.getElementById("totalShipmentAmount");

    shipmentAmountElements.forEach(function (element) {
        var value = extractNumber(element.innerText);
        total += value;
    });

    // Выводим сумму в элемент div
    totalDiv.innerText = total;
}

// Вызываем функцию для вычисления и вывода суммы
calculateSumAndDisplay();

function calculateTotalPurchasePriceRub() {
    var total = 0;

    // Находим все элементы с классом 'purchase-price-rub'
    var elements = document.querySelectorAll('.purchase-price-rub');
    elements.forEach(function (element) {
        var textContent = element.textContent.trim();
        var numberValue = extractNumber(textContent);

        if (!isNaN(numberValue)) {
            total += numberValue;
        }
    });

    // Выводим общую сумму в элемент div
    var totalDiv = document.getElementById("totalPurchasePriceRub");
    totalDiv.innerText = total.toFixed(2) + " RUB";
}

// Функция для настройки MutationObserver
function setupMutationObserver() {
    // Находим все элементы с классом 'purchase-price-rub'
    var elements = document.querySelectorAll('.purchase-price-rub');
    elements.forEach(function (element) {
        // Создаем новый экземпляр MutationObserver
        var observer = new MutationObserver(function (mutationsList) {
            // При изменении содержимого вызываем функцию для пересчета суммы
            calculateTotalPurchasePriceRub();
        });

        // Настраиваем наблюдение за изменениями внутри элемента 'element'
        observer.observe(element, { childList: true, subtree: true });
    });
}

// Вызываем функцию для настройки MutationObserver при загрузке страницы
document.addEventListener('DOMContentLoaded', function () {
    calculateTotalPurchasePriceRub(); // Вызываем для первичного расчета суммы
    setupMutationObserver(); // Вызываем для настройки MutationObserver
});

$(document).ready(function () {
    var deletedOrders = [];
    $(document).on('click', '.delete-order', function () {
        var orderId = $(this).data('order-id');

        // Удаляем основную строку
        $('#orderRow_' + orderId).remove();

        // Удаляем строку с информацией
        $('.info-row[data-order-id="' + orderId + '"]').remove();

        // Добавляем ID удалённого заказа в массив
        deletedOrders.push(orderId);
        $('#deletedOrders').val(deletedOrders.join(','));

        // Обновление нумерации строк, если необходимо
        updateRowNumbers();

        // Перерасчет стоимости и других данных
        calculateSumAndDisplay();
        calculateTotalPurchasePriceRub();
        let list = JSON.parse(sessionStorage.getItem('selectedStockList' + params.get("ids")) || '[]');
        list = list.filter(item => String(item.orderId) !== String(orderId));
        sessionStorage.setItem('selectedStockList' + params.get("ids"), JSON.stringify(list));

        // Обновление списка поставщиков
        window.updateSupplierList();

    });
    function updateRowNumbers() {
        $('#shipmentTable tbody tr').each(function (index) {
            $(this).find('.row-number').text(index + 1);
        });
    }
    
});

$(document).ready(function () {
    // Обработка изменения поставщика в индивидуальных полях
    $('.supplier-select').on('change', function () {
        var selectedSupplier = $(this).find('option:selected');
        var currencyCode = selectedSupplier.data('currency-code');
        var orderId = $(this).attr('id').split('_')[1]; // Извлекаем ID заказа из ID элемента

        if (currencyCode) {
            $('#CurrencyCode_' + orderId).text(currencyCode);
        } else {
            $('#CurrencyCode_' + orderId).text('');
        }
    });

    // Обработка изменения поставщика в общем поле
    $('#universalSelect').on('change', function () {
        var selectedSupplier = $(this).find('option:selected');
        var supplierId = selectedSupplier.val();
        var currencyCode = selectedSupplier.data('currency-code');

        $('.supplier-select').each(function () {
            $(this).val(supplierId).trigger('change');
        });

        $('.currency-code').each(function () {
            if (currencyCode) {
                $(this).text(currencyCode);
            } else {
                $(this).text('');
            }
        });
    });
});

$(document).ready(function () {
    // Получаем значения курсов валют из Razor View
    var rateUSD = CurrencyRates.USD;
    var rateEUR = CurrencyRates.EUR;
    var rateBYN = CurrencyRates.BYN;

    // Обработка изменения поставщика в индивидуальных полях
    $('.supplier-select').on('change', function () {
        var selectedSupplier = $(this).find('option:selected');
        var weightFactor = extractNumber(selectedSupplier.data('weight-factor').toString());
        var orderId = $(this).attr('id').split('_')[1]; // Извлекаем ID заказа из ID элемента
        var weightInput = $('#ProductWeight_' + orderId);

        // Если весовой коэффициент равен 0, устанавливаем поле веса в 0 и делаем его readonly
        if (weightFactor === 0) {
            weightInput.val('0').prop('readonly', true);
        } else {
            weightInput.prop('readonly', false); // Включаем возможность редактирования
        }

        // Пример: обновляем другие значения на странице
        var currencyCode = selectedSupplier.data('currency-code');
        var weightCurrencyCode = selectedSupplier.data('weight-currency-code');
        var costFactor = extractNumber(selectedSupplier.data('cost-factor').toString());

        if (currencyCode) {
            $('#CurrencyCode_' + orderId).text(currencyCode);
            var purchasePriceInRub = updatePurchasePriceInRub(orderId, currencyCode, costFactor);
            updateCostPrice(orderId, currencyCode, costFactor, weightCurrencyCode, purchasePriceInRub);
        } else {
            $('#CurrencyCode_' + orderId).text('');
            $('#PurchasePriceRub_' + orderId).text('');
            $('#PurchasePriceHidden_' + orderId).text('');
            $('#CostPrice_' + orderId).text('');
        }
    });

    // Обработка изменения значения purchase price
    $('.purchase-price-field').on('input', function () {
        var orderId = $(this).attr('id').split('_')[1];
        var selectedSupplier = $('#SupplierId_' + orderId).find('option:selected');
        var currencyCode = selectedSupplier.data('currency-code');
        var weightCurrencyCode = selectedSupplier.data('weight-currency-code');
        var costFactor = extractNumber(selectedSupplier.data('cost-factor').toString());

        var purchasePriceInRub = updatePurchasePriceInRub(orderId, currencyCode, costFactor);
        updateCostPrice(orderId, currencyCode, costFactor, weightCurrencyCode, purchasePriceInRub);
        updateCalculations(orderId);
        window.updateSupplierList(); // Используем глобальную переменную
    });

    // Обработка изменения значения веса
    $('.product-weight-field').on('input', function () {
        var orderId = $(this).attr('id').split('_')[1];
        var selectedSupplier = $('#SupplierId_' + orderId).find('option:selected');
        var currencyCode = selectedSupplier.data('currency-code');
        var weightCurrencyCode = selectedSupplier.data('weight-currency-code');
        var costFactor = extractNumber(selectedSupplier.data('cost-factor').toString());

        var purchasePriceInRub = updatePurchasePriceInRub(orderId, currencyCode, costFactor);
        updateCostPrice(orderId, currencyCode, costFactor, weightCurrencyCode, purchasePriceInRub);
        updateCalculations(orderId);

        window.updateSupplierList();
    });

    $('.ozon-commission-field').on('input', function () {
        var orderId = $(this).attr('id').split('_')[1];
        updateCalculations(orderId);
    });

    // Вызываем обновление при загрузке страницы для каждого элемента
    $('.supplier-select').each(function () {
        var orderId = $(this).attr('id').split('_')[1];
        var selectedSupplier = $(this).find('option:selected');
        var currencyCode = selectedSupplier.data('currency-code');
        var weightCurrencyCode = selectedSupplier.data('weight-currency-code');
        var costFactor = extractNumber(selectedSupplier.data('cost-factor').toString());

        if (currencyCode) {
            $('#CurrencyCode_' + orderId).text(currencyCode);
            var purchasePriceInRub = updatePurchasePriceInRub(orderId, currencyCode, costFactor);
            updateCostPrice(orderId, currencyCode, costFactor, weightCurrencyCode, purchasePriceInRub);
        } else {
            $('#CurrencyCode_' + orderId).text('');
            $('#PurchasePriceRub_' + orderId).text('');
            $('#PurchasePriceHidden_' + orderId).text('');
            $('#CostPrice_' + orderId).text('');
        }
    });

    // Вызываем обновление вычислений при загрузке страницы для каждого элемента
    $('.purchase-price-field, .ozon-commission-field').each(function () {
        var orderId = $(this).attr('id').split('_')[1];
        updateCalculations(orderId);
    });

    function updatePurchasePriceInRub(orderId, currencyCode, costFactor) {
        var purchasePrice = parseFloat($('#PurchasePrice_' + orderId).val());
        var purchasePriceInRub = 0;
        var titleText = '';

        switch (currencyCode) {
            case 'USD':
                purchasePriceInRub = purchasePrice * costFactor * rateUSD;
                titleText = `${purchasePrice} (Цена закупки USD) * ${costFactor} (Коэффициент) * ${rateUSD} (Курс USD) = ${purchasePriceInRub}`;
                break;
            case 'EUR':
                purchasePriceInRub = purchasePrice * costFactor * rateEUR;
                titleText = `${purchasePrice} (Цена закупки EUR) * ${costFactor} (Коэффициент) * ${rateEUR} (Курс EUR) = ${purchasePriceInRub}`;
                break;
            case 'BYN':
                purchasePriceInRub = purchasePrice * costFactor * rateBYN;
                titleText = `${purchasePrice} (Цена закупки BYN) * ${costFactor} (Коэффициент) * ${rateBYN} (Курс BYN) = ${purchasePriceInRub}`;
                break;
            default:
                purchasePriceInRub = purchasePrice * costFactor;
                titleText = `${purchasePrice} (Цена закупки RUB) * ${costFactor} = ${purchasePriceInRub}`;
                break;
        }
        
        console.log(purchasePriceInRub);

        $('#PurchasePrice_' + orderId).attr('title', titleText);
        $('#PurchasePriceRub_' + orderId).text(!isNaN(purchasePriceInRub) ? Math.round(purchasePriceInRub) + ' RUB' : '');
        $('#PurchasePriceRub_' + orderId).attr('title', titleText);
        $('#PurchasePrice_' + orderId).data('purchase-price-rub', purchasePriceInRub);
        $('#PurchasePriceHidden_' + orderId).val(!isNaN(purchasePriceInRub) ? Math.round(purchasePriceInRub) : '');

        return purchasePriceInRub;
    }

    function updateCostPrice(orderId, currencyCode, costFactor, weightCurrencyCode, purchasePriceInRub) {
        var weight = parseFloat($('#ProductWeight_' + orderId).val());
        var supplierWeightFactor = parseFloat($('#SupplierId_' + orderId).find('option:selected').data('weight-factor').toString().replace(',', '.')) || 0;
        var supplierWeightFactorInRub = 0;

        var supplierWeightFactorInRubTitle = ``;
        switch (weightCurrencyCode) {
            case 'USD':
                supplierWeightFactorInRub = supplierWeightFactor * rateUSD;
                supplierWeightFactorInRubTitle = ` = ${supplierWeightFactor} * ${rateUSD}`;
                break;
            case 'EUR':
                supplierWeightFactorInRub = supplierWeightFactor * rateEUR;
                supplierWeightFactorInRubTitle = ` = ${supplierWeightFactor} * ${rateEUR}`;
                break;
            case 'BYN':
                supplierWeightFactorInRub = supplierWeightFactor * rateBYN;
                supplierWeightFactorInRubTitle = ` = ${supplierWeightFactor} * ${rateBYN}`;
                break;
            default:
                supplierWeightFactorInRub = supplierWeightFactor;
                break;
        }

        var costPrice = 0;
        var titleText = '';

        if (!isNaN(weight) && !isNaN(supplierWeightFactor) && !isNaN(purchasePriceInRub) && supplierWeightFactorInRub != 0) {
            costPrice = (weight * supplierWeightFactorInRub) + purchasePriceInRub;
            titleText = `${weight} (Вес) * ${supplierWeightFactorInRub} (Цена за кг. RUB${supplierWeightFactorInRubTitle}) + ${purchasePriceInRub} (Цена закупки RUB) = ${costPrice}`;
        } else if (!isNaN(purchasePriceInRub)) {
            costPrice = purchasePriceInRub;
            titleText = `${purchasePriceInRub} (Цена закупки RUB) = ${costPrice}`;
        } else {
            $('#CostPrice_' + orderId).text('');
            $('#CostPrice_' + orderId).attr('title', '');
            $('#CostPrice_' + orderId).data('cost-price', '');
            updateCalculations(orderId);
            return;
        }

        $('#CostPrice_' + orderId).text(Math.round(costPrice) + ' RUB');
        $('#CostPrice_' + orderId).attr('title', titleText);
        $('#CostPrice_' + orderId).data('cost-price', costPrice);

        updateCalculations(orderId);
    }



    function updateCalculations(orderId) {
        var costPrice = extractNumber($('#CostPrice_' + orderId).data('cost-price'));
        var price = extractNumber($('#Price_' + orderId).text());

        if (costPrice === null || costPrice === 0) {
            // Если costPrice равен null или 0, не выполнять расчеты
            $('#MinProfit_' + orderId).text('');
            $('#MaxProfit_' + orderId).text('');
            $('#MaxDiscount_' + orderId).text('');
            $('#MinDiscount_' + orderId).text('');
            return;
        }

        var minOzonCommission = extractNumber($('#MinOzonCommission_' + orderId).val() || $('#MinOzonCommission_' + orderId).text());
        var maxOzonCommission = extractNumber($('#MaxOzonCommission_' + orderId).val() || $('#MaxOzonCommission_' + orderId).text());

        var startCommission = price - costPrice;
        var minProfit = startCommission - maxOzonCommission;
        var maxProfit = startCommission - minOzonCommission;
        var minDiscount = (minProfit / costPrice) * 100;
        var maxDiscount = (maxProfit / costPrice) * 100;

        // Округляем до целого числа и обрезаем до 4 знаков, если нужно
        minProfit = Math.floor(minProfit);
        maxProfit = Math.floor(maxProfit);
        minDiscount = Math.floor(minDiscount);
        maxDiscount = Math.floor(maxDiscount);

        var displayMinProfit = minProfit > 9999 ? minProfit.toString().slice(0, 4) + '...' : minProfit.toString();
        var displayMaxProfit = maxProfit > 9999 ? maxProfit.toString().slice(0, 4) + '...' : maxProfit.toString();
        var displayMinDiscount = minDiscount > 9999 ? minDiscount.toString().slice(0, 4) + '...' : minDiscount.toString();
        var displayMaxDiscount = maxDiscount > 9999 ? maxDiscount.toString().slice(0, 4) + '...' : maxDiscount.toString();

        $('#MinProfit_' + orderId).text(!isNaN(minProfit) ? displayMinProfit : "");
        $('#MaxProfit_' + orderId).text(!isNaN(maxProfit) ? displayMaxProfit : "");
        $('#MaxDiscount_' + orderId).text(!isNaN(maxDiscount) ? displayMaxDiscount + ' %' : "");
        $('#MinDiscount_' + orderId).text(!isNaN(minDiscount) ? displayMinDiscount + ' %' : "");

        $('#MinProfit_' + orderId).css('color', minProfit < 0 ? 'red' : '');
        $('#MaxProfit_' + orderId).css('color', maxProfit < 0 ? 'red' : '');
        $('#MaxDiscount_' + orderId).css('color', maxDiscount < 0 ? 'red' : '');
        $('#MinDiscount_' + orderId).css('color', minDiscount < 0 ? 'red' : '');

        // Добавляем title к полям
        $('#MinProfit_' + orderId).attr('title', `${price} (Цена) - ${costPrice} (Себестоимость) - ${maxOzonCommission} (Макс. Комиссия ОЗОН) = ${minProfit}`);
        $('#MaxProfit_' + orderId).attr('title', `${price} (Цена) - ${costPrice} (Себестоимость) - ${minOzonCommission} (Мин. Комиссия ОЗОН) = ${maxProfit}`);

        $('#MaxDiscount_' + orderId).attr('title', `${maxProfit} (Макс. Прибыль) / ${costPrice} (Себестоимость) * 100 = ${maxDiscount}%`);
        $('#MinDiscount_' + orderId).attr('title', `${minProfit} (Мин. Прибыль) / ${costPrice} (Себестоимость) * 100 = ${minDiscount}%`);
    }

});

var inputChangeEvent = new Event('inputchange', {
    bubbles: true,
    cancelable: true
});

// Находим общее поле для выбора из выпадающего списка и поля для обновления в цикле
var universalSelect = document.getElementById('universalSelect');
var supplierSelects = document.querySelectorAll('.supplier-select');

// Добавляем обработчик события на изменение значения общего поля
universalSelect.addEventListener('change', function () {
    var value = this.value; // Получаем выбранное значение из общего поля

    // Обновляем значения полей в цикле
    supplierSelects.forEach(function (select) {
        select.value = value;
    });
});

// Находим общее поле для выбора статуса из выпадающего списка и поля для обновления в цикле
var universalStatusSelect = document.getElementById('universalStatusSelect');
var statusSelects = document.querySelectorAll('.status-select');

// Добавляем обработчик события на изменение значения общего поля
universalStatusSelect.addEventListener('change', function () {
    var value = this.value; // Получаем выбранное значение из общего поля

    // Обновляем значения полей в цикле
    statusSelects.forEach(function (select) {
        select.value = value;
    });
});

document.addEventListener('DOMContentLoaded', function () {
    // Находим общее поле для ввода и все целевые поля для обновления
    var universalField = document.getElementById('universalField');
    var supplierFields = document.querySelectorAll('.supplier-field');

    // Добавляем обработчик события на изменение значения общего поля
    universalField.addEventListener('input', function () {
        var value = this.value; // Получаем текущее значение из общего поля

        // Обновляем значения целевых полей в цикле
        supplierFields.forEach(function (field) {
            field.value = value;
            field.dispatchEvent(new Event('change'));
        });
    });
});

document.addEventListener('DOMContentLoaded', function () {
    // Находим общее поле для ввода и все целевые поля для обновления
    var universalField = document.getElementById('universalOrderNumberToSupplier');
    var universalOrderNumberToSupplier = document.querySelectorAll('.orderNumberToSupplie-field');

    // Добавляем обработчик события на изменение значения общего поля
    universalField.addEventListener('input', function () {
        var value = this.value; // Получаем текущее значение из общего поля

        // Обновляем значения целевых полей в цикле
        universalOrderNumberToSupplier.forEach(function (field) {
            field.value = value;
        });
    });
});

// Функция для чтения куки
function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
}


// Получаем блок modal-body
const modalBody = document.querySelector('.modal-body-checkbox');

// Получаем все чекбоксы внутри блока modal-body
const checkboxes = modalBody.querySelectorAll('input[type="checkbox"]');

// Устанавливаем начальное состояние чекбоксов из кук
checkboxes.forEach(function (checkbox) {
    const checkboxId = checkbox.id;
    const checkboxState = getCookie(checkboxId);

    console.log(getCookie(checkboxId));

    if (checkboxState !== undefined) {
        checkbox.checked = checkboxState === 'true';
        processCheckboxState(checkboxId, checkboxState);
        console.log(1)
    }
});

// Перебираем чекбоксы и добавляем обработчик события change
checkboxes.forEach(function (checkbox) {
    checkbox.addEventListener('change', function (event) {
        const checkboxId = event.target.id;
        const checkboxState = event.target.checked;
        document.cookie = `${checkboxId}=${checkboxState}`;
        processCheckboxState(checkboxId, checkboxState);
    });
});

// Функция для скрытия и раскрытия столбца таблицы по его ID
function toggleColumnVisibility(columnIndex, isVisible) {
    const table = document.getElementById("shipmentTable");
    if (table) {
        const rows = table.rows;
        for (let i = 0; i < rows.length; i++) {
            const cells = rows[i].cells;

            isVisible = typeof isVisible === 'string' ? isVisible.toLowerCase() === 'true' : isVisible;

            if (!isVisible) {
                console.log("none")
                cells[columnIndex].style.display = 'none';

            }
            else {
                console.log("none2")
                cells[columnIndex].style.display = '';
            }

        }
    }
};

document.addEventListener('DOMContentLoaded', (event) => {
    const supplierSelects = document.querySelectorAll('.supplier-select');

    function checkField(select) {
        const value = select.value.trim().toLowerCase();
        const selectedText = select.options[select.selectedIndex].text;

        if (value === "" || selectedText.startsWith('Не указан')) {
            select.classList.add('highlight-error');
        } else {
            select.classList.remove('highlight-error');
        }
    }

    supplierSelects.forEach(select => {
        select.addEventListener('change', function () {
            checkField(this);
        });

        checkField(select);
    });
});


const orderDataCache = {};
const rowCache = new Map();

function setStok(userOrderId) {
    const data = JSON.parse(sessionStorage.getItem('selectedStockList' + ids) || '[]');
    data.forEach(({ orderId, priceValue, indexRow }) => {
        if (userOrderId == orderId) {
            const button = document.querySelector(`.stok-order[data-order-id="${orderId}"][data-index-row="${indexRow}"]`);
            if (button) {
                button.click();
            }
        }
    });
}
document.addEventListener('DOMContentLoaded', function() {
    const buttons = document.querySelectorAll('.get-full-order-info');
    buttons.forEach((button, index) => {
        button.addEventListener('click', function() {
            fetchSupplierInfo(this);
        });
    });
    document.getElementById('startButton').addEventListener('click', function () {
        this.style.display = 'none';
        buttons.forEach((button, index) => {
            setTimeout(() => fetchSupplierInfoInBg(button), index * 4500);
        });
    });
});


async function fetchSupplierInfo(button) {
    const orderId = button.getAttribute('data-order-id');
    const orderArticle = button.getAttribute('data-order-article');
    const rowId = `#orderRow_${orderId}`;

    // Иконки для кнопки
    const svgLoading = `
                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-hourglass-split" viewBox="0 0 16 16">
                        <path d="M2.5 15a.5.5 0 1 1 0-1h1v-1a4.5 4.5 0 0 1 2.557-4.06c.29-.139.443-.377.443-.59v-.7c0-.213-.154-.451-.443-.59A4.5 4.5 0 0 1 3.5 3V2h-1a.5.5 0 0 1 0-1h11a.5.5 0 0 1 0 1h-1v1a4.5 4.5 0 0 1-2.557 4.06c-.29.139-.443.377-.443.59v.7c0 .213.154.451.443.59A4.5 4.5 0 0 1 12.5 13v1h1a.5.5 0 0 1 0 1zm2-13v1c0 .537.12 1.045.337 1.5h6.326c.216-.455.337-.963.337-1.5V2zm3 6.35c0 .701-.478 1.236-1.011 1.492A3.5 3.5 0 0 0 4.5 13s.866-1.299 3-1.48zm1 0v3.17c2.134.181 3 1.48 3 1.48a3.5 3.5 0 0 0-1.989-3.158C8.978 9.586 8.5 9.052 8.5 8.351z"/>
                    </svg>
                    <span class="visually-hidden">Loading</span>
                `;

    const svgUp = `
                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-chevron-double-up" viewBox="0 0 16 16">
                        <path fill-rule="evenodd" d="M7.646 2.646a.5.5 0 0 1 .708 0l6 6a.5.5 0 0 1-.708.708L8 3.707 2.354 9.354a.5.5 0 1 1-.708-.708z"/>
                        <path fill-rule="evenodd" d="M7.646 6.646a.5.5 0 0 1 .708 0l6 6a.5.5 0 0 1-.708.708L8 7.707l-5.646 5.647a.5.5 0 0 1-.708-.708z"/>
                    </svg>
                    <span class="visually-hidden">Info</span>
                `;

    const svgDown = `
                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-chevron-double-down" viewBox="0 0 16 16">
                        <path fill-rule="evenodd" d="M1.646 6.646a.5.5 0 0 1 .708 0L8 12.293l5.646-5.647a.5.5 0 0 1 .708.708l-6 6a.5.5 0 0 1-.708 0l-6-6a.5.5 0 0 1 0-.708z"/>
                        <path fill-rule="evenodd" d="M1.646 2.646a.5.5 0 0 1 .708 0L8 8.293l5.646-5.647a.5.5 0 0 1 .708.708l-6 6a.5.5 0 0 1-.708 0l-6-6a.5.5 0 0 1 0-.708z"/>
                    </svg>
                    <span class="visually-hidden">Info</span>
                `;

    // Устанавливаем иконку загрузки
    button.innerHTML = svgLoading;
    button.disabled = true;

    // Если строка уже открыта - закрываем
    if ($(rowId).next().hasClass('info-row')) {
        $(rowId).next().remove();
        button.innerHTML = svgDown;
        button.disabled = false;
        return;
    }

    try {
        // Проверяем кеш
        if (rowCache.has(orderId)) {
            $(rowId).after(rowCache.get(orderId));
            button.innerHTML = svgUp;
            button.disabled = false;
            setStok(orderId);
            return;
        }

        // Создаем массив промисов для всех запросов
        const requests = [];

        // Добавляем запросы только если данных нет в кэше
        if (!orderDataCache[orderId]?.orderDetails) {
            requests.push(fetchOrderDetails(orderId).then(data => processOrderDetails(data, orderId)));
        }

        if (!orderDataCache[orderId]?.tradesoftData) {
            requests.push(fetchTradesoftData(orderId).then(data => processTradesoftData(data, orderId)));
        }

        if (!orderDataCache[orderId]?.abcpData) {
            requests.push(fetchAbcpData(orderId).then(data => processAbcpData(data, orderId)));
        }

        if (!orderDataCache[orderId]?.avdData) {
            requests.push(fetchAvdData(orderId).then(data => processAvdData(data, orderId)));
        }

        if (!orderDataCache[orderId]?.stockData) {
            requests.push(getOrderStocks(orderId).then(data => processStockData(data, orderId)));
        }

        if (!orderDataCache[orderId]?.zzapData) {
            requests.push(fetchZzapDataWithRetry(orderId).then(data => processZzapData(data, orderId)));
        }

        if (!orderDataCache[orderId]?.bitrixData) {
            requests.push(fetchBitrixData(orderArticle).then(data => processBitrixData(data, orderId)));
        }

        // Всегда выполняем этот запрос, так как он не кэшируется
        requests.push(fetchOneAbcpData(orderId).then(data => processOneAbcpData(data, orderId)));

        // Ожидаем завершения всех запросов
        await Promise.all(requests);

        const rates = {
            'RUB': 1,
            'USD': parseFloat(CurrencyRates.USD.replace(',', '.')),
            'EUR': parseFloat(CurrencyRates.EUR.replace(',', '.')),
            'BYN': parseFloat(CurrencyRates.BYN.replace(',', '.'))
        };

        const infoRow = generateInfoRow(orderId, rates);
        rowCache.set(orderId, infoRow);
        $(rowId).after(infoRow);
        setStok(orderId);
        updateAllPricesForOrder(orderId);
        button.innerHTML = svgUp;

    } catch (error) {
        console.error("Ошибка при загрузке данных:", error);
        // Возвращаем исходную иконку при ошибке
        button.innerHTML = svgDown;
    } finally {
        button.disabled = false;
    }
}

async function fetchSupplierInfoInBg(button) {
    const orderId = button.getAttribute('data-order-id');
    const orderArticle = button.getAttribute('data-order-article');

    try {
        // Создаем массив промисов для всех запросов
        const requests = [];

        // Добавляем запросы только если данных нет в кэше
        if (!orderDataCache[orderId]?.orderDetails) {
            requests.push(
                fetchOrderDetails(orderId)
                    .then(data => {
                        if (data && Object.keys(data).length > 0) {
                            processOrderDetails(data, orderId);
                        }
                    })
            );
        }

        if (!orderDataCache[orderId]?.tradesoftData) {
            requests.push(
                fetchTradesoftData(orderId)
                    .then(data => {
                        if (data && Object.keys(data).length > 0) {
                            processTradesoftData(data, orderId);
                        }
                    })
            );
        }

        if (!orderDataCache[orderId]?.abcpData) {
            requests.push(
                fetchAbcpData(orderId)
                    .then(data => {
                        if (data && Object.keys(data).length > 0) {
                            processAbcpData(data, orderId);
                        }
                    })
            );
        }

        if (!orderDataCache[orderId]?.avdData) {
            requests.push(
                fetchAvdData(orderId)
                    .then(data => {
                        if (data && Object.keys(data).length > 0) {
                            processAvdData(data, orderId);
                        }
                    })
            );
        }

        if (!orderDataCache[orderId]?.stockData) {
            requests.push(
                getOrderStocks(orderId)
                    .then(data => {
                        if (data && Object.keys(data).length > 0) {
                            processStockData(data, orderId);
                        }
                    })
            );
        }

        if (!orderDataCache[orderId]?.zzapData) {
            requests.push(
                fetchZzapDataWithRetry(orderId)
                    .then(data => {
                        if (data && data.length > 0) {
                            processZzapData(data, orderId);
                        }
                    })
            );
        }

        if (!orderDataCache[orderId]?.bitrixData) {
            requests.push(
                fetchBitrixData(orderArticle)
                    .then(data => {
                        if (data && data.length > 0) {
                            processBitrixData(data, orderId);
                        }
                    })
            );
        }
        await Promise.all(requests);

    } catch (error) {
        console.error("Ошибка при загрузке данных:", error);
    } finally {
        button.disabled = false;
    }
}

function fetchOrderDetails(orderId) {
    return $.ajax({
        url: '@Url.Action("OrderDetailedInformation", "Orders")',
        type: 'GET',
        data: { orderId: orderId }
    }).catch(error => {
        if (error.status === 500) {
            console.error('Ошибка 500 при получении деталей заказа:', error);
        }
        return null;
    });
}

function fetchTradesoftData(orderId) {
    return $.ajax({
        url: '@Url.Action("GetTradesoftData", "Tradesoft")',
        type: "GET",
        data: { orderId: orderId }
    }).catch(error => {
        if (error.status === 500) {
            console.error('Ошибка 500 при получении данных от Tradesoft:', error);
        }
        return null;
    });
}

function fetchAbcpData(orderId) {
    return $.ajax({
        url: '@Url.Action("GetAbcpData", "Tradesoft")',
        type: "GET",
        data: {
            orderId: orderId,
        }
    }).catch(error => {
        if (error.status === 500) {
            console.error('Ошибка 500 при получении данных от ABCP:', error.responseText);
        }
        return null;
    });
}


function fetchOneAbcpData(orderId) {
    return $.ajax({
        url: '@Url.Action("GetOneAbcpData", "Tradesoft")',
        type: "GET",
        data: {
            orderId: orderId,
        }
    }).catch(error => {
        if (error.status === 500 || error.status === 204) {
            console.error('Ошибка 500 при получении данных от ABCP:', error.responseText);
        }
        return null;
    });
}

async function fetchZzapDataWithRetry(orderId, retryCount = 0) {
    const regionCode = $('#regionSelect').val() || 1;

    try {
        const response = await $.ajax({
            url: '@Url.Action("GetZzapData", "Tradesoft")',
            type: "GET",
            data: {
                orderId: orderId,
                regionCode: regionCode
            }
        });
        return response;
    } catch (error) {
        if (error.status === 400 && retryCount < 3) {
            const delay = Math.pow(2, retryCount) * 1000;
            await new Promise(resolve => setTimeout(resolve, delay));
            return fetchZzapDataWithRetry(orderId, retryCount + 1);
        }
        if (error.status === 500) {
            console.error('Ошибка 500 при запросе к Zzap API:', error);
        }
        return [];
    }
}
function fetchAvdData(orderId) {
    return $.ajax({
        url: '@Url.Action("GetAvdData", "Tradesoft")',
        type: "GET",
        data: {
            orderId: orderId,
        }
    }).catch(error => {
        if (error.status === 500) {
            console.error('Ошибка 500 при получении данных от AVD:', error.responseText);
        }
        return null;
    });
}

function fetchBitrixData(orderArticle) {
    return $.ajax({
        url: '@Url.Action("GetBitrixData", "Bitrix")',
        type: "GET",
        data: {
            article: orderArticle
        }
    }).catch(error => {
        if (error.status === 500) {
            console.error('Ошибка 500 при получении данных из Bitrix:', error.responseText);
        }
        return null;
    });
}


function getOrderStocks(orderId) {
    return $.ajax({
        url: '@Url.Action("OrderStocks", "Orders")',
        type: "GET",
        data: { orderId: orderId }
    }).catch(error => {
        if (error.status === 500) {
            console.error('Ошибка 500 при получении остатков по заказу:', error);
        }
        return null;
    });
}

// Функции обработки данных
function processOrderDetails(response, orderId) {
    const images = response.detailInfo.imgUrls?.length > 0
        ? response.detailInfo.imgUrls
        : ["https://www.interparts.store/_sysimg/no-photo.png"];

    // Обновление иконок
    const hidden_info = document.querySelector(`.hidden-info-icon[data-order-id="${orderId}"]`);
    if (hidden_info) {
        hidden_info.style.display = (
            (response.detailAttributes?.length !== 0) ||
            (response.productInfo && Object.keys(response.productInfo).length > 0)
        ) ? 'inline' : 'none';
    }

    const preferred_supplier = document.querySelector(`.preferred-supplier[data-order-id="${orderId}"]`);
    if (preferred_supplier) {
        preferred_supplier.style.display = 'none';
    }

    // Сохранение в кеш
    if (!orderDataCache[orderId]) orderDataCache[orderId] = {};
    orderDataCache[orderId].orderDetails = {
        response,
        mainImage: images[0],
        limitedMiniatures: images.slice(1, 5)
    };
}

function processTradesoftData(tradesofResponse, orderId) {
    const tradesofData = tradesofResponse;

    // Обновление иконки Tradesoft
    const preferred_tradesoft = document.querySelector(`.preferred-tradesoft[data-order-id="${orderId}"]`);
    if (preferred_tradesoft) {
        preferred_tradesoft.style.display = tradesofData.length > 0 ? 'inline' : 'none';
    }

    if (!orderDataCache[orderId]) orderDataCache[orderId] = {};
    orderDataCache[orderId].tradesoftData = {
        tradesofData
    };
}
// 5. Новая функция для обработки данных Zzap
function processZzapData(zzapResponse, orderId) {
    // Проверяем, что ответ успешный и содержит данные
    const zzapData = zzapResponse && !zzapResponse.error ? zzapResponse : [];

    // Обновление иконки Zzap (добавьте соответствующий элемент в HTML)
    const preferred_zzap = document.querySelector(`.preferred-zzap[data-order-id="${orderId}"]`);
    if (preferred_zzap) {
        preferred_zzap.style.display = zzapData.length > 0 ? 'inline' : 'none';
    }

    // Сохраняем данные в кеш
    if (!orderDataCache[orderId]) orderDataCache[orderId] = {};
    orderDataCache[orderId].zzapData = {
        zzapData
    };

    // Дополнительная логика обработки данных Zzap при необходимости
}

function processAbcpData(abcpResponse, orderId) {
    const abcpData = abcpResponse || [];
    const preferredAbcp = document.querySelector(`.preferred-abcp[data-order-id="${orderId}"]`);
    if (preferredAbcp) {
        preferredAbcp.style.display = abcpData.length > 0 ? 'inline' : 'none';
        preferredAbcp.title = abcpData.length > 0
            ? `Найдено ${abcpData.length} позиций в ABCP`
            : 'Нет данных в ABCP';
    }

    if (!orderDataCache[orderId]) orderDataCache[orderId] = {};
    orderDataCache[orderId].abcpData = {
        items: abcpData,
        lastUpdated: new Date().toISOString()
    };
    if (abcpData.length > 0) {
        updatePriceComparison(orderId, abcpData);
    }
}

function processOneAbcpData(abcpOneDataResponse, orderId) {
    if (!orderDataCache[orderId]) orderDataCache[orderId] = {};
    orderDataCache[orderId].abcpOneData = {
        ...abcpOneDataResponse, // сохраняем плоско
        lastUpdated: new Date().toISOString()
    };
}

function processAvdData(avdResponse, orderId) {
    const avdData = avdResponse || [];
    const preferredAvd = document.querySelector(`.preferred-avd[data-order-id="${orderId}"]`);

    if (preferredAvd) {
        preferredAvd.style.display = avdData.length > 0 ? 'inline' : 'none';
        preferredAvd.title = avdData.length > 0
            ? `Найдено ${avdData.length} позиций в AVD`
            : 'Нет данных в AVD';
    }

    if (!orderDataCache[orderId]) orderDataCache[orderId] = {};
    orderDataCache[orderId].avdData = {
        items: avdData,
        lastUpdated: new Date().toISOString()
    };

    if (avdData.length > 0) {
        updatePriceComparison(orderId, avdData);
    }
}

function processStockData(stocks, orderId) {
    // Обновление иконки склада
    const preferred_stock = document.querySelector(`.preferred-stock[data-order-id="${orderId}"]`);
    if (preferred_stock) {
        preferred_stock.style.display = stocks?.length > 0 ? 'inline' : 'none';
    }

    // Сохранение в кеш
    if (!orderDataCache[orderId]) orderDataCache[orderId] = {};
    orderDataCache[orderId].stockData = { stocks };
}

function processBitrixData(bitrixResponse, orderId) {
    const bitrixData = bitrixResponse || {};
    const stockElement = document.querySelector(`.bitrix-stock[data-order-id="${orderId}"]`);

    if (stockElement) {
        stockElement.style.display = Object.keys(bitrixData.stores || {}).length > 0 ? 'inline' : 'none';
        stockElement.title = Object.keys(bitrixData.stores || {}).length > 0
            ? `Цена: ${bitrixData.price} ${bitrixData.currency}, остатки: ${JSON.stringify(bitrixData.stores)}`
            : 'Нет данных в Bitrix';
    }

    if (!orderDataCache[orderId]) orderDataCache[orderId] = {};
    orderDataCache[orderId].bitrixData = {
        data: bitrixData,
        lastUpdated: new Date().toISOString()
    };
}


function openImage(imageSrc) {
    const modalImage = document.getElementById('modalImage');
    if (modalImage) {
        modalImage.src = imageSrc;
        modalImage.style.width = '530px'; // Увеличиваем в 2 раза (265 * 2)
        modalImage.style.height = '530px';
        const modal = new bootstrap.Modal(document.getElementById('imageModal'));
        modal.show();
    } else {
        console.error("Элемент modalImage не найден.");
    }
}

function formatPrice(price) {
    if (price == null) return '';
    // Remove decimals by converting the price to an integer
    let formattedPrice = Math.floor(price).toLocaleString('ru-RU');
    // Replace commas with spaces for the thousands separator
    return formattedPrice.replace(",", " ");
}

function handleRowClick(index) {
    const supplier = response.suppliersDetailedInformation[index];
    alert('Вы выбрали поставщика: ' + supplier.supplierName);
    // Здесь можно выполнить любую другую функцию по вашему требованию
}

// Функция для выделения строки и конкретного элемента
function highlightRow(index, element) {
    const table = element.closest('table'); // Найти таблицу, в которой находится элемент
    const orderId = element.getAttribute('data-order-id'); // Получить data-order-id текущего элемента

    // Найти все элементы с данным индексом и data-order-id внутри одной таблицы
    const elements = table.querySelectorAll(`[data-index="${index}"][data-order-id="${orderId}"]`);

    // Добавить класс 'highlighted' для всех найденных элементов
    elements.forEach(el => {
        el.classList.add('highlighted');
    });

    // Выделяем текущий элемент отдельно
    element.classList.add('highlighted-item');
}

function setWight(element) {
    const orderId = element.getAttribute('data-order-id');
    const blockValue = element.textContent.trim();

    var productWeight = document.getElementById(`ProductWeight_${orderId}`);
    const productWeightValue = parseFloat(blockValue);
    if (!isNaN(productWeightValue)) {
        productWeight.value = productWeightValue;
    }

    var event = new Event('input', { bubbles: true, cancelable: true });
    productWeight.dispatchEvent(event);
}

function setSupplierPrice(element){
    const orderId = element.getAttribute('data-order-id');
    const blockValue = element.textContent.trim();

    var purchasePrice = document.getElementById(`PurchasePrice_${orderId}`);
    const purchasePriceValue = parseFloat(blockValue.replace(/[^0-9.-]+/g, ''));
    if (!isNaN(purchasePriceValue)) {
        purchasePrice.value = purchasePriceValue;
    }

    var event = new Event('input', { bubbles: true, cancelable: true });
    purchasePrice.dispatchEvent(event);
}

// Функция для снятия выделения
function removeHighlight(index, element) {
    const table = element.closest('table'); // Найти таблицу, в которой находится элемент
    const orderId = element.getAttribute('data-order-id');

    // Найти все элементы с данным индексом и data-order-id внутри одной таблицы
    const elements = table.querySelectorAll(`[data-index="${index}"][data-order-id="${orderId}"]`);

    // Удалить класс 'highlighted' у всех найденных элементов
    elements.forEach(el => {
        el.classList.remove('highlighted', 'highlighted-item');
    });
}


const params = new URLSearchParams(window.location.search);
const ids = params.get("ids");
console.log(ids);

$(document).ready(function () {
    // Кэш для данных
    const substituteCache = new Map();

    // Обработчик клика по ссылке
    $(document).on("click", ".getSubstituteLink", function (e) {
        e.preventDefault(); // Предотвращаем переход по ссылке

        const orderId = $(this).data("id"); // Получаем ID заказа из data-атрибута
        const linkElement = $(this); // Сохраняем ссылку для управления

        if (!orderId) {
            alert("ID заказа не указан.");
            return;
        }

        // Деактивируем ссылку
        linkElement.addClass("disabled").prop("disabled", true);

        // Проверяем наличие данных в кэше
        if (substituteCache.has(orderId)) {
            const cachedData = substituteCache.get(orderId);
            displaySubstituteData(cachedData);
            // Активируем ссылку обратно
            linkElement.removeClass("disabled").prop("disabled", false);
            return;
        }

        // Показываем спиннер, если он существует
        const loader = document.getElementById('loadingSpinner');
        if (loader) loader.style.display = 'block';

        $.ajax({
            url: `/Studio2/Orders/OrderSubstitute`, // Укажите имя контроллера
            type: "GET",
            data: { orderId: orderId }, // Передаем orderId в качестве параметра
            success: function (response) {
                if (response && response.substituteSchema && response.substituteSchema.length > 0) {
                    substituteCache.set(orderId, response); // Сохраняем данные в кэш
                    displaySubstituteData(response);
                } else {
                    showToast("Применяемость не найдена")
                }
            },
            error: function (xhr, status, error) {
                console.error("Ошибка:", error);
                alert("Произошла ошибка при получении данных о заменах.");
            },
            complete: function () {
                if (loader) loader.style.display = 'none';
                linkElement.removeClass("disabled").prop("disabled", false);
            }
        });
    });

    // Функция отображения данных
    function displaySubstituteData(data) {
        const modal = new bootstrap.Modal($('#substituteModal'));
        modal.show();

        // Очищаем контейнер перед добавлением новых данных
        $('#substituteTableContainer').empty();

        let htmlContent = `<table class="table table-bordered">
                <thead>
                    <tr>
                        <th>Тип</th>
                        <th>Kатегории</th>
                        <th>Полное название модификации</th>
                    </tr>
                </thead>
                <tbody>`;

        data.substituteSchema.forEach(substitute => {
            htmlContent += `<tr>
                    <td>${substitute.type || ''}</td>
                    <td>${substitute.name || ''}</td>
                    <td>${substitute.modification?.description || ''} ${substitute.modification?.constructionInterval || ''}</td>
                </tr>`;

            if (Array.isArray(substitute.attributes) && substitute.attributes.length > 0) {
                const uniqueId = 'attributes-' + Math.random().toString(36).substr(2, 9); // Генерация уникального ID для этого элемента

                htmlContent += `<tr id="${uniqueId}">
                            <td colspan="3">
                                <table class="table table-sm table-striped">
                                    <thead>
                                        <tr>
                                            <th></th>
                                            <th></th>
                                        </tr>
                                    </thead>
                                    <tbody>`;

                // Добавляем первые три атрибута
                substitute.attributes.slice(0, 3).forEach(attribute => {
                    htmlContent += `<tr>
                                <td>${attribute.title || ''}</td>
                                <td>${attribute.value || ''}</td>
                            </tr>`;
                });

                // Добавляем скрытые атрибуты (начально скрыты)
                if (substitute.attributes.length > 3) {
                    htmlContent += `<tr class="additional-attributes" id="additional-${uniqueId}" style="display:none;">
                                <td colspan="2">
                                    <table class="table table-sm table-striped">`;

                    substitute.attributes.slice(3).forEach(attribute => {
                        htmlContent += `<tr>
                                    <td>${attribute.title || ''}</td>
                                    <td>${attribute.value || ''}</td>
                                </tr>`;
                    });

                    htmlContent += `</table></td></tr>`;
                }

                // Кнопка для отображения скрытых атрибутов
                if (substitute.attributes.length > 3) {
                    htmlContent += `<tr>
                                <td colspan="3">
                                    <button class="btn btn-link" id="toggle-btn-${uniqueId}" onclick="toggleAttributes('${uniqueId}')">Все</button>
                                </td>
                            </tr>`;
                }

                htmlContent += `</tbody></table>
                            </td>
                        </tr>`;
            }
        });

        htmlContent += `</tbody></table>`;

        $('#substituteTableContainer').html(htmlContent);
    }
});

function toggleAttributes(uniqueId) {
    const additionalAttributes = document.querySelector(`#additional-${uniqueId}`);
    const button = document.querySelector(`#toggle-btn-${uniqueId}`);

    // Переключаем видимость скрытых атрибутов
    if (additionalAttributes) {
        additionalAttributes.style.display = (additionalAttributes.style.display === 'none') ? '' : 'none';
    }

    // Изменяем текст кнопки в зависимости от состояния
    if (button) {
        button.textContent = (button.textContent === 'Все') ? 'Скрыть' : 'Все';
    }
}

function showToast(message) {
    const toastBody = document.getElementById('flashMessageText');
    toastBody.textContent = message;
    const toast = new bootstrap.Toast(document.getElementById('flashMessage'));
    toast.show();
}

$(document).on('click', '.stok-order', function () {
    const $button = $(this);
    const $svg = $button.find('svg');
    const indexRow = $button.data('index-row'); // получаем индекс строки

    const orderId = $button.data('order-id');
    const stokId = $button.data('stok-id');
    const supplierUrl = $button.data('supplier-url');
    selectSupplierByUrl(orderId, supplierUrl);

    const $priceElement =  $(`.price[data-order-id="${orderId}"][data-index-row="${indexRow}"]`);
    const $manufacturerElement =  $(`.manufacturer[data-order-id="${orderId}"][data-index-row="${indexRow}"]`);
    const $shortName = $(`.short-name[data-order-id="${orderId}"][data-index-row="${indexRow}"]`);
    const $supplier = $(`.supplier[data-order-id="${orderId}"][data-index-row="${indexRow}"]`);
    const $quantityElement = document.getElementById('Quantity_' + orderId);
    const $article = document.getElementById('thArticle2_' + orderId);

    var article = $article.textContent.trim();
    var quantityValue = $quantityElement.textContent.trim();
    var quantity = parseInt(quantityValue);
    var produser = $supplier.text().trim();
    var itemName = $shortName.attr("title");
    let manufacturer = $manufacturerElement.text().trim();
    let priceText = $priceElement.text().trim(); // например, "4.72 EUR"
    let priceValue = null;
    let currencyCode = null;

    if (priceText && priceText !== '-') {
        const parts = priceText.split(' ');
        priceValue = parseFloat(parts[0].replace(',', '.')); // числовая часть
        currencyCode = parts[1]; // валютный код
    }

    console.log("Цена:", priceValue);
    console.log("Валюта:", currencyCode);

    const item = { orderId, stokId, produser, priceValue, currencyCode, manufacturer, itemName, quantity, article, indexRow};

    // Получаем текущий список из sessionStorage
    let list = JSON.parse(sessionStorage.getItem('selectedStockList'+ids) || '[]');

    // Проверяем, выбрана ли уже эта кнопка
    const isAlreadySelected = list.some(x => x.orderId === orderId && x.stokId === stokId);

    if (!isAlreadySelected) {
        // Удаляем все элементы с тем же orderId
        list = list.filter(x => x.orderId !== orderId);

        // Снимаем выделение и иконку с других кнопок с тем же orderId
        $(`.stok-order[data-order-id="${orderId}"]`).each(function () {
            const $btn = $(this);
            if ($btn.hasClass('checked')) {
                $btn.find('svg').replaceWith(`
                    <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" fill="currentColor"
                         class="bi bi-cart" viewBox="0 0 18 18" style="background-color: transparent;">
                      <path d="M0 1.5A.5.5 0 0 1 .5 1H2a.5.5 0 0 1 .485.379L2.89 3H14.5a.5.5 0 0 1
                               .491.592l-1.5 8A.5.5 0 0 1 13 12H4a.5.5 0 0 1-.491-.408L2.01 3.607
                               1.61 2H.5a.5.5 0 0 1-.5-.5M3.102 4l1.313 7h8.17l1.313-7zM5
                               12a2 2 0 1 0 0 4 2 2 0 0 0 0-4m7 0a2 2 0 1 0 0 4
                               2 2 0 0 0 0-4m-7 1a1 1 0 1 1 0 2 1 1 0 0 1 0-2m7
                               0a1 1 0 1 1 0 2 1 1 0 0 1 0-2"/>
                    </svg>
                `);
                $btn.removeClass('checked');
            }
        });

        // Добавляем новый элемент
        list.push(item);

        // Меняем иконку на активную
        $svg.replaceWith(`
            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor"
                 class="bi bi-cart-check" viewBox="0 0 16 16" style="background-color: transparent;">
              <path d="M11.354 6.354a.5.5 0 0 0-.708-.708L8 8.293 6.854 7.146a.5.5 0 1 0-.708.708l1.5 1.5a.5.5 0 0 0 .708 0z"/>
              <path d="M.5 1a.5.5 0 0 0 0 1h1.11l.401 1.607 1.498 7.985A.5.5 0 0 0 4 12h1a2 2 0 1 0 0 4
                       2 2 0 0 0 0-4h7a2 2 0 1 0 0 4 2 2 0 0 0 0-4h1a.5.5 0 0 0 .491-.408l1.5-8A.5.5 0 0 0
                       14.5 3H2.89l-.405-1.621A.5.5 0 0 0 2 1zm3.915 10L3.102 4h10.796l-1.313 7zM6
                       14a1 1 0 1 1-2 0 1 1 0 0 1 2 0m7 0a1 1 0 1 1-2 0 1 1 0 0 1 2 0"/>
            </svg>
        `);
        $button.addClass('checked');
    } else {
        // Удаляем текущий элемент (отмена выбора)
        list = list.filter(x => !(x.orderId === orderId && x.stokId === stokId));

        $svg.replaceWith(`
            <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" fill="currentColor"
                 class="bi bi-cart" viewBox="0 0 18 18" style="background-color: transparent;">
              <path d="M0 1.5A.5.5 0 0 1 .5 1H2a.5.5 0 0 1 .485.379L2.89 3H14.5a.5.5 0 0 1
                       .491.592l-1.5 8A.5.5 0 0 1 13 12H4a.5.5 0 0 1-.491-.408L2.01 3.607
                       1.61 2H.5a.5.5 0 0 1-.5-.5M3.102 4l1.313 7h8.17l1.313-7zM5
                       12a2 2 0 1 0 0 4 2 2 0 0 0 0-4m7 0a2 2 0 1 0 0 4
                       2 2 0 0 0 0-4m-7 1a1 1 0 1 1 0 2 1 1 0 0 1 0-2m7
                       0a1 1 0 1 1 0 2 1 1 0 0 1 0-2"/>
            </svg>
        `);
        $button.removeClass('checked');
    }
    sessionStorage.setItem('selectedStockList'+ids, JSON.stringify(list));
});

// Основная функция для выбора поставщика по URL
function selectSupplierByUrl(orderId, supplierUrl) {
    // Находим select по orderId
    const $select = $(`#SupplierId_${orderId}`);

    if (!$select.length) {
        console.error(`Не найден select для orderId: ${orderId}`);
        return false;
    }

    // Ищем точное совпадение по URL
    const $exactMatch = $select.find(`option[data-site="${supplierUrl}"]`);

    if ($exactMatch.length) {
        $select
            .removeClass('highlight-error')
            .val($exactMatch.val())
            .trigger('change');
        console.log(`Выбран поставщик: ${$exactMatch.text()}`);
        return true;
    }

    // Если точного совпадения нет, ищем без учета регистра
    const $options = $select.find('option[data-site]');
    let foundMatch = null;

    $options.each(function() {
        const optionUrl = $(this).data('site') || '';
        if (optionUrl.trim().toLowerCase() === supplierUrl.trim().toLowerCase()) {
            foundMatch = $(this);
            return false; // Прерываем цикл
        }
    });

    if (foundMatch) {
        // Удаляем класс ошибки и устанавливаем найденного поставщика
        $select.removeClass('highlight-error')
            .val(foundMatch.val())
            .trigger('change');
        return true;
    }

    console.warn(`Не найден поставщик с URL: ${supplierUrl}`);
    return false;
}


$('#createOrderForm').on('submit', function (e) {
    const listJson = sessionStorage.getItem('selectedStockList'+ids) || '[]';
    sessionStorage.removeItem('selectedStockList'+ids);
    try {
        const parsedList = JSON.parse(listJson);
        if (Array.isArray(parsedList) && parsedList.length > 0) {
            e.preventDefault(); // предотвращаем отправку формы до завершения запроса

            fetch('SaveSelectedStockListToSession', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                body: listJson
            })
                .then(response => response.json())
                .then(data => {
                    if (data && data.failedOrderIds && Array.isArray(data.failedOrderIds)) {
                        data.failedOrderIds.forEach(function (orderId) {
                            if (!deletedOrders.includes(orderId)) {
                                deletedOrders.push(orderId);
                            }
                        });
                        $('#deletedOrders').val(deletedOrders.join(','));
                    }

                    // Удаляем из sessionStorage после успешной отправки


                    // Отправляем форму повторно
                    $('#createOrderForm')[0].submit();
                })
                .catch(err => {
                    console.warn('Ошибка при отправке списка на сервер', err);

                });

        }
    } catch (err) {
        console.warn('Ошибка при чтении selectedStockList из sessionStorage', err);
        // В случае ошибки парсинга — форма может быть отправлена, если это нужно
        // $('#createOrderForm')[0].submit(); // <- раскомментировать, если нужно
    }
});

document.getElementById('cartModal').addEventListener('shown.bs.modal', function () {
    printGroupedCartToModal();
});

function printGroupedCartToModal() {
    const data = JSON.parse(sessionStorage.getItem('selectedStockList'+ids) || '[]');
    const body = document.getElementById('cartModalBody');

    let html = '';
    let grandTotalPrice = {}; // Общая сумма цен
    let grandTotalSum = {};   // Общая сумма (цена × количество)

    if (data.length === 0) {
        html = `
        <table class="table table-sm table-bordered mb-2">
            <thead>
                <tr>
                    <th>Артикул</th>
                    <th>Название</th>
                    <th>Кол-во</th>
                    <th>Цена</th>
                    <th>Сумма</th>
                    <th>Валюта</th>
                    <th>Произв.</th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td colspan="7" class="text-center">Нет данных</td>
                </tr>
            </tbody>
        </table>
        `;
        body.innerHTML = html;
        return;
    }

    const grouped = {};

    data.forEach(item => {
        const producer = item.produser?.trim() || '-';
        const currency = item.currencyCode?.trim() || '-';
        const quantity = item.quantity || 1;
        const price = item.priceValue || 0;
        const sum = quantity * price;

        // Инициализация общих сумм
        if (!grandTotalPrice[currency]) grandTotalPrice[currency] = 0;
        if (!grandTotalSum[currency]) grandTotalSum[currency] = 0;

        grandTotalPrice[currency] += price;
        grandTotalSum[currency] += sum;

        if (!grouped[producer]) {
            grouped[producer] = {
                items: [],
                totalPrice: {}, // Сумма цен по поставщику
                totalSum: {}   // Сумма (цена × количество) по поставщику
            };
        }

        grouped[producer].items.push({
            stokId: item.stokId,
            priceValue: price,
            currency: currency,
            orderId: item.orderId,
            manufacturer: item.manufacturer,
            itemName: item.itemName,
            article: item.article || '-',
            quantity: quantity,
            sum: sum
        });

        if (!grouped[producer].totalPrice[currency]) grouped[producer].totalPrice[currency] = 0;
        if (!grouped[producer].totalSum[currency]) grouped[producer].totalSum[currency] = 0;

        grouped[producer].totalPrice[currency] += price;
        grouped[producer].totalSum[currency] += sum;
    });

    for (const [producer, group] of Object.entries(grouped)) {
        html += `<b>🛒 Поставщик: ${producer}</b><br>`;
        html += `
        <table class="table table-sm table-bordered mb-2">
            <thead>
                <tr>
                    <th>Артикул</th>
                    <th>Название</th>
                    <th>Кол-во</th>
                    <th>Цена</th>
                    <th>Сумма</th>
                    <th>Валюта</th>
                    <th>Произв.</th>
                </tr>
            </thead>
            <tbody>
        `;

        group.items.forEach(item => {
            html += `
                <tr>
                    <td>${item.article}</td>
                    <td>${item.itemName}</td>
                    <td>${item.quantity}</td>
                    <td>${item.priceValue.toFixed(2)}</td>
                    <td>${item.sum.toFixed(2)}</td>
                    <td>${item.currency}</td>
                    <td>${item.manufacturer}</td>
                </tr>
            `;
        });

        html += `</tbody></table>`;

        // Вывод итогов в одну строку с табами
        html += `<div class="d-flex">`;
        html += `<div class="me-3"><b>Итого цена:\t`;

        for (const [currency, total] of Object.entries(group.totalPrice)) {
            html += `${total.toFixed(2)} ${currency}\t`;
        }

        html += `</b></div>`;

        html += `<div><b>Итого сумма:\t`;
        for (const [currency, total] of Object.entries(group.totalSum)) {
            html += `${total.toFixed(2)} ${currency}\t`;
        }
        html += `</b></div>`;
        html += `</div><hr>`;
    }

    // Общая сумма в одну строку с табами
    html += `<div class="d-flex mt-3">`;
    html += `<div class="me-3"><b>Общая цена:\t`;

    for (const [currency, total] of Object.entries(grandTotalPrice)) {
        html += `${total.toFixed(2)} ${currency}\t`;
    }

    html += `</b></div>`;

    html += `<div><b>Общая сумма:\t`;
    for (const [currency, total] of Object.entries(grandTotalSum)) {
        html += `${total.toFixed(2)} ${currency}\t`;
    }
    html += `</b></div>`;
    html += `</div>`;

    body.innerHTML = html;
}

function setPurchasePrice(orderId, price, index, itemId) {
    console.log(`Установка цены для orderId ${orderId}, index ${index}: ${price}`);
    try {
        // Установка цены
        const input = document.querySelector(`#PurchasePrice_${orderId}`);
        if (input) {
            const formattedPrice = price.toString().replace(',', '.');
            input.value = formattedPrice;
            const inputEvent = new Event('input', { bubbles: true });
            input.dispatchEvent(inputEvent);
        }
        updateAllPricesForOrder(orderId)

        const tableRow = document.querySelector(`tr[data-order-id="${orderId}"][data-item-id="${itemId}"]`);

        // --- Логика кеширования и переключения подсветки ---
        window.highlightedItem = window.highlightedItem || {};
        const isCurrentlyHighlighted = tableRow && tableRow.classList.contains('table-warning');

        // Снимаем подсветку со всех строк и элементов для данного orderId
        const allRowsWithSameOrderId = document.querySelectorAll(`tr[data-order-id="${orderId}"]`);
        allRowsWithSameOrderId.forEach(row => row.classList.remove('table-warning'));

        const allButtonsWithSameOrderId = document.querySelectorAll(`
                .stok-order[data-order-id="${orderId}"],
                .price[data-order-id="${orderId}"]
            `);
        allButtonsWithSameOrderId.forEach(button => button.classList.remove('highlight-button'));

        // Если строка была подсвечена, то снимаем подсветку и очищаем кеш.
        if (isCurrentlyHighlighted) {
            delete window.highlightedItem[orderId];
            return;
        }

        // Если не была, то подсвечиваем ее и сохраняем itemId в кеш
        if (tableRow) {
            tableRow.classList.add('table-warning');
            window.highlightedItem[orderId] = itemId;
        }

        const clickedButton = document.querySelector(`
            .stok-order[data-order-id="${orderId}"][data-index-row="${index}"],
            .price[data-order-id="${orderId}"][data-index-row="${index}"]
        `);
        if (clickedButton) {
            clickedButton.classList.add('highlight-button');
            setTimeout(() => {
                clickedButton.classList.remove('highlight-button');
            }, 1000);
        }

    } catch (error) {
        console.error('Error in setPurchasePrice:', error);
    }
}

document.addEventListener("DOMContentLoaded", function () {
    const chatToggle = document.getElementById("chatToggle");

    // Заменяем атрибуты
    chatToggle.setAttribute("data-bs-toggle", "modal");
    chatToggle.setAttribute("data-bs-target", "#cartModal");
    chatToggle.setAttribute("aria-expanded", "false");
    chatToggle.setAttribute("title", "Открыть корзину");
    chatToggle.classList.add("shadow-none");

    // Очищаем старое содержимое
    chatToggle.innerHTML = `
            <svg xmlns="http://www.w3.org/2000/svg"
                 width="20"
                 height="20"
                 fill="currentColor"
                 class="bi bi-cart"
                 viewBox="0 0 16 16">
                <path d="M0 1.5A.5.5 0 0 1 .5 1H2a.5.5 0 0 1 .485.379L2.89 3H14.5a.5.5 0 0 1 .491.592l-1.5 8A.5.5 0 0 1 13 12H4a.5.5 0 0 1-.491-.408L2.01 3.607 1.61 2H.5a.5.5 0 0 1-.5-.5M3.102 4l1.313 7h8.17l1.313-7zM5 12a2 2 0 1 0 0 4 2 2 0 0 0 0-4m7 0a2 2 0 1 0 0 4 2 2 0 0 0 0-4m-7 1a1 1 0 1 1 0 2 1 1 0 0 1 0-2m7 0a1 1 0 1 1 0 2 1 1 0 0 1 0-2"/>
            </svg>
            <span id="cartBadge" class="d-none badge bg-danger position-absolute top-0 start-100 translate-middle p-1 border border-light rounded-circle">
                <span class="visually-hidden">новые товары</span>
            </span>
        `;
});
document.addEventListener('click', function (e) {
    if (e.target.classList.contains('weight-value')) {
        let weight = e.target.getAttribute('data-weight') || '';
        const orderId = e.target.getAttribute('data-order-id');
        const weightInput = document.querySelector(`#ProductWeight_${orderId}`);

        if (weightInput) {
            // Очистка строки: убираем "кг" и пробелы
            weight = weight.replace(/кг/gi, '').trim();

            // Заменяем запятые на точки
            weight = weight.replace(/,/g, '.');

            // Берём первое число из строки (если несколько значений через запятую/пробел)
            const match = weight.match(/[\d.]+/g);
            const firstWeight = match ? match[0] : null;

            if (firstWeight) {
                // Форматируем вес (parseFloat убирает лишние нули)
                const formattedWeight = parseFloat(firstWeight).toString();

                // Устанавливаем значение
                weightInput.value = formattedWeight;

                // Триггерим события
                const eventsToTrigger = ['input', 'change', 'blur'];
                eventsToTrigger.forEach(eventType => {
                    const event = new Event(eventType, { bubbles: true, cancelable: true });
                    weightInput.dispatchEvent(event);
                });

                // Для jQuery (если используется)
                if (typeof jQuery !== 'undefined') {
                    $(weightInput).trigger('input').trigger('change');
                }

                // Подсвечиваем поле на 1 секунду
                weightInput.style.backgroundColor = '#e6ffe6';
                setTimeout(() => {
                    weightInput.style.backgroundColor = '';
                }, 1000);

                // Фокусируем поле ввода
                weightInput.focus();
            } else {
                console.warn(`Не удалось распарсить вес: "${weight}"`);
            }

            sortAndRerenderTable(orderId, 'costPrice', 'number');
        } else {
            console.warn(`Не найдено поле ввода веса для orderId: ${orderId}`);
        }
    }
});

const rates = {
    'RUB': 1,
    'USD': parseFloat(CurrencyRates.USD.replace(',', '.')),
    'EUR': parseFloat(CurrencyRates.EUR.replace(',', '.')),
    'BYN': parseFloat(CurrencyRates.BYN.replace(',', '.'))
};
function updatePriceDisplay(orderId, indexRow) {
    const $priceLink = $(`.price-link[data-order-id="${orderId}"][data-index-row="${indexRow}"]`);
    const $priceBlock = $(`.cost-price-row[data-order-id="${orderId}"][data-index-row="${indexRow}"]`);
    const $weightInput = $(`#ProductWeight_${orderId}`);
    const $supplierSelect = $(`#SupplierId_${orderId}`);
    const selectedSupplier = $supplierSelect.find('option:selected');


    const weight = parseFloat(($weightInput.val() || '0').replace(',', '.')) || 0;

    const priceSite = $priceLink.data('site');
    const supplierOptionBySite = $supplierSelect.find(`option[data-site="${priceSite}"]`);
    const weightFactor = parseFloat((supplierOptionBySite.data('weight-factor') || '0').toString().replace(',', '.')) || 0;
    const costFactor = parseFloat((supplierOptionBySite.data('cost-factor') || '1').toString().replace(',', '.')) || 1;
    const weightCurrency = supplierOptionBySite.data('weight-currency-code') || 'RUB';

    const rawPrice = parseFloat(($priceLink.data('price') || '0').toString().replace(',', '.')) || 0;
    const priceCurrency = $priceLink.data('currency-code') || 'RUB';

    const ratePrice = rates[priceCurrency] || 1;
    const rateWeight = rates[weightCurrency] || 1;

    const pricePerKgRUB = weightFactor * rateWeight;
    const totalWeightCost = weight * pricePerKgRUB;
    const purchasePriceRUB = rawPrice * costFactor * ratePrice;
    const totalRUB = Math.round(totalWeightCost + purchasePriceRUB);

    const originalDisplay = `${rawPrice.toFixed(2)} ${priceCurrency}`;
    const formattedTotalRUB = totalRUB.toLocaleString('ru-RU');
    const displayText = `${formattedTotalRUB} ₽`;

    $priceBlock.text(displayText);
    $priceBlock.attr('data-value', totalRUB);

    if (window.tradesofDataCache && window.tradesofDataCache[orderId] && window.tradesofDataCache[orderId][indexRow]) {
        window.tradesofDataCache[orderId][indexRow].costPrice = totalRUB;
        window.tradesofDataCache[orderId][indexRow].costPriceFormatted = displayText;
    }

    const title = `${weight.toFixed(0)} (Вес) * ${pricePerKgRUB.toFixed(4)} ` +
        `(Цена за кг. RUB = ${weightFactor.toFixed(2)} * ${rateWeight.toFixed(4)}) + ` +
        `${rawPrice.toFixed(2)} ${priceCurrency} * ${ratePrice.toFixed(4)} * ${costFactor.toFixed(4)} (Коэфф.) = ` +
        `${purchasePriceRUB.toFixed(13)} (Цена закупки RUB) = ${totalRUB} RUB`;

    $priceBlock.attr('title', title);
}

function updateAllPricesForOrder(orderId) {
    $(`.price-link[data-order-id="${orderId}"]`).each(function () {
        const indexRow = $(this).data('index-row');
        updatePriceDisplay(orderId, indexRow);
    });
}

$('.product-weight-field').on('input', function () {
    const orderId = $(this).attr('id').split('_')[1];
    updateAllPricesForOrder(orderId);
    sortAndRerenderTable(orderId, 'costPrice', 'number', 'asc')
});

$('.supplier-select').on('change', function () {
    const orderId = $(this).attr('id').split('_')[1];
    updateAllPricesForOrder(orderId);
});

let regionsLoaded = false;

$('#regionSelect').one('focus', function () {
    if (regionsLoaded) return;

    $.ajax({
        url: '/Tradesoft/GetRegions',
        type: 'GET'
    }).done(function (response) {
        if (response && response.error === "") {
            const regions = response.table;
            const select = $('#regionSelect');

            // Удаляем все, кроме первой опции (по умолчанию)
            select.find('option').not(':first').remove();

            regions.forEach(region => {
                if (region.code_region !== 1) { // 1 уже добавлен
                    const option = $('<option>', {
                        value: region.code_region,
                        text: region.class_region
                    });
                    select.append(option);
                }
            });

            regionsLoaded = true;
        } else {
            console.error('Ошибка получения регионов:', response.error || 'Неизвестная ошибка');
        }
    }).fail(function (error) {
        console.error('Ошибка при запросе регионов:', error);
    });
});
document.getElementById("createOrderForm").addEventListener("keydown", function (event) {
    if (event.key === "Enter") {
        event.preventDefault(); // отменяет стандартное действие
        return false;           // дополнительная защита
    }
});

function toggleComment(e) {
    e.preventDefault(); // предотвращает переход по ссылке
    const container = document.getElementById('commentContainer');
    const link = e.target;
    const isVisible = container.style.display === 'block';

    container.style.display = isVisible ? 'none' : 'block';
    link.textContent = isVisible ? 'Добавить комментарий' : 'Скрыть комментарий';
}


$(document).ready(function(){
    $('[data-bs-toggle="tooltip"]').tooltip({
        placement: 'right', // Показывать справа от элемента
        trigger: 'hover',   // Показывать при наведении
        container: 'body'   // Чтобы tooltip не обрезался контейнером
    });
});