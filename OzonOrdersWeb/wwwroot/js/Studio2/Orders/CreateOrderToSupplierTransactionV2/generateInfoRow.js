function generateInfoRow(orderId, rates) {
    // Безопасное получение данных с значениями по умолчанию
    const orderDetails = orderDataCache[orderId]?.orderDetails || {};
    const { response = {} } = orderDetails;

    const tradesoftItems = orderDataCache[orderId]?.tradesoftData?.tradesofData || [];
    const zzapItems = orderDataCache[orderId]?.zzapData?.zzapData || [];
    const abcpItems = orderDataCache[orderId]?.abcpData?.items || [];
    const avdItems = orderDataCache[orderId]?.avdData?.items || [];
    const stockData = orderDataCache[orderId]?.stockData || {};
    const { stocks = [] } = stockData;
    const abcpOneItem = orderDataCache[orderId]?.abcpOneData || {};
    const bitrixData = orderDataCache[orderId]?.bitrixData?.data || [];

    const colorMap = {
        'https://www.maxi.parts/': 'text-success',
        'https://www.zzap.ru/': 'text-danger',
        'https://www.abcp.ru/': 'text-primary',
        'https://www.avdmotors.ru/': 'text-info'
    };

    const tradesofData = [...tradesoftItems, ...zzapItems, ...abcpItems, ...avdItems];

    // Берем изображения из imageUrls (новое поле в конце ответа)
    let allImages = response.imageUrls || [];

    // Если imageUrls пустой, используем fallback изображение
    if (allImages.length === 0) {
        allImages = ["https://s3.timeweb.cloud/25f554fc-6f66254e-9650-4d17-8e13-77b5b7d3242e/AppData/Studio2/IMG/300x200no-image.svg"];
    }

    // Определяем главное изображение (первое в списке)
    const mainImage = allImages[0] || "https://s3.timeweb.cloud/25f554fc-6f66254e-9650-4d17-8e13-77b5b7d3242e/AppData/Studio2/IMG/300x200no-image.svg";

    // Миниатюры (все изображения кроме первого, ограничиваем до 4 штук)
    const limitedMiniatures = allImages.slice(1, 5);

    // Добавляем поле priceInRub каждому элементу
    tradesofData.forEach(item => {
        if (item && item.priceDecimal && item.currencyCode) {
            const rate = rates[item.currencyCode] || 1;
            item.priceInRub = parseFloat(item.priceDecimal) * rate;
        } else {
            item.priceInRub = 0;
        }
    });

    // Сортировка по цене в рублях по умолчанию (только валидные элементы)
    tradesofData.sort((a, b) => (a.priceInRub || 0) - (b.priceInRub || 0));

    // Безопасное получение текстовых данных
    const detailInfo = response.detailInfo || {};
    const productInfo = response.productInfo || {};
    const description = detailInfo.description || productInfo.description || '';
    const article = detailInfo.articleSchema?.dataSupplierArticleNumber || productInfo.articlE_NUMBER || '';
    const normalizedArticle = detailInfo.normalizedArticle || '';
    const supplier = detailInfo.supplier || {};
    const detailAttributes = detailInfo.detailAttributes || [];

    return `
        <tr class="info-row table-light always-light" data-order-id="${orderId}">
            <td colspan="5">
                <div class="d-flex">
                    <div class="main-content me-4" style="flex: 1;">
                        ${description ? `
                            <p style="font-size: 20px;">
                                ${description.length > 100
        ? description.slice(0, 100) + '...'
        : description}
                            </p>
                        ` : ''}

                        <div class="d-flex justify-content-start align-items-start">
                            <div class="gallery float-start me-3">
                                <div class="main-image mb-2">
                                    <img src="${mainImage}" alt="Main Image" class="img-fluid rounded border" style="width: 265px; height: 265px; cursor: pointer;" onclick="openImage('${mainImage}')">
                                </div>

                                ${limitedMiniatures.length > 0 ? `
                                    <div class="miniatures d-flex justify-content-start">
                                        ${limitedMiniatures.map((img, index) => `
                                            <div class="miniature mx-1">
                                                <img src="${img}" alt="Thumbnail" class="img-thumbnail" style="width: 60px; height: 60px; cursor: pointer;" onclick="openImage('${img}')">
                                            </div>
                                        `).join('')}
                                    </div>
                                ` : ''}
                            </div>

                            <div class="mb-3" style="font-size: 20px;">
                                ${article ? `
                                    <strong>Артикул: </strong>
                                    ${article}
                                    ${normalizedArticle && normalizedArticle !== article
        ? `(${normalizedArticle})`
        : ''}
                                    <br/>
                                ` : ''}

                                ${supplier.code || supplier.name || productInfo.manufacturer ? `
                                    <strong>Производитель:</strong><br>
                                    <ul style="font-size: 16px; padding-left: 20px;">
                                        ${supplier.code || productInfo.manufacturer
        ? `<li><strong>Префикс:</strong> ${supplier.code || productInfo.manufacturer}</li>`
        : ''}
                                        ${supplier.name
        ? `<li><strong>Название:</strong> ${supplier.name}</li>`
        : ''}
                                        ${supplier.description
        ? `<li><strong>Описание:</strong> ${supplier.description}</li>`
        : ''}
                                        ${supplier.rating !== undefined && supplier.rating !== null
        ? `<li><strong>Рейтинг:</strong> ${supplier.rating}</li>`
        : ''}
                                        ${supplier.tecdocSupplierId || productInfo.teC_DOC_PROD
        ? `<li><strong>Tecdoc ID:</strong> ${supplier.tecdocSupplierId || productInfo.teC_DOC_PROD}</li>`
        : ''}
                                    </ul>
                                ` : ''}

                                ${description ? `
                                    <strong>Описание: </strong>
                                    <span title="${description}">
                                        ${description.length > 30
        ? description.slice(0, 30) + '...'
        : description}
                                    </span>
                                    <br/>
                                ` : ''}
                                
                                ${abcpOneItem?.weight ? `
                                    <strong>Вес ABCP:</strong>
                                    <span class="weight-value"
                                          style="cursor: pointer; text-decoration: underline; color: blue;"
                                          data-weight="${abcpOneItem.weight}"
                                          data-order-id="${orderId}">
                                          ${abcpOneItem.weight} кг
                                    </span>
                                    <br/>
                                ` : ''}
                                
                                ${productInfo.shorT_DESCRIPTION || productInfo.barcodes || productInfo.packagE_WEIGHT ? `
                                    <strong>Доп. информация:</strong><br>
                                    <ul style="font-size: 16px; padding-left: 20px;">
                                        ${productInfo.shorT_DESCRIPTION ? `<li><strong>Категория:</strong> ${productInfo.shorT_DESCRIPTION}</li>` : ''}
                                        ${productInfo.barcodes ? `<li><strong>Штрих-коды:</strong> ${productInfo.barcodes}</li>` : ''}
                                        ${productInfo.packagE_WEIGHT ? `
                                            <li>
                                                <strong>Вес:</strong>
                                                <span class="weight-value"
                                                      style="cursor: pointer; text-decoration: underline; color: blue;"
                                                      data-weight="${productInfo.packagE_WEIGHT}"
                                                      data-order-id="${orderId}">
                                                    ${productInfo.packagE_WEIGHT} кг
                                                </span>
                                            </li>
                                        ` : ''}
                                        ${productInfo.packagE_LENGTH && productInfo.packagE_WIDTH && productInfo.packagE_HEIGHT ?
        `<li><strong>Габариты упаковки:</strong> ${productInfo.packagE_LENGTH}×${productInfo.packagE_WIDTH}×${productInfo.packagE_HEIGHT} см</li>` : ''}
                                    </ul>
                                ` : ''}

                                ${detailAttributes.length > 0 ? `
                                    <strong>Характеристики:</strong><br>
                                    <ul class="attributes-list" style="font-size: 18px; max-height: 200px; overflow-y: auto; padding-left: 20px;">
                                        ${detailAttributes
        .filter(attr => attr && (attr.displayTitle || attr.description || attr.displayValue))
        .map(attr => {
            const title = attr.displayTitle || attr.description || '';
            const value = attr.displayValue || '';
            const isWeight = /Вес \[кг\]|Масса/i.test(title);

            const displayValue = isWeight && value
                ? `<span class="weight-value"
                                                         style="cursor: pointer; text-decoration: underline; color: blue;"
                                                         data-weight="${value}"
                                                         data-order-id="${orderId}">
                                                       ${value} кг
                                                   </span>`
                : value ? `- ${value.length > 15 ? value.slice(0, 15) + '...' : value}` : '';

            return `<li title="${attr.description || ''} - ${value}">
                                                    ${title.length > 15 ? title.slice(0, 15) + '...' : title} ${displayValue}
                                                </li>`;
        }).join('')}
                                    </ul>
                                ` : ''}
                                
                                ${bitrixData.length > 0 ? `
                                    <strong>Данные Bitrix:</strong>
                                    <ul style="font-size: 16px; padding-left: 20px;">
                                        ${bitrixData.map(item => item ? `
                                            <li>
                                                <b>Цена</b>: <em>${item.group || 'Нет группы'}</em> ${item.price || '0'} ${item.currency || 'RUB'}<br/>
                                                Склад: ${item.stores && Object.keys(item.stores).length > 0
        ? Object.entries(item.stores).map(([store, qty]) => `${store}: ${qty}`).join(', ')
        : 'Нет данных'}
                                            </li>
                                        ` : '').join('')}
                                    </ul>
                                ` : ''}
                            </div>
                        </div>
                    </div>
                </div>
            </td>
            <td></td>
            <td colspan="10">
                ${generateStockDataTable(orderId, stocks)}
                ${generateStockTradeTable(orderId, stocks, tradesofData, colorMap)}
            </td>
            <script>
                applyColumnState('.stock-trade-table');
            </script>
        </tr>
    `;
}

function generateStockTradeTable(orderId, stocks, tradesofData, colorMap) {
    // Временное хранение состояния сортировки для каждого orderId
    window.sortState = window.sortState || {};
    window.sortState[orderId] = window.sortState[orderId] || { column: 'priceInRub', direction: 'asc' };

    // Сохраняем данные в глобальном кеше, чтобы не передавать их в HTML
    window.tradesofDataCache = window.tradesofDataCache || {};
    window.tradesofDataCache[orderId] = tradesofData;

    return `
        <table class="table table-bordered table-striped stock-trade-table compact-mode" data-order-id="${orderId}">
            <thead>
                <tr>
                    <th class="sortable" data-column="caption" onclick="sortAndRerenderTable('${orderId}', 'caption', 'string')">
                        Наимен.
                        <span class="sort-arrow"></span>
                    </th>
                    <th class="sortable" data-column="direction" onclick="sortAndRerenderTable('${orderId}', 'direction', 'string')">
                        Направление
                        <span class="sort-arrow"></span>
                    </th>
                    <th class="sortable" data-column="title" onclick="sortAndRerenderTable('${orderId}', 'title', 'string')">
                        Поставщик
                        <span class="sort-arrow"></span>
                    </th>
                    <th class="sortable" data-column="rest" onclick="sortAndRerenderTable('${orderId}', 'rest', 'number')">
                        Наличие
                        <span class="sort-arrow"></span>
                    </th>
                    <th class="sortable" data-column="priceInRub" onclick="sortAndRerenderTable('${orderId}', 'priceInRub', 'number')">
                        Цена постав.
                        <span class="sort-arrow"></span>
                    </th>
                    <th class="sortable" data-column="costPrice" onclick="sortAndRerenderTable('${orderId}', 'costPrice', 'number')">
                        Себест.
                        <span class="sort-arrow"></span>
                    </th>
                    <th class="sortable" data-column="deliveryDays" onclick="sortAndRerenderTable('${orderId}', 'deliveryDays', 'number')">
                        Срок сред.
                        <span class="sort-arrow"></span>
                    </th>
                    <th class="sortable" data-column="deliveryDaysMax" onclick="sortAndRerenderTable('${orderId}', 'deliveryDaysMax', 'number')">
                        Срок макс.
                        <span class="sort-arrow"></span>
                    </th>
                    <th class="sortable" data-column="producer" onclick="sortAndRerenderTable('${orderId}', 'producer', 'string')">
                        Произв.
                        <span class="sort-arrow"></span>
                    </th>
                </tr>
            </thead>
            <tbody>
                ${generateStockTradeRows(orderId, stocks, tradesofData, colorMap)}
            </tbody>
        </table>
    `;
}

function generateStockTradeRows(orderId, stocks, tradesofData, colorMap) {
    let rows = '';

    tradesofData.forEach((trade, index) => {
        const rowColor = trade.siteUrl ? colorMap[trade.siteUrl] || '' : '';
        const costPriceDisplay = trade.costPriceFormatted || (trade.costPrice !== undefined ? trade.costPrice : '-');

        rows +=
            `<tr style="${rowColor ? `background-color: ${rowColor};` : ''}"  data-order-id="${orderId}" data-item-id="${trade.itemId}">
            <td class="custom-wrap">
                <div class="py-1 short-name" style="white-space: nowrap;" data-order-id="${orderId}" data-index-row="${index}" title="${trade.caption || '-'}">
                    ${trade.caption && trade.caption.length > 10 ? trade.caption.substring(0,10) + '...' : trade.caption || '-'}
                </div>
            </td>

            <td class="custom-wrap">
                <div class="py-1 short-name" style="white-space: nowrap;" data-order-id="${orderId}" data-index-row="${index}" title="${trade.direction || '-'}">
                    ${trade.direction && trade.direction.length > 10 ? trade.direction.substring(0,10) + '…' : trade.direction || '-'}
                </div>
            </td>

            <td class="custom-wrap">
                <div class="py-1 supplier short-name ${colorMap[trade.siteUrl] || 'text-body'}" style="white-space: nowrap;" data-order-id="${orderId}" data-index-row="${index}" title="${(trade.siteUrl || '') + (trade.description ? '&#10;' + trade.description.replace(/\n/g, '&#10;') : '')}">
                    ${trade.title || '-'}
                </div>
            </td>

            <td class="custom-wrap">
                <div class="py-1 short-name" style="white-space: nowrap; text-align: right;" data-order-id="${orderId}" data-index-row="${index}">
                    ${trade.rest || '-'}
                </div>
            </td>

            <td class="custom-wrap">
                <div class="py-1 d-flex justify-content-end align-items-center short-name" style="white-space: nowrap;"
                     data-order-id="${orderId}"
                     data-index-row="${index}">
                     <span class="me-2">
                        <a href="javascript:void(0);"
                           onclick="setPurchasePrice('${orderId}', '${trade.priceDecimal}', '${index}', '${trade.itemId}')"
                           style="text-decoration: none; cursor: pointer;"
                           class="price price-link ${trade.priceDescription ? 'text-muted' : ''}"
                           data-order-id="${orderId}"
                           data-index-row="${index}"
                           data-price="${trade.priceDecimal}"
                           data-currency-code="${trade.currencyCode}"
                           data-site="${trade.siteUrl}"
                           title="${trade.priceDescription || ''}">
                           ${trade.priceDecimal !== undefined ? trade.priceDecimal + " " + (trade.currencyCode || '') : '-'}
                        </a>
                    </span>
                    <a href="javascript:void(0);" onclick="setPurchasePrice('${orderId}', '${trade.priceDecimal}', '${index}', '${trade.itemId}')" style="text-decoration: none; cursor: pointer;">
                        <button type="button"
                                class="btn-link p-0 stok-order"
                                style="width: 18px; height: 18px; vertical-align: middle;"
                                data-order-id="${orderId}"
                                data-stok-id="${trade.itemId || '0'}"
                                data-index-row="${index}"
                                data-supplier-url="${trade.siteUrl}"
                                data-value="${trade.costPrice}">
                            <svg xmlns="http://www.w3.org/2000/svg"
                                 width="18"
                                 height="18"
                                 fill="currentColor"
                                 class="bi bi-cart"
                                 viewBox="0 0 18 18"
                                 style="background-color: transparent;">
                              <path d="M0 1.5A.5.5 0 0 1 .5 1H2a.5.5 0 0 1 .485.379L2.89 3H14.5a.5.5 0 0 1 .491.592l-1.5 8A.5.5 0 0 1 13 12H4a.5.5 0 0 1-.491-.408L2.01 3.607 1.61 2H.5a.5.5 0 0 1-.5-.5m3.102 4l1.313 7h8.17l1.313-7zM5 12a2 2 0 1 0 0 4 2 2 0 0 0 0-4m7 0a2 2 0 1 0 0 4 2 2 0 0 0 0-4m-7 1a1 1 0 1 1 0 2 1 1 0 0 1 0-2m7 0a1 1 0 1 1 0 2 1 1 0 0 1 0-2"/>
                            </svg>
                            <span class="visually-hidden">Button</span>
                        </button>
                    </a>
                </div>
            </td>
            <td class="custom-wrap" style="text-align: right;">
                <div class="py-1 short-name cost-price-row ${colorMap[trade.siteUrl] || 'text-body'}"
                     style="white-space: nowrap; text-align: right;"
                     data-order-id="${orderId}"
                     data-index-row="${index}"s
                     data-value="${trade.costPrice}">
                    ${costPriceDisplay}
                </div>
            </td>
            <td class="custom-wrap" style="text-align: right;">
                <div class="py-1 short-name"
                     style="white-space: nowrap; text-align: right;"
                     data-order-id="${orderId}"
                     data-index-row="${index}"
                     title="${trade.deliveryDays || '-'}">
                    ${trade.deliveryDays && trade.deliveryDays.length > 15 ? trade.deliveryDays.substring(0, 15) + '...' : trade.deliveryDays || '-'}
                </div>
            </td>
            <td class="custom-wrap" style="text-align: right;">
                <div class="py-1 short-name"
                     style="white-space: nowrap; text-align: right;"
                     data-order-id="${orderId}"
                     data-index-row="${index}">
                    ${trade.deliveryDaysMax || '-'}
                </div>
            </td>
            <td class="custom-wrap" style="text-align: right;">
                <div class="py-1 manufacturer short-name"
                     style="white-space: nowrap; text-align: right;"
                     data-order-id="${orderId}"
                     data-index-row="${index}">
                    ${trade.producer || '-'}
                </div>
            </td>
        </tr>`;
    });

    return rows;
}

function generateStockDataTable(orderId, stocks) {
    const stockRows = stocks && stocks.length > 0 ? stocks.map(stock => {
        const fullName = stock.name || '-';
        const shortName = fullName.length > 10 ? fullName.substring(0, 10) + '…' : fullName;

        return `
                <tr>
                    <td class="custom-wrap">
                        <div class="py-1" title="${fullName}" style="white-space: nowrap;" data-order-id="${orderId}">
                            ${shortName}
                        </div>
                    </td>
                    <td class="custom-wrap">
                        <div class="py-1" style="white-space: nowrap;" data-order-id="${orderId}">
                            ${stock.article || '-'}
                        </div>
                    </td>
                    <td class="custom-wrap">
                        <div class="py-1" style="white-space: nowrap;" data-order-id="${orderId}">
                            ${stock.stockName || '-'}
                        </div>
                    </td>
                    <td class="custom-wrap">
                        <div class="py-1" style="white-space: nowrap; text-align: right;" data-order-id="${orderId}">
                            ${stock.stock || '-'}
                        </div>
                    </td>
                    <td class="custom-wrap">
                        <div class="py-1" style="white-space: nowrap; text-align: right;" data-order-id="${orderId}">
                            ${formatPrice(stock.price) || '-'}
                        </div>
                    </td>
                </tr>
            `;
            }).join('') : `
            <tr>
                <td colspan="5">
                    <div class="py-1" style="text-align: center;">-</div>
                </td>
            </tr>
        `;

    return `
        <table class="table table-bordered table-sm stock-table mb-2" data-order-id="${orderId}">
            <thead>
                <tr>
                    <th style="width: 20%;">Наимен.</th>
                    <th style="width: 20%;">Артикул</th>
                    <th style="width: 20%;">Файл</th>
                    <th style="width: 20%; text-align: right;">Остаток</th>
                    <th style="width: 20%; text-align: right;">Цена</th>
                </tr>
            </thead>
            <tbody>
                ${stockRows}
            </tbody>
        </table>
    `;
}
function sortAndRerenderTable(orderId, column, type, sortDirection = null) {
    const tradesofData = window.tradesofDataCache[orderId];
    if (!tradesofData) {
        console.error(`Данные для orderId ${orderId} не найдены в кэше.`);
        return;
    }

    const tableContainer = document.querySelector(`.stock-trade-table[data-order-id="${orderId}"] tbody`);
    if (!tableContainer) return;

    window.sortState = window.sortState || {};
    window.sortState[orderId] = window.sortState[orderId] || { column: null, direction: 'asc' };
    const currentSortState = window.sortState[orderId];

    // Определяем направление сортировки
    if (currentSortState.column === column) {
        // Если передан явный direction - используем его, иначе переключаем
        currentSortState.direction = sortDirection !== null
            ? sortDirection
            : (currentSortState.direction === 'asc' ? 'desc' : 'asc');
    } else {
        currentSortState.column = column;
        // Если передан явный direction - используем его, иначе по умолчанию 'asc'
        currentSortState.direction = sortDirection !== null ? sortDirection : 'asc';
    }

    // Логика сортировки
    tradesofData.sort((a, b) => {
        const valueA = a[column];
        const valueB = b[column];

        let valA, valB;

        if (type === 'number') {
            if (column === 'costPrice') {
                valA = a.costPrice;
                valB = b.costPrice;
            } else {
                valA = parseFloat(valueA) || 0;
                valB = parseFloat(valueB) || 0;
            }
            return currentSortState.direction === 'asc' ? valA - valB : valB - valA;
        } else {
            const strA = String(valueA || '').toLowerCase();
            const strB = String(valueB || '').toLowerCase();
            if (strA < strB) return currentSortState.direction === 'asc' ? -1 : 1;
            if (strA > strB) return currentSortState.direction === 'asc' ? 1 : -1;
            return 0;
        }
    });

    const colorMap = {
        'https://www.maxi.parts/': 'text-success',
        'https://www.zzap.ru/': 'text-danger',
        'https://www.abcp.ru/': 'text-primary',
        'https://www.avdmotors.ru/': 'text-info'
    };

    const newRows = generateStockTradeRows(orderId, null, tradesofData, colorMap);
    tableContainer.innerHTML = newRows;

    const tableHeaders = document.querySelectorAll(`.stock-trade-table[data-order-id="${orderId}"] th.sortable`);
    tableHeaders.forEach(header => {
        header.classList.remove('sorted-asc', 'sorted-desc');
    });

    const activeHeader = document.querySelector(`.stock-trade-table[data-order-id="${orderId}"] th.sortable[data-column="${column}"]`);
    if (activeHeader) {
        activeHeader.classList.add(`sorted-${currentSortState.direction}`);
    }

    // Логика восстановления подсветки после сортировки
    const highlightedItemId = window.highlightedItem ? window.highlightedItem[orderId] : null;
    if (highlightedItemId) {
        const highlightedRow = document.querySelector(`tr[data-order-id="${orderId}"][data-item-id="${highlightedItemId}"]`);
        if (highlightedRow) {
            highlightedRow.classList.add('table-warning');
        }
    }
}
function applyColumnState(tableSelector) {
    const state = JSON.parse(localStorage.getItem('tableColumnState') || '{}');
    if (!state.columns) return;

    const table = document.querySelector(tableSelector);
    if (!table) return;

    state.columns.forEach((col, index) => {
        const th = table.querySelector(`thead th:nth-child(${index + 1})`);
        if (th) {
            if (col.hidden) th.style.display = 'none';
            if (col.width) th.style.width = col.width;
        }
        table.querySelectorAll(`tbody td:nth-child(${index + 1})`).forEach(td => {
            if (col.hidden) td.style.display = 'none';
            if (col.width) td.style.width = col.width;
        });
    });
}