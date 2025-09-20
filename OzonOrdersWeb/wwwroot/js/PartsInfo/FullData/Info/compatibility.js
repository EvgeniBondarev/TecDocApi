// wwwroot/js/parts-info/compatibility.js

let allSubstitutesData = [];

function fetchSubstitutes(supplier, code) {
    if (!supplier || !code) return;
    const url = `/PartsInfo/FullData/GetSubstitute?supplier=${encodeURIComponent(supplier)}&code=${encodeURIComponent(code)}`;
    const container = document.getElementById('substitutes-container');
    const tabsContainer = document.querySelector('.compatibility-tabs');
    if (!container || !tabsContainer) return;
    
    container.innerHTML = '<div class="loading">Загрузка данных...</div>';

    fetch(url)
        .then(response => response.ok ? response.json() : Promise.reject('Network response was not ok'))
        .then(data => {
            allSubstitutesData = data;
            document.getElementById('search-container').style.display = data?.length ? 'block' : 'none';

            const specificData = data.filter(item => item.modelId !== -1);
            const generalData = data.filter(item => item.modelId === -1);

            if (specificData.length > 0 || generalData.length > 0) {
                tabsContainer.style.display = 'flex';
                initCompatibilityTabs(specificData.length > 0, generalData.length > 0);
                if (specificData.length > 0) renderSubstitutes(specificData, 'substitutes-container');
                if (generalData.length > 0) renderSubstitutes(generalData, 'substitutes-container-general');
            } else {
                container.innerHTML = '<div class="no-data">Нет данных о совместимости</div>';
            }
        })
        .catch(error => {
            document.getElementById('search-container').style.display = 'none';
            container.innerHTML = `<div class="error">Ошибка загрузки данных: ${error.message}</div>`;
        });
}

function initCompatibilityTabs(hasSpecificData, hasGeneralData) {
    const specificTabLink = document.querySelector('.compatibility-tabs .tab a[href="#specific-compatibility"]');
    const generalTabLink = document.querySelector('.compatibility-tabs .tab a[href="#general-compatibility"]');

    if (!hasSpecificData) specificTabLink.parentElement.style.display = 'none';
    if (!hasGeneralData) generalTabLink.parentElement.style.display = 'none';

    if (hasSpecificData && !hasGeneralData) {
        specificTabLink.classList.add('active');
        document.getElementById('specific-compatibility').classList.add('active');
    } else if (!hasSpecificData && hasGeneralData) {
        generalTabLink.classList.add('active');
        document.getElementById('general-compatibility').classList.add('active');
    }

    if (typeof M !== 'undefined') {
        const tabs = document.querySelector('.compatibility-tabs');
        if (tabs) M.Tabs.init(tabs);
    }
}

function renderSubstitutes(data, containerId) {
    const container = document.getElementById(containerId);
    if (!container) return;
    container.innerHTML = '';
    if (!data || data.length === 0) {
        container.innerHTML = '<div class="no-data">Нет данных о совместимости</div>';
        return;
    }

    data.forEach(model => {
        const modelSection = document.createElement('div');
        modelSection.className = 'model-section';
        const modelHeader = document.createElement('div');
        modelHeader.className = 'model-header';
        modelHeader.innerHTML = `<span class="model-name">${model.modelName}</span><span class="toggle-icon">+</span>`;
        const modelContent = document.createElement('div');
        modelContent.className = 'model-content';
        modelContent.style.display = 'none';

        if (model.substitutes?.length > 0) {
            // ... (здесь сложная логика рендеринга таблиц, копируем ее без изменений)
            const table = document.createElement('table');
            table.className = 'substitutes-table';
            const thead = document.createElement('thead');
            thead.innerHTML = `<tr><th>Тип</th><th>Название</th><th>Модификация</th><th>Период производства</th></tr>`;
            table.appendChild(thead);
            const tbody = document.createElement('tbody');
            model.substitutes.forEach(substitute => {
                const mainRow = document.createElement('tr');
                mainRow.className = 'main-row';
                mainRow.dataset.description = substitute.modification?.description.toLowerCase() || '';
                mainRow.innerHTML = `<td>${substitute.type || '—'}</td><td>${substitute.name || '—'}</td><td>${substitute.modification?.description || '—'}</td><td>${substitute.modification?.constructionInterval || '—'}</td>`;
                tbody.appendChild(mainRow);

                if (substitute.attributes?.length > 0) {
                    const attrRow = document.createElement('tr');
                    attrRow.className = 'attributes-row';
                    attrRow.style.display = 'none';
                    const attrCell = document.createElement('td');
                    attrCell.colSpan = 4;
                    const attrHeader = document.createElement('div');
                    attrHeader.className = 'attributes-header';
                    attrHeader.innerHTML = `<span>Характеристики</span><span class="toggle-icon">+</span>`;
                    const attrContent = document.createElement('div');
                    attrContent.className = 'attributes-content';
                    const attrTable = document.createElement('table');
                    attrTable.className = 'attributes-table';
                    substitute.attributes.forEach(attr => {
                        const row = document.createElement('tr');
                        row.innerHTML = `<td class="attr-title">${attr.title}</td><td class="attr-value">${attr.value}</td>`;
                        attrTable.appendChild(row);
                    });
                    attrContent.appendChild(attrTable);
                    attrCell.appendChild(attrHeader);
                    attrCell.appendChild(attrContent);
                    attrRow.appendChild(attrCell);
                    tbody.appendChild(attrRow);

                    mainRow.addEventListener('click', () => {
                        const isHidden = attrRow.style.display === 'none';
                        attrRow.style.display = isHidden ? 'table-row' : 'none';
                        attrHeader.querySelector('.toggle-icon').textContent = isHidden ? '-' : '+';
                    });
                }
            });
            table.appendChild(tbody);
            modelContent.appendChild(table);
        }

        modelSection.appendChild(modelHeader);
        modelSection.appendChild(modelContent);
        container.appendChild(modelSection);
        modelHeader.addEventListener('click', function () {
            const isHidden = modelContent.style.display === 'none';
            modelContent.style.display = isHidden ? 'block' : 'none';
            this.querySelector('.toggle-icon').textContent = isHidden ? '-' : '+';
        });
    });
}


function setupSearch() {
    const searchInput = document.getElementById('substitute-search');
    if (!searchInput) return;

    searchInput.addEventListener('input', function () {
        const searchTerm = this.value.toLowerCase().replace(/\s+/g, '');
        const activeTabHref = document.querySelector('.compatibility-tabs .tab a.active')?.getAttribute('href');
        const isSpecificTab = activeTabHref === '#specific-compatibility';

        if (searchTerm === '') {
            renderSubstitutes(allSubstitutesData.filter(item => (isSpecificTab ? item.modelId !== -1 : item.modelId === -1)), isSpecificTab ? 'substitutes-container' : 'substitutes-container-general');
            return;
        }

        const filteredData = allSubstitutesData.map(model => {
            const filteredSubstitutes = model.substitutes?.filter(substitute =>
                (substitute.modification?.description.toLowerCase().replace(/\s+/g, '') || '').includes(searchTerm)
            );
            return { ...model, substitutes: filteredSubstitutes?.length ? filteredSubstitutes : null };
        }).filter(model => model.substitutes);

        renderSubstitutes(filteredData.filter(item => (isSpecificTab ? item.modelId !== -1 : item.modelId === -1)), isSpecificTab ? 'substitutes-container' : 'substitutes-container-general');
    });
}