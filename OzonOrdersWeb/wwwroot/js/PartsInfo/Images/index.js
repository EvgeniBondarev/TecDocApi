$(document).ready(function () {
    const BASE_URL = 'https://api.interparts.ru/s3/';
    const RANDOM_IMAGES_URL = BASE_URL + 'random-images?count=15';
    const FOLDERS_CACHE_KEY = 's3_folders_cache';
    const IMAGES_CACHE_KEY = 's3_images_cache';
    const RANDOM_CACHE_KEY = 's3_random_images_cache';
    const CACHE_EXPIRY = 5 * 60 * 1000;
    let foldersLoaded = false;
    $('#loadingImages').hide();
    $('.chat-widget').remove();
    loadRandomImages();

    function toggleDropdown(show) {
        if (show && !foldersLoaded) loadFolders();
        $('#folderDropdown').toggle(show);
    }

    $('#folderSearch').on('focus', function () {
        toggleDropdown(true);
    }).on('blur', function () {
        setTimeout(() => toggleDropdown(false), 200);
    });

    function loadFolders() {
        const cachedData = getCachedData(FOLDERS_CACHE_KEY);
        if (cachedData) {
            $('#loadingSpinner').hide();
            displayFolders(cachedData.data);
            updateStatus('Папки загружены из кеша', cachedData.timestamp);
            foldersLoaded = true;
            return;
        }
        fetchFolders();
    }

    function getCachedData(key) {
        const cached = localStorage.getItem(key);
        if (!cached) return null;
        const { data, timestamp } = JSON.parse(cached);
        if (Date.now() - timestamp < CACHE_EXPIRY) return { data, timestamp };
        return null;
    }

    function fetchFolders() {
        $('#loadingSpinner').show();
        $('#folderList').hide();
        $('#errorMessage').hide();

        $.ajax({
            url: `${BASE_URL}folders`,
            type: 'GET',
            dataType: 'json',
            headers: { 'Accept': 'application/json' },
            success: function (data) {
                const timestamp = Date.now();
                localStorage.setItem(FOLDERS_CACHE_KEY, JSON.stringify({ data, timestamp }));
                $('#loadingSpinner').hide();
                displayFolders(data);
                updateStatus('Папки загружены с API', timestamp);
                foldersLoaded = true;
            },
            error: function () {
                $('#loadingSpinner').hide();
                $('#errorMessage').show();
            }
        });
    }

    function displayFolders(folders) {
        const $list = $('#folderList').empty();
        if (folders.length > 0) {
            folders.forEach(folder => {
                $list.append(`<div class="list-group-item folder-item"><i class="fas fa-folder folder-icon"></i><span class="folder-name">${folder}</span></div>`);
            });
            $('.folder-item').on('click', function () {
                const name = $(this).find('.folder-name').text();
                $('#folderSearch').val(name).trigger('input');
                toggleDropdown(false);
            });
            $('#folderSearch').on('input', function () {
                const term = $(this).val().toLowerCase();
                $('.folder-item').each(function () {
                    const name = $(this).find('.folder-name').text().toLowerCase();
                    $(this).toggle(name.includes(term));
                });
            });
            $list.show();
        } else {
            $list.append('<div class="list-group-item text-muted">Папки не найдены</div>').show();
        }
    }

    $('#clearSearch').on('click', function () {
        $('#folderSearch').val('').trigger('input').focus();
    });
    $('#clearCode').on('click', function () {
        $('#codeInput').val('').focus();
    });

    $('#searchButton').on('click', function () {
        const folder = $('#folderSearch').val().trim();
        const code = $('#codeInput').val().trim();
        if (!code) return alert('Пожалуйста, введите код');
        searchImages(folder, code);
    });

    function searchImages(folder, code) {
        $('#randomImagesContainer').hide();
        const key = `${IMAGES_CACHE_KEY}_${code}`;
        const cached = getCachedData(key);
        if (cached) {
            displayImages(cached.data);
            updateStatus(`Изображения загружены из кеша (${code})`, cached.timestamp);
            return;
        }
        $('#loadingImages').show();
        $('#imagesContainer').empty();
        if(folder === null || folder === '') {
            $.ajax({
                url: `${BASE_URL}image-view-urls-all?code=${encodeURIComponent(code)}`,
                type: 'GET',
                dataType: 'json',
                success: function (data) {
                    const timestamp = Date.now();
                    localStorage.setItem(key, JSON.stringify({ data, timestamp }));
                    $('#loadingImages').hide();
                    displayImages(data);
                    updateStatus(`Изображения загружены с API (${code})`, timestamp);
                },
                error: function () {
                    $('#loadingImages').hide();
                    $('#imagesContainer').html('<div class="alert alert-danger">Не удалось загрузить изображения. Попробуйте снова.</div>');
                }
            });
        }
        else{
            $.ajax({
                url: `${BASE_URL}image-view-urls?code=${encodeURIComponent(code)}&folder_name=${encodeURIComponent(folder)}`,
                type: 'GET',
                dataType: 'json',
                success: function (data) {
                    const timestamp = Date.now();
                    localStorage.setItem(key, JSON.stringify({ data, timestamp }));
                    $('#loadingImages').hide();
                    displayImages(data);
                    updateStatus(`Изображения загружены с API (${folder}/${code})`, timestamp);
                },
                error: function () {
                    $('#loadingImages').hide();
                    $('#imagesContainer').html('<div class="alert alert-danger">Не удалось загрузить изображения. Попробуйте снова.</div>');
                }
            });
        }
        
    }

    function displayImages(images) {
        const $container = $('#imagesContainer').empty();

        if (!images.length) {
            $container.html('<div class="alert alert-info">Изображения по заданным параметрам не найдены</div>');
            return;
        }

        if (images.length === 1) {
            // Одно изображение с лоадером
            const item = $(`
            <div class="image-item text-center p-3">
                <div class="image-loader">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Загрузка...</span>
                    </div>
                </div>
                <img src="${images[0]}" class="img-fluid" loading="lazy" alt="S3 Image">
            </div>
        `);

            const img = item.find('img');
            const loader = item.find('.image-loader');

            img.on('load', function() {
                img.addClass('loaded');
                loader.remove();
            }).on('error', function() {
                loader.remove();
                $(this).remove();
            });

            $container.append(item);
            return;
        }

        // Если больше одной картинки — карусель
        const carouselId = 'imageCarousel';

        let indicators = '';
        let items = '';

        images.forEach((url, index) => {
            indicators += `
        <button type="button" data-bs-target="#${carouselId}" data-bs-slide-to="${index}" 
            ${index === 0 ? 'class="active" aria-current="true"' : ''} 
            aria-label="Слайд ${index + 1}">
        </button>`;

            items += `
        <div class="carousel-item ${index === 0 ? 'active' : ''}">
            <div class="image-item text-center p-3">
                <div class="image-loader">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Загрузка...</span>
                    </div>
                </div>
                <img src="${url}" class="d-block mx-auto img-fluid" loading="lazy" alt="S3 Image">
            </div>
        </div>`;
        });

        const carouselHtml = `
    <div id="${carouselId}" class="carousel slide" data-bs-ride="carousel">
        <div class="carousel-indicators">
            ${indicators}
        </div>
        <div class="carousel-inner">
            ${items}
        </div>
        <button class="carousel-control-prev" type="button" data-bs-target="#${carouselId}" data-bs-slide="prev">
            <span aria-hidden="true">
                <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" fill="#0d6efd" viewBox="0 0 16 16">
                    <path fill-rule="evenodd" d="M11.354 1.646a.5.5 0 0 1 0 .708L5.707 8l5.647 5.646a.5.5 0 0 1-.708.708l-6-6a.5.5 0 0 1 0-.708l6-6a.5.5 0 0 1 .708 0z"/>
                </svg>
            </span>
            <span class="visually-hidden">Назад</span>
        </button>
        
        <button class="carousel-control-next" type="button" data-bs-target="#${carouselId}" data-bs-slide="next">
            <span aria-hidden="true">
                <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" fill="#0d6efd" viewBox="0 0 16 16">
                    <path fill-rule="evenodd" d="M4.646 1.646a.5.5 0 0 1 .708 0l6 6a.5.5 0 0 1 0 .708l-6 6a.5.5 0 0 1-.708-.708L10.293 8 4.646 2.354a.5.5 0 0 1 0-.708z"/>
                </svg>
            </span>
            <span class="visually-hidden">Вперёд</span>
        </button>
    </div>`;

        $container.html(carouselHtml);

        // Добавляем обработчики событий для всех изображений в карусели
        $container.find('img').each(function() {
            const img = $(this);
            const loader = img.siblings('.image-loader');

            img.on('load', function() {
                img.addClass('loaded');
                loader.remove();
            }).on('error', function() {
                loader.remove();
                img.remove();

                // Если в карусели не осталось изображений, показываем сообщение
                if ($container.find('img').length === 0) {
                    $container.html('<div class="alert alert-info">Не удалось загрузить изображения</div>');
                }
            });
        });
    }

    function updateStatus(message, timestamp) {
        const date = new Date(timestamp);
        $('#lastUpdated').html(`Последнее обновление: ${date.toLocaleString()}`);
        $('#cacheStatus').html(`Статус: ${message}`);
    }

    $('#retryLink').click(function (e) {
        e.preventDefault();
        fetchFolders();
    });

    $(document).on('click', function (e) {
        if (!$(e.target).closest('#folderSearchContainer').length) toggleDropdown(false);
    });

    $(document).on('click', '.copy-url-btn', function () {
        const $btn = $(this);
        const original = $btn.html();
        const url = $btn.data('url');
        navigator.clipboard.writeText(url).then(() => {
            $btn.html('<i class="fas fa-check me-1"></i>Скопировано!');
            $btn.prop('disabled', true);
            setTimeout(() => {
                $btn.html(original);
                $btn.prop('disabled', false);
            }, 1500);
        });
    });

    function loadRandomImages() {
        const cached = getCachedData(RANDOM_CACHE_KEY);
        if (cached) {
            const shuffled = shuffleArray([...cached.data]);
            displayRandomImages(shuffled);
            return;
        }

        $.ajax({
            url: RANDOM_IMAGES_URL,
            type: 'GET',
            dataType: 'json',
            success: function (images) {
                const timestamp = Date.now();
                localStorage.setItem(RANDOM_CACHE_KEY, JSON.stringify({ data: images, timestamp }));
                displayRandomImages(images);
            },
            error: function () {
                $('#randomImagesContainer').html('<div class="alert alert-warning">Не удалось загрузить случайные изображения</div>');
            }
        });
    }

    function shuffleArray(array) {
        for (let i = array.length - 1; i > 0; i--) {
            const j = Math.floor(Math.random() * (i + 1));
            [array[i], array[j]] = [array[j], array[i]];
        }
        return array;
    }

    function displayRandomImages(images) {
        const $container = $('#randomImagesContainer').empty();

        if (!images.length) {
            $container.html('<div class="alert alert-info">Нет изображений для отображения</div>');
            return;
        }

        images.forEach(url => {
            const item = $(`
                            <div class="col">
                                <div class="mini-image-item text-center">
                                    <div class="image-loader">
                                        <div class="spinner-border text-primary" role="status">
                                            <span class="visually-hidden">Загрузка...</span>
                                        </div>
                                    </div>
                                    <img src="${url}" class="img-fluid" loading="lazy" alt="S3 Image">
                                </div>
                            </div>
                        `);

            const img = item.find('img');
            const loader = item.find('.image-loader');

            img.on('load', function () {
                img.addClass('loaded');
                loader.remove(); 
            }).on('error', function () {
                loader.remove();
                $(this).remove();
            });

            $container.append(item);
        });

    }
});
