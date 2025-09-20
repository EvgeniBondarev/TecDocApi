// wwwroot/js/parts-info/productInfo.js

function fetchProductInformation(code, manufacturer) {
    if (!code || !manufacturer) return;

    const url = `/PartsInfo/FullData/GetProductInformation?code=${encodeURIComponent(code)}&manufacturer=${encodeURIComponent(manufacturer)}`;
    const container = document.querySelector('#tab-parameters');
    if (!container) return;

    // Показываем контейнер перед загрузкой
    container.hidden = false;

    // Создаем элемент для отображения статуса загрузки
    const loadingDiv = document.createElement('div');
    loadingDiv.className = 'loading';
    loadingDiv.textContent = 'Загрузка дополнительных характеристик...';
    container.appendChild(loadingDiv);

    fetch(url)
        .then(res => res.ok ? res.json() : Promise.reject('Ошибка получения информации'))
        .then(data => {
            container.removeChild(loadingDiv); // Удаляем статус загрузки
            renderProductInformation(data, container);
        })
        .catch(error => {
            container.removeChild(loadingDiv); // Удаляем статус загрузки
            const errorDiv = document.createElement('div');
            errorDiv.className = 'error';
            errorDiv.textContent = `Ошибка: ${error.message || error}`;
            container.appendChild(errorDiv);
        });
}

function renderProductInformation(data, container) {
    // Создаем таблицу
    const table = document.createElement('table');
    table.className = 'details-table';

    // Создаем тело таблицы
    const tbody = document.createElement('tbody');
    table.appendChild(tbody);

    // Функция для добавления строки в таблицу
    const addTableRow = (label, value) => {
        if (!value && value !== 0) return;

        const row = document.createElement('tr');

        const nameCell = document.createElement('td');
        nameCell.className = 'details-table__name';
        nameCell.textContent = label;

        const valueCell = document.createElement('td');
        valueCell.className = 'details-table__value';
        valueCell.textContent = value;

        row.appendChild(nameCell);
        row.appendChild(valueCell);
        tbody.appendChild(row);
    };

    // Добавляем характеристики в таблицу
    addTableRow('Категория', data.shorT_DESCRIPTION);
    addTableRow('Вес упаковки', data.packagE_WEIGHT);
    addTableRow('Длина упаковки', data.packagE_LENGTH);
    addTableRow('Ширина упаковки', data.packagE_WIDTH);
    addTableRow('Высота упаковки', data.packagE_HEIGHT);

    // Добавляем таблицу в контейнер только если есть хотя бы одна строка
    if (tbody.querySelector('tr')) {
        container.appendChild(table);
    }

    // Добавляем описание, если оно есть
    if (data.description?.trim()) {
        const descriptionHeader = document.createElement('h1');
        descriptionHeader.className = 'product-title';
        descriptionHeader.textContent = 'Описание';
        container.appendChild(descriptionHeader);

        const descriptionDiv = document.createElement('div');
        descriptionDiv.className = 'description-text';
        descriptionDiv.innerHTML = data.description;
        container.appendChild(descriptionDiv);
    }
}