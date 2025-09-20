// Глобальный конфигурационный объект для передачи данных из Razor в JS
window.ozonOrdersConfig = window.ozonOrdersConfig || {};

// Вспомогательная функция для извлечения числа из текста
function extractNumber(text) {
    if (typeof text !== 'string' && typeof text !== 'number') {
        text = String(text); // Преобразуем в строку, если это не строка и не число
    } else if (typeof text === 'number') {
        return text; // Уже число, возвращаем как есть
    }
    if (!text) return null; // Если текст пустой или null, возвращаем null
    // Заменяем все пробелы на пустоту и запятую на точку, затем парсим как float
    const number = parseFloat(text.replace(/\s/g, '').replace(',', '.'));
    return isNaN(number) ? null : number; // Если результат не число, возвращаем null, иначе само число
}

// Выполняется, когда DOM полностью загружен
$(document).ready(function () {

    // --- Удаление строк и нумерация ---
    function updateRowNumbers() {
        // Обновляем номера для всех видимых строк в теле таблицы
        $('#shipmentTable tbody tr:visible').each(function (index) {
            $(this).find('.row-number').text(index + 1); // Устанавливаем порядковый номер
        });
    }
    // Первичная нумерация строк при загрузке
    updateRowNumbers();

    // Обработчик для кнопок удаления строк (если такие кнопки будут добавлены с классом 'delete-order')
    $('#shipmentTable').on('click', '.delete-order', function () {
        var orderId = $(this).data('order-id'); // Получаем ID заказа из data-атрибута
        $('#orderRow_' + orderId).remove(); // Удаляем строку таблицы
        // Или можно так: $(this).closest('tr').remove();
        updateRowNumbers(); // Обновляем нумерацию
        if (window.updateSupplierList) { // Проверяем, существует ли функция обновления списка поставщиков
            window.updateSupplierList(); // Вызываем обновление списка
        }
    });


    // --- Обработка выбора статусов ---
    // Функция для подсветки поля выбора статуса
    function checkStatusField(select) {
        const selectedText = select.options[select.selectedIndex].text.trim(); // Текст выбранной опции
        select.classList.remove('highlight-null', 'highlight-success'); // Снимаем предыдущие подсветки
        if (selectedText === "Заказан поставщику") {
            select.classList.add('highlight-success'); // Подсвечиваем успешный статус
        } else if (selectedText === "Отменен") {
            select.classList.add('highlight-null'); // Подсвечиваем отмененный статус
        }
    }

    // Обработчик для универсального (общего) селекта статусов
    $('#statusSelect').on('change', function () {
        const selectElement = this;
        if (selectElement.value === "create") { // Если выбрана опция "Создать"
            const createUrl = $(selectElement).find('option[value="create"]').data('url'); // Получаем URL из data-атрибута
            if (createUrl) {
                window.location.href = createUrl; // Переходим по URL
            }
        } else {
            var selectedStatus = selectElement.value; // Значение выбранного статуса
            // Применяем выбранный статус ко всем индивидуальным селектам статусов в таблице
            document.querySelectorAll('.status-select').forEach(function (statusSelect) {
                statusSelect.value = selectedStatus;
                checkStatusField(statusSelect); // Применяем подсветку
                $(statusSelect).trigger('change'); // Генерируем событие 'change' для других возможных обработчиков
            });
        }
    });

    // Обработчик для индивидуальных селектов статусов в таблице
    $('#shipmentTable').on('change', '.status-select', function () {
        checkStatusField(this); // Применяем подсветку при изменении
    });

    // Первичная проверка и подсветка всех селектов статусов при загрузке страницы
    document.querySelectorAll('.status-select').forEach(select => {
        checkStatusField(select);
    });


    // --- Обработка выбора поставщиков ---
    // Функция для подсветки поля выбора поставщика (ошибка, если не выбран)
    function checkSupplierField(select) {
        const value = select.value.trim().toLowerCase(); // Значение выбранной опции
        const selectedText = select.options[select.selectedIndex].text; // Текст выбранной опции
        // Если значение пустое или текст начинается с "Не указан" или "Выберите"
        if (value === "" || selectedText.startsWith('Не указан') || selectedText.startsWith('Выберите')) {
            select.classList.add('highlight-error'); // Подсвечиваем как ошибку
        } else {
            select.classList.remove('highlight-error'); // Снимаем подсветку ошибки
        }
    }

    // Обработчик для универсального (общего) селекта поставщиков
    $('#universalSelect').on('change', function () {
        const selectElement = this;
        if (selectElement.value === "create") { // Если выбрана опция "Создать"
            const createUrl = $(selectElement).find('option[value="create"]').data('url'); // Получаем URL из data-атрибута
            if (createUrl) {
                window.location.href = createUrl; // Переходим по URL
            }
        } else {
            var selectedOption = selectElement.options[selectElement.selectedIndex];
            var supplierId = selectedOption.value; // ID выбранного поставщика
            // Применяем выбранного поставщика ко всем индивидуальным селектам в таблице
            document.querySelectorAll('.supplier-select').forEach(function (supplierSelect) {
                supplierSelect.value = supplierId;
                $(supplierSelect).trigger('change'); // Важно для запуска других обработчиков (например, расчета цен)
                checkSupplierField(supplierSelect); // Применяем подсветку
            });
        }
    });

    // Обработчик для индивидуальных селектов поставщиков в таблице
    $('#shipmentTable').on('change', '.supplier-select', function () {
        checkSupplierField(this); // Применяем подсветку при изменении
        // Логика обновления кода валюты теперь часть секции расчета цен
    });

    // Первичная проверка и подсветка всех селектов поставщиков при загрузке страницы
    document.querySelectorAll('.supplier-select').forEach(select => {
        checkSupplierField(select);
    });

    // --- Подсветка поля ввода закупочной цены (ошибка, если пустое) ---
    $('#shipmentTable').on('input', '.purchase-price-field', function () {
        if (this.value.trim() === "") { // Если поле пустое
            this.classList.add('highlight-error'); // Подсвечиваем как ошибку
        } else {
            this.classList.remove('highlight-error'); // Снимаем подсветку
        }
    });
    // Первичная проверка полей закупочной цены при загрузке
    document.querySelectorAll('.purchase-price-field').forEach(field => {
        if (field.value.trim() === "") {
            field.classList.add('highlight-error');
        }
    });


    // --- Сводная таблица по выбранным поставщикам ---
    const selectedSuppliers = {}; // Объект для хранения данных о выбранных поставщиках
    // Функция обновления сводной таблицы
    function updateSelectedSuppliersList() {
        // Очищаем текущее состояние (обнуляем счетчики для каждого поставщика)
        for (const key in selectedSuppliers) {
            if (selectedSuppliers.hasOwnProperty(key)) {
                selectedSuppliers[key] = { count: 0, totalAmount: 0, totalQuantity: 0, totalPurchasePrice: 0 };
            }
        }

        // Пересчитываем значения для видимых строк таблицы заказов
        document.querySelectorAll('#shipmentTable tbody tr').forEach((row) => {
            if (row.style.display === 'none') return; // Пропускаем скрытые строки (если используется фильтрация)

            const supplierSelect = row.querySelector('.supplier-select'); // Находим селект поставщика в строке
            if (!supplierSelect || !supplierSelect.value) return; // Пропускаем, если поставщик не выбран

            const orderId = supplierSelect.id.split('_')[1]; // Получаем ID заказа из ID селекта
            // Получаем текстовые значения из ячеек и преобразуем их в числа
            const shipmentAmountText = row.querySelector(`#shipmentAmount_${orderId}`)?.textContent || '0';
            const quantityText = row.querySelector(`#Quantity_${orderId}`)?.textContent || '0';
            const purchasePriceRubText = row.querySelector(`#PurchasePriceRub_${orderId}`)?.textContent || '0';

            const shipmentAmount = extractNumber(shipmentAmountText);
            const quantity = extractNumber(quantityText);
            const purchasePrice = extractNumber(purchasePriceRubText);

            const selectedOption = supplierSelect.options[supplierSelect.selectedIndex];
            const supplierName = selectedOption.text; // Имя поставщика

            // Если поставщика еще нет в нашем объекте, инициализируем его
            if (!selectedSuppliers[supplierName]) {
                selectedSuppliers[supplierName] = { count: 0, totalAmount: 0, totalQuantity: 0, totalPurchasePrice: 0 };
            }
            // Обновляем счетчики
            selectedSuppliers[supplierName].count++;
            selectedSuppliers[supplierName].totalAmount += (shipmentAmount || 0);
            selectedSuppliers[supplierName].totalQuantity += (quantity || 0);
            selectedSuppliers[supplierName].totalPurchasePrice += (purchasePrice || 0);
        });

        const selectedSuppliersListDiv = document.getElementById('selectedSuppliersList');
        selectedSuppliersListDiv.innerHTML = ''; // Очищаем предыдущую таблицу

        // Если нет выбранных поставщиков или у всех счетчики по нулям, не рисуем таблицу
        if (Object.keys(selectedSuppliers).length === 0 || Object.values(selectedSuppliers).every(s => s.count === 0)) {
            return;
        }

        // Создаем HTML-таблицу для сводки
        const table = document.createElement('table');
        table.classList.add('table', 'table-striped', 'table-sm', 'mt-3'); // Bootstrap классы

        const thead = document.createElement('thead');
        const headerRow = document.createElement('tr');
        ['Поставщик', 'Заказы у поставщика', 'Количество товаров', 'Сумма отправления', 'Цена закупки RUB'].forEach(headerText => {
            const th = document.createElement('th');
            th.textContent = headerText;
            headerRow.appendChild(th);
        });
        thead.appendChild(headerRow);
        table.appendChild(thead);

        const tbody = document.createElement('tbody');
        for (const [supplier, data] of Object.entries(selectedSuppliers)) {
            if (data.count > 0) { // Добавляем строку, только если есть заказы для этого поставщика
                const row = document.createElement('tr');
                row.insertCell().textContent = supplier;
                row.insertCell().textContent = data.count;
                row.insertCell().textContent = data.totalQuantity;
                row.insertCell().textContent = data.totalAmount.toFixed(2);
                row.insertCell().textContent = data.totalPurchasePrice.toFixed(2);
                tbody.appendChild(row);
            }
        }
        table.appendChild(tbody);
        selectedSuppliersListDiv.appendChild(table); // Добавляем таблицу на страницу
    }
    window.updateSupplierList = updateSelectedSuppliersList; // Делаем функцию глобально доступной

    // Первичный вызов и привязка к событиям изменения в таблице
    $('#shipmentTable').on('change', '.supplier-select, .purchase-price-field', function() {
        updateSelectedSuppliersList();
    });
    updateSelectedSuppliersList(); // Вызываем при загрузке


    // --- Динамические расчеты цен/себестоимости/прибыли ---
    // Убеждаемся, что курсы валют доступны (должны быть установлены в Razor представлении)
    const rateUSD = parseFloat(window.ozonOrdersConfig.rateUSD) || 0;
    const rateEUR = parseFloat(window.ozonOrdersConfig.rateEUR) || 0;
    const rateBYN = parseFloat(window.ozonOrdersConfig.rateBYN) || 0;

    // Функция обновления закупочной цены в рублях
    function updatePurchasePriceInRub(orderId, currencyCode, costFactor) {
        const purchasePriceField = $(`#PurchasePrice_${orderId}`);
        const purchasePrice = extractNumber(purchasePriceField.val()); // Получаем значение из поля ввода
        let purchasePriceInRub = 0;
        let titleText = ''; // Текст для всплывающей подсказки

        if (purchasePrice === null) { // Если цена не введена
            $('#PurchasePriceRub_' + orderId).text('').attr('title', '');
            purchasePriceField.data('purchase-price-rub', null);
            return null;
        }

        // Расчет в зависимости от валюты
        switch (currencyCode) {
            case 'USD':
                purchasePriceInRub = purchasePrice * costFactor * rateUSD;
                titleText = `${purchasePrice} (Цена закупки ${currencyCode}) * ${costFactor} (Коэфф.) * ${rateUSD} (Курс ${currencyCode}) = ${purchasePriceInRub.toFixed(2)}`;
                break;
            case 'EUR':
                purchasePriceInRub = purchasePrice * costFactor * rateEUR;
                titleText = `${purchasePrice} (Цена закупки ${currencyCode}) * ${costFactor} (Коэфф.) * ${rateEUR} (Курс ${currencyCode}) = ${purchasePriceInRub.toFixed(2)}`;
                break;
            case 'BYN':
                purchasePriceInRub = purchasePrice * costFactor * rateBYN;
                titleText = `${purchasePrice} (Цена закупки ${currencyCode}) * ${costFactor} (Коэфф.) * ${rateBYN} (Курс ${currencyCode}) = ${purchasePriceInRub.toFixed(2)}`;
                break;
            default: // Предполагаем RUB или не указана валюта
                purchasePriceInRub = purchasePrice * costFactor;
                titleText = `${purchasePrice} (Цена закупки RUB) * ${costFactor} (Коэфф.) = ${purchasePriceInRub.toFixed(2)}`;
                break;
        }

        purchasePriceField.attr('title', titleText); // Устанавливаем подсказку для поля ввода
        // Отображаем рублевую цену и устанавливаем подсказку для этого элемента
        $(`#PurchasePriceRub_${orderId}`).text(!isNaN(purchasePriceInRub) ? Math.round(purchasePriceInRub) + ' RUB' : '').attr('title', titleText);
        purchasePriceField.data('purchase-price-rub', purchasePriceInRub); // Сохраняем рублевую цену в data-атрибуте
        return purchasePriceInRub;
    }

    // Функция обновления себестоимости
    function updateCostPrice(orderId, currencyCode, costFactor, weightCurrencyCode, purchasePriceInRub) {
        const weight = extractNumber($(`#ProductWeight_${orderId}`).val()); // Вес товара
        const supplierSelect = $(`#SupplierId_${orderId}`);
        // Весовой коэффициент поставщика (цена за кг доставки)
        const supplierWeightFactor = extractNumber(supplierSelect.find('option:selected').data('weight-factor')) || 0;
        let supplierWeightFactorInRub = 0;
        let supplierWeightFactorInRubTitle = ``; // Для подсказки

        if (purchasePriceInRub === null) { // Если закупочная цена не рассчитана
            $('#CostPrice_' + orderId).text('').attr('title', '').data('cost-price', null);
            updateCalculations(orderId); // Распространяем null дальше
            return;
        }

        // Конвертируем весовой коэффициент в рубли, если он в другой валюте
        switch (weightCurrencyCode) {
            case 'USD':
                supplierWeightFactorInRub = supplierWeightFactor * rateUSD;
                supplierWeightFactorInRubTitle = ` = ${supplierWeightFactor.toFixed(2)} ${weightCurrencyCode} * ${rateUSD.toFixed(2)} (Курс)`;
                break;
            case 'EUR':
                supplierWeightFactorInRub = supplierWeightFactor * rateEUR;
                supplierWeightFactorInRubTitle = ` = ${supplierWeightFactor.toFixed(2)} ${weightCurrencyCode} * ${rateEUR.toFixed(2)} (Курс)`;
                break;
            case 'BYN':
                supplierWeightFactorInRub = supplierWeightFactor * rateBYN;
                supplierWeightFactorInRubTitle = ` = ${supplierWeightFactor.toFixed(2)} ${weightCurrencyCode} * ${rateBYN.toFixed(2)} (Курс)`;
                break;
            default: // Предполагаем RUB
                supplierWeightFactorInRub = supplierWeightFactor;
                break;
        }

        let costPrice = 0; // Себестоимость
        let titleText = ''; // Подсказка

        // Рассчитываем себестоимость: (вес * цена за кг доставки) + закупочная цена в рублях
        if (weight !== null && supplierWeightFactor !== 0 && supplierWeightFactorInRub !== 0) {
            costPrice = (weight * supplierWeightFactorInRub) + purchasePriceInRub;
            titleText = `${weight.toFixed(2)} (Вес) * ${supplierWeightFactorInRub.toFixed(2)} (Цена за кг RUB${supplierWeightFactorInRubTitle}) + ${purchasePriceInRub.toFixed(2)} (Закупка RUB) = ${costPrice.toFixed(2)}`;
        } else { // Если вес или цена за кг не указаны, себестоимость равна закупочной цене
            costPrice = purchasePriceInRub;
            titleText = `${purchasePriceInRub.toFixed(2)} (Закупка RUB) = ${costPrice.toFixed(2)}`;
        }

        // Отображаем себестоимость и сохраняем в data-атрибуте
        $(`#CostPrice_${orderId}`).text(Math.round(costPrice) + ' RUB').attr('title', titleText).data('cost-price', costPrice);
        updateCalculations(orderId); // Вызываем обновление прибыли и наценки
    }

    // Функция обновления расчетов прибыли и наценки
    function updateCalculations(orderId) {
        const costPrice = extractNumber($(`#CostPrice_${orderId}`).data('cost-price')); // Себестоимость из data-атрибута
        const price = extractNumber($(`#Price_${orderId}`).text()); // Цена продажи из ячейки

        // Если себестоимость или цена не определены, очищаем поля прибыли/наценки
        if (costPrice === null || costPrice === 0 || price === null) {
            $('#MinProfit_' + orderId).text('').attr('title', '');
            $('#MaxProfit_' + orderId).text('').attr('title', '');
            $('#MaxDiscount_' + orderId).text('').attr('title', '');
            $('#MinDiscount_' + orderId).text('').attr('title', '');
            return;
        }

        // Получаем значения комиссий Ozon (из поля ввода или из текста, если поле readonly)
        let minOzonCommission = extractNumber($(`#MinOzonCommission_${orderId}`).val() || $(`#MinOzonCommission_${orderId}`).text());
        let maxOzonCommission = extractNumber($(`#MaxOzonCommission_${orderId}`).val() || $(`#MaxOzonCommission_${orderId}`).text());

        // Если поля ввода комиссий пустые (вернут null), считаем комиссию равной 0
        minOzonCommission = minOzonCommission === null ? 0 : minOzonCommission;
        maxOzonCommission = maxOzonCommission === null ? 0 : maxOzonCommission;

        // Расчеты
        const startCommission = price - costPrice; // "Грязная" прибыль до вычета комиссии Ozon
        let minProfit = startCommission - maxOzonCommission; // Минимальная прибыль (при максимальной комиссии)
        let maxProfit = startCommission - minOzonCommission; // Максимальная прибыль (при минимальной комиссии)
        // Наценка в % (если себестоимость не 0)
        let minDiscount = (costPrice !== 0) ? (minProfit / costPrice) * 100 : 0;
        let maxDiscount = (costPrice !== 0) ? (maxProfit / costPrice) * 100 : 0;

        // Округляем до целого
        minProfit = Math.floor(minProfit);
        maxProfit = Math.floor(maxProfit);
        minDiscount = Math.floor(minDiscount);
        maxDiscount = Math.floor(maxDiscount);

        // Функция для форматирования отображаемого значения (обрезка длинных чисел)
        function formatDisplay(value, suffix = '') {
            if (isNaN(value)) return ""; // Если не число, возвращаем пусто
            let s_val = value.toString();
            // Если строка длиннее 6 символов, обрезаем и добавляем "..."
            return s_val.length > 6 ? s_val.slice(0,6) + "..." : s_val + suffix;
        }

        // Отображаем рассчитанные значения и устанавливаем цвет (красный для отрицательных)
        $('#MinProfit_' + orderId).text(formatDisplay(minProfit)).css('color', minProfit < 0 ? 'red' : '');
        $('#MaxProfit_' + orderId).text(formatDisplay(maxProfit)).css('color', maxProfit < 0 ? 'red' : '');
        $('#MinDiscount_' + orderId).text(formatDisplay(minDiscount, ' %')).css('color', minDiscount < 0 ? 'red' : '');
        $('#MaxDiscount_' + orderId).text(formatDisplay(maxDiscount, ' %')).css('color', maxDiscount < 0 ? 'red' : '');

        // Устанавливаем всплывающие подсказки с формулами расчета
        $('#MinProfit_' + orderId).attr('title', `${price.toFixed(2)} (Цена) - ${costPrice.toFixed(2)} (Себест.) - ${maxOzonCommission.toFixed(2)} (Макс.Ком.) = ${minProfit.toFixed(2)}`);
        $('#MaxProfit_' + orderId).attr('title', `${price.toFixed(2)} (Цена) - ${costPrice.toFixed(2)} (Себест.) - ${minOzonCommission.toFixed(2)} (Мин.Ком.) = ${maxProfit.toFixed(2)}`);
        $('#MinDiscount_' + orderId).attr('title', `${minProfit.toFixed(2)} (Мин.Приб.) / ${costPrice.toFixed(2)} (Себест.) * 100 = ${minDiscount.toFixed(2)}%`);
        $('#MaxDiscount_' + orderId).attr('title', `${maxProfit.toFixed(2)} (Макс.Приб.) / ${costPrice.toFixed(2)} (Себест.) * 100 = ${maxDiscount.toFixed(2)}%`);
    }

    // Обработчики событий для запуска расчетов цен/себестоимости/прибыли
    // При изменении поставщика
    $('#shipmentTable').on('change', '.supplier-select', function () {
        const orderId = this.id.split('_')[1]; // ID заказа
        const selectedSupplier = $(this).find('option:selected'); // Выбранная опция поставщика
        const currencyCode = selectedSupplier.data('currency-code'); // Валюта поставщика
        const weightFactor = parseFloat(selectedSupplier.data('weight-factor')) || 0; // Весовой коэффициент
        const weightCurrencyCode = selectedSupplier.data('weight-currency-code'); // Валюта весового коэффициента
        const costFactor = extractNumber(selectedSupplier.data('cost-factor')) || 1; // Общий коэффициент затрат
        const weightInput = $(`#ProductWeight_${orderId}`); // Поле ввода веса

        // Если весовой коэффициент 0, делаем поле веса readonly и устанавливаем значение 0
        if (weightFactor === 0) {
            weightInput.val('0').prop('readonly', true);
        } else {
            weightInput.prop('readonly', false); // Иначе делаем редактируемым
        }

        // Пример обновления отображения кода валюты (если есть соответствующий span)
        // $(`#CurrencyCode_${orderId}`).text(currencyCode || '');

        // Запускаем цепочку обновлений расчетов
        const purchasePriceInRub = updatePurchasePriceInRub(orderId, currencyCode, costFactor);
        updateCostPrice(orderId, currencyCode, costFactor, weightCurrencyCode, purchasePriceInRub);
        // updateCalculations вызывается из updateCostPrice
        if (window.updateSupplierList) { // Обновляем сводку по поставщикам
            window.updateSupplierList();
        }
    });

    // При изменении закупочной цены, веса товара или комиссии Ozon
    $('#shipmentTable').on('input', '.purchase-price-field, .product-weight-field, .ozon-commission-field', function () {
        const elementId = this.id; // ID измененного элемента
        const orderId = elementId.split('_')[1]; // ID заказа
        const supplierSelect = $(`#SupplierId_${orderId}`);
        const selectedSupplier = supplierSelect.find('option:selected');
        const currencyCode = selectedSupplier.data('currency-code');
        const weightCurrencyCode = selectedSupplier.data('weight-currency-code');
        const costFactor = extractNumber(selectedSupplier.data('cost-factor')) || 1;

        // Если изменилась закупочная цена или вес, пересчитываем себестоимость
        if (elementId.startsWith('PurchasePrice_') || elementId.startsWith('ProductWeight_')) {
            const purchasePriceInRub = updatePurchasePriceInRub(orderId, currencyCode, costFactor);
            updateCostPrice(orderId, currencyCode, costFactor, weightCurrencyCode, purchasePriceInRub);
        } else { // Если изменилась комиссия Ozon, пересчитываем только прибыль/наценку
            updateCalculations(orderId);
        }
        if (window.updateSupplierList) { // Обновляем сводку по поставщикам
            window.updateSupplierList();
        }
    });

    // Первичный расчет для всех строк при загрузке страницы
    // Запускаем событие 'change' для каждого селекта поставщика, чтобы инициировать расчеты
    $('.supplier-select').each(function () {
        $(this).trigger('change');
    });


    // --- Универсальное поле для "Номера заказа поставщику" ---
    // При вводе в общее поле, обновляем все индивидуальные поля в таблице
    $('#universalOrderNumberToSupplier').on('input', function () {
        var value = this.value; // Значение из общего поля
        $('.orderNumberToSupplie-field').val(value); // Устанавливаем это значение для всех полей с классом 'orderNumberToSupplie-field'
    });

    // Логика для скрытия/показа колонок (cookie based) была удалена согласно требованию.

}); // Конец Document Ready