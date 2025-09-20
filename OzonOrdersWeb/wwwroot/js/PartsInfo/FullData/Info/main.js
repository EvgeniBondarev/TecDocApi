// wwwroot/js/parts-info/main.js

document.addEventListener('DOMContentLoaded', function() {
    // Инициализация Materialize компонентов
    M.Tabs.init(document.querySelectorAll('.tabs'));

    // Получаем параметры из URL для запуска AJAX-запросов
    const urlParams = new URLSearchParams(window.location.search);
    const code = urlParams.get('code');
    const supplier = urlParams.get('supplier');

    const div = document.querySelector('.supplier-ids');
    const tecdocId = div.dataset.tecdocId;
    const jsId = div.dataset.jsId;


    if (code && supplier) {
        // Запускаем загрузку данных из разных модулей
        fetchSubstitutes(supplier, code);
        fetchAndRenderCrossCodes(code, jsId);
        fetchProductInformation(code, supplier);
        setupSearch();
    }

    // Инициализация галереи происходит через инлайн-скрипт в cshtml,
    // так как ей нужны данные, сгенерированные на сервере.
});