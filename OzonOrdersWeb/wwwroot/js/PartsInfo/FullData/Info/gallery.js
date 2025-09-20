// wwwroot/js/parts-info/gallery.js

function initGallery(imageUrls) {
    const gallery = document.querySelector('.gc-fotorama');
    if (!gallery) return;

    const mainPhoto = gallery.querySelector('.photo-main');
    const thumbnails = gallery.querySelectorAll('.thumbnail');
    const dots = gallery.querySelectorAll('.dot');
    const prevBtn = gallery.querySelector('.left-arrow');
    const nextBtn = gallery.querySelector('.right-arrow');

    let currentIndex = 0;
    const images = imageUrls || [];

    function decodeHtml(html) {
        const txt = document.createElement('textarea');
        txt.innerHTML = html;
        return txt.value;
    }

    function updateGallery(index, isModalUpdate = false) {
        currentIndex = index;

        thumbnails.forEach((thumb, i) => thumb.classList.toggle('active', i === currentIndex));
        dots.forEach((dot, i) => dot.classList.toggle('active-dot', i === currentIndex));

        const containers = mainPhoto.querySelectorAll('.container-img');
        containers.forEach((container, i) => {
            container.style.transform = i === currentIndex ? 'translateX(0)' : 'translateX(100%)';
            container.className = `container-img ${i === currentIndex ? 'current' : 'next'}`;
        });

        if (isModalUpdate) {
            updateModalGallery(index);
        }
    }

    thumbnails.forEach((thumb, index) => thumb.addEventListener('click', () => updateGallery(index)));
    dots.forEach((dot, index) => dot.addEventListener('click', () => updateGallery(index)));
    prevBtn.addEventListener('click', () => updateGallery((currentIndex - 1 + images.length) % images.length, true));
    nextBtn.addEventListener('click', () => updateGallery((currentIndex + 1) % images.length, true));

    // --- Modal Gallery Logic ---
    const modal = document.getElementById('galleryModal');
    const modalImgContainer = modal.querySelector('.modal-gallery-img-container');
    const modalImg = modal.querySelector('.modal-gallery-img');
    const modalClose = modal.querySelector('.modal-gallery-close');
    const modalPrev = modal.querySelector('.modal-gallery-prev');
    const modalNext = modal.querySelector('.modal-gallery-next');
    const modalThumbnails = modal.querySelector('.modal-gallery-thumbnails');

    modalThumbnails.innerHTML = ''; // Очищаем на случай повторной инициализации
    images.forEach((img, index) => {
        const thumbnail = document.createElement('div');
        thumbnail.className = 'modal-thumbnail';
        thumbnail.innerHTML = `<img src="${decodeHtml(img)}" alt="Миниатюра ${index + 1}">`;
        thumbnail.addEventListener('click', (e) => {
            e.stopPropagation();
            updateModalGallery(index);
            updateGallery(index); // Синхронизируем с основной галереей
        });
        modalThumbnails.appendChild(thumbnail);
    });

    function updateModalGallery(index) {
        currentIndex = index;
        if (modalImg) {
            modalImg.src = decodeHtml(images[currentIndex]);
            modalImg.alt = `Изображение ${currentIndex + 1}`;
        }
        const modalThumbs = modalThumbnails.querySelectorAll('.modal-thumbnail');
        modalThumbs.forEach((thumb, i) => thumb.classList.toggle('active', i === currentIndex));
    }

    function closeModal() {
        modal.style.display = 'none';
        document.body.style.overflow = 'auto';
    }

    mainPhoto.addEventListener('click', () => {
        modal.style.display = 'block';
        document.body.style.overflow = 'hidden';
        updateModalGallery(currentIndex);
    });

    modalClose.addEventListener('click', e => { e.stopPropagation(); closeModal(); });
    modal.addEventListener('click', e => { if (e.target === modal) closeModal(); });
    modalImgContainer.addEventListener('click', e => e.stopPropagation());
    modalThumbnails.addEventListener('click', e => e.stopPropagation());
    modalPrev.addEventListener('click', e => { e.stopPropagation(); updateModalGallery((currentIndex - 1 + images.length) % images.length); updateGallery(currentIndex); });
    modalNext.addEventListener('click', e => { e.stopPropagation(); updateModalGallery((currentIndex + 1) % images.length); updateGallery(currentIndex); });
    document.addEventListener('keydown', e => { if (e.key === 'Escape' && modal.style.display === 'block') closeModal(); });

    updateGallery(0);
}