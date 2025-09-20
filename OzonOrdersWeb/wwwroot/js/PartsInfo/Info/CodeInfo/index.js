document.addEventListener('DOMContentLoaded', function() {
    const input = document.getElementById('search');
    const resultsList = document.getElementById('autocomplete-results');
    const cache = {};
    let debounceTimeout;
    let lastSearch = '';

    // Оптимизация: предотвращение множественных запросов при быстром вводе
    input.addEventListener('input', function() {
        const query = this.value.trim();

        // Если запрос не изменился, ничего не делаем
        if (query === lastSearch) return;
        lastSearch = query;

        clearTimeout(debounceTimeout);

        if (query.length < 3) {
            hideResults();
            return;
        }

        debounceTimeout = setTimeout(() => {
            const sanitizedQuery = query.replace(/[^a-zA-Z0-9]/g, '');

            if (cache[sanitizedQuery]) {
                renderResults(cache[sanitizedQuery]);
            } else {
                showLoading();
                fetch(`/PartsInfo/Abcp/GetTips?number=${sanitizedQuery}`)
                    .then(handleResponse)
                    .then(data => {
                        cache[sanitizedQuery] = data.tips || [];
                        renderResults(cache[sanitizedQuery]);
                    })
                    .catch(handleError);
            }
        }, 300);
    });

    // Обработка клика вне поля ввода
    document.addEventListener('click', function(e) {
        if (!input.contains(e.target) && !resultsList.contains(e.target)) {
            hideResults();
        }
    });

    // Обработка клавиш (вверх/вниз/enter)
    input.addEventListener('keydown', function(e) {
        if (!resultsList.children.length) return;

        const items = resultsList.querySelectorAll('.autocomplete-item');
        let current = Array.from(items).findIndex(item => item.classList.contains('highlighted'));

        if (e.key === 'ArrowDown') {
            e.preventDefault();
            if (current >= items.length - 1) current = -1;
            highlightItem(items, current + 1);
        } else if (e.key === 'ArrowUp') {
            e.preventDefault();
            if (current <= 0) current = items.length;
            highlightItem(items, current - 1);
        } else if (e.key === 'Enter' && current >= 0) {
            e.preventDefault();
            items[current].click();
        }
    });

    function highlightItem(items, index) {
        items.forEach(item => item.classList.remove('highlighted'));
        items[index].classList.add('highlighted');
        items[index].scrollIntoView({ block: 'nearest' });
    }

    function showLoading() {
        resultsList.innerHTML = '<div class="autocomplete-item">Загрузка...</div>';
        resultsList.style.display = 'block';
    }

    function hideResults() {
        resultsList.style.display = 'none';
        resultsList.innerHTML = '';
    }

    function handleResponse(res) {
        if (!res.ok) throw new Error(res.statusText);
        return res.json();
    }

    function handleError() {
        resultsList.innerHTML = '<div class="no-results">Ошибка загрузки данных</div>';
        resultsList.style.display = 'block';
    }

    function renderResults(tips) {
        if (!tips || !tips.length) {
            resultsList.innerHTML = '<div class="no-results">Ничего не найдено</div>';
            resultsList.style.display = 'block';
            return;
        }

        resultsList.innerHTML = '';

        tips.slice(0, 50).forEach(tip => {
            const item = document.createElement('li');
            item.className = 'autocomplete-item';

            item.innerHTML = `
      <a href="/PartsInfo/FullData/Info?code=${encodeURIComponent(tip.number)}&supplier=${encodeURIComponent(tip.brand)}" class="item-main-line">
        <span class="item-number">${tip.number || '—'}</span>
        <span class="item-brand">${tip.brand || '—'}</span>
      </a>
      ${tip.description ? `<div class="item-description">${tip.description}</div>` : ''}
    `;

            item.addEventListener('click', () => {
                input.value = tip.number || '';
                hideResults();
                input.focus();
            });

            resultsList.appendChild(item);
        });

        // Выравниваем ширину списка с полем ввода
        resultsList.style.width = input.offsetWidth + 'px';
        resultsList.style.display = 'block';
    }
});