// wwwroot/js/parts-info/crossCodes.js

function fetchAndRenderCrossCodes(code, jsId) {
    if (!code) return;

    const urlTdCross = `/PartsInfo/FullData/GetTdCross?code=${encodeURIComponent(code)}`;
    const urlJcCross = `/PartsInfo/FullData/GetJcCross?code=${encodeURIComponent(code)}&jcSupplierId=${jsId}`;
    const container = document.getElementById('tab-crosscodes');
    if (!container) return;

    container.innerHTML = '<div class="loading">Загрузка кросс-кодов...</div>';

    Promise.all([
        fetch(urlTdCross).then(res => res.ok ? res.json() : Promise.resolve([])), // Не прерываемся при ошибке
        fetch(urlJcCross).then(res => res.ok ? res.json() : Promise.resolve([]))
    ])
        .then(([tdData, jcData]) => {
            container.innerHTML = '';
            let hasContent = false;

            if (tdData?.length > 0) {
                hasContent = true;
                container.appendChild(createCrossCodeTable('Кросс-коды TecDoc', tdData, item => ({
                    code: item.oeNbr || '—',
                    description: (item.supplier?.description || '') + (item.manufacturer?.description ? ` (${item.manufacturer.description})` : '') || '—'
                })));
            }

            if (tdData?.length > 0 && jcData?.length > 0) {
                const divider = document.createElement('div');
                divider.className = 'divider';
                container.appendChild(divider);
            }

            if (jcData?.length > 0) {
                hasContent = true;
                container.appendChild(createCrossCodeTable('Кросс-коды JСCross', jcData, item => ({
                    code: item.code || '—',
                    description: item.supplier?.description || item.supplier?.name || '—'
                })));
            }

            if (!hasContent) {
                container.innerHTML = '<div class="no-data">Нет данных о кросс-кодах</div>';
            }
        })
        .catch(error => {
            container.innerHTML = `<div class="error">Ошибка загрузки кросс-кодов: ${error.message || error}</div>`;
        });
}

function createCrossCodeTable(title, data, mapper) {
    const fragment = document.createDocumentFragment();
    const titleDiv = document.createElement('div');
    titleDiv.className = 'crosscodes-title product-code';
    titleDiv.textContent = title;
    fragment.appendChild(titleDiv);

    const table = document.createElement('table');
    table.className = 'details-table';
    const tbody = document.createElement('tbody');

    data.map(mapper).forEach(item => {
        const row = document.createElement('tr');

        // Ячейка с кодом - ссылка
        const codeCell = document.createElement('td');
        const codeLink = document.createElement('a');
        codeLink.href = `/PartsInfo/FullData/Info?code=${encodeURIComponent(item.code)}&supplier=${encodeURIComponent(item.description)}`;
        codeLink.textContent = item.code;
        codeLink.target = "_blank"; // Открывать в новой вкладке
        codeLink.rel = "noopener noreferrer"; // Безопасность
        codeCell.appendChild(codeLink);

        // Ячейка с описанием - тоже ссылка
        const descCell = document.createElement('td');
        const descLink = document.createElement('a');
        descLink.href = `/PartsInfo/FullData/Info?code=${encodeURIComponent(item.code)}&supplier=${encodeURIComponent(item.description)}`;
        descLink.textContent = item.description;
        descLink.target = "_blank";
        descLink.rel = "noopener noreferrer";
        descCell.appendChild(descLink);

        row.appendChild(codeCell);
        row.appendChild(descCell);
        tbody.appendChild(row);
    });

    table.appendChild(tbody);
    fragment.appendChild(table);
    return fragment;
}