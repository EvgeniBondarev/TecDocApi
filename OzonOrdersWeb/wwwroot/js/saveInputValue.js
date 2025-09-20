document.addEventListener('DOMContentLoaded', (event) => {
    // Функция для сохранения значения поля в куки на 1 день
    function saveToCookies(key, value) {
        const expires = new Date();
        expires.setTime(expires.getTime() + (24 * 60 * 60 * 1000)); // 1 день
        document.cookie = `${key}=${encodeURIComponent(value)}; expires=${expires.toUTCString()}; path=/`;
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

    // Отслеживание поля 'statusSelect' на странице
    const statusSelectElement = document.getElementById('statusSelect');
    if (statusSelectElement) {
        const savedStatusValue = loadFromCookies(statusSelectElement.id);

        // Устанавливаем значение из куков, если оно есть
        if (savedStatusValue) {
            statusSelectElement.value = savedStatusValue;
        }

        // Сохраняем в куки при изменении
        statusSelectElement.addEventListener('change', function () {
            saveToCookies(this.id, this.value);
        });
    }

    // Для каждого select с классом 'status-select'
    document.querySelectorAll('.status-select').forEach(select => {
        const id = select.id;
        const savedValue = loadFromCookies(id);

        // Если есть сохраненное значение, устанавливаем его
        if (savedValue) {
            select.value = savedValue;
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
        }

        input.addEventListener('input', function () {
            saveToCookies(id, this.value);
        });
    });
});
