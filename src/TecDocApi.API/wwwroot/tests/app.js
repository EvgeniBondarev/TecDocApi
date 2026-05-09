const indexStatus = document.getElementById('indexStatus');
const searchOutput = document.getElementById('searchOutput');
const imageOutput = document.getElementById('imageOutput');
const articleImageOutput = document.getElementById('articleImageOutput');
const globalS3Output = document.getElementById('globalS3Output');

function escapeHtml(value) {
  return String(value ?? '')
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#39;');
}

async function timedFetch(url, options) {
  const startedAt = performance.now();
  const response = await fetch(url, options);
  const elapsedMs = performance.now() - startedAt;
  return { response, elapsedMs };
}

function statusCard(title, data) {
  const percent = Math.max(0, Math.min(100, Number(data.completionPercent ?? 0)));
  return `
    <div class="status-card">
      <div class="meta-label">${escapeHtml(title)}</div>
      <div class="meta-value">${escapeHtml(data.indexedDocuments)} / ${escapeHtml(data.sourceTotal)}</div>
      <div class="muted">Заполнено: ${escapeHtml(data.completionPercent)}%</div>
      <div class="muted">Осталось: ${escapeHtml(data.remainingDocuments)}</div>
      <div class="meter"><span data-width="${percent}"></span></div>
    </div>`;
}

async function refreshIndexStatus() {
  indexStatus.innerHTML = '<div class="muted">Загрузка статуса индексации...</div>';
  try {
    const { response } = await timedFetch('/api/search-diagnostics/index-status');
    const data = await response.json();
    if (!response.ok) {
      throw new Error(data.message || 'Не удалось получить статус индексации');
    }

    indexStatus.innerHTML = [
      statusCard('Articles index', data.articles),
      statusCard('Suppliers index', data.suppliers)
    ].join('');
    indexStatus.querySelectorAll('[data-width]').forEach((element) => {
      element.style.width = `${element.getAttribute('data-width')}%`;
    });
  } catch (error) {
    indexStatus.innerHTML = `<div class="error">${escapeHtml(error.message)}</div>`;
  }
}

function renderSearchResult(url, requestBody, payload, elapsedMs, httpStatus) {
  const items = Array.isArray(payload.items) ? payload.items : [];
  const itemsMarkup = items.length === 0
    ? '<div class="muted">Результатов нет.</div>'
    : `<div class="result-list">${items.map(item => `
      <article class="result-item">
        <h4>${escapeHtml(item.foundString?.trim() || item.dataSupplierArticleNumber || 'Без артикула')}</h4>
        <p><strong>Название:</strong> ${escapeHtml(item.normalizedDescription || '-')}</p>
        <p><strong>Поставщик:</strong> ${escapeHtml(item.supplierDescription || '-')} (ID ${escapeHtml(item.supplierId ?? '-')})</p>
        <p><strong>Описание:</strong> ${escapeHtml(item.description || '-')}</p>
        <p><strong>Артикул для запроса картинок:</strong> ${escapeHtml(item.dataSupplierArticleNumber || '-')}</p>
      </article>`).join('')}</div>`;

  searchOutput.innerHTML = `
    <div class="result-meta">
      <div class="meta-box"><div class="meta-label">HTTP</div><div class="meta-value">${escapeHtml(httpStatus)}</div></div>
      <div class="meta-box"><div class="meta-label">Время</div><div class="meta-value">${elapsedMs.toFixed(2)} ms</div></div>
      <div class="meta-box"><div class="meta-label">Всего</div><div class="meta-value">${escapeHtml(payload.total ?? 0)}</div></div>
      <div class="meta-box"><div class="meta-label">Elasticsearch took</div><div class="meta-value">${escapeHtml(payload.took ?? 0)} ms</div></div>
    </div>
    <div class="request-box"><div class="meta-label">Full URL</div><code>${escapeHtml(new URL(url, window.location.origin).toString())}</code></div>
    <div class="request-box panel-gap"><div class="meta-label">Request body</div><code>${escapeHtml(JSON.stringify(requestBody, null, 2))}</code></div>
    <div class="panel-gap">${itemsMarkup}</div>
    <details class="panel-gap" open>
      <summary>Исходный JSON</summary>
      <div class="json-box mt-12"><pre>${escapeHtml(JSON.stringify(payload, null, 2))}</pre></div>
    </details>`;
}

function renderImagesMarkup(images) {
  if (!Array.isArray(images) || images.length === 0) {
    return '<p class="muted"><strong>Картинки:</strong> нет</p>';
  }

  return `
    <div class="image-gallery">
      ${images.map(image => `
        <div class="gallery-card">
          <img class="image-preview" src="${escapeHtml(image.s3Url || image.url)}" alt="${escapeHtml(image.pictureName)}" loading="eager" decoding="async">
          <p><strong>Файл:</strong> ${escapeHtml(image.pictureName)}</p>
          <p><strong>Тип:</strong> ${escapeHtml(image.documentType || '-')}</p>
          <p><strong>Описание:</strong> ${escapeHtml(image.description || '-')}</p>
          <p><a href="${escapeHtml(image.url)}" target="_blank" rel="noreferrer">Открыть файл</a></p>
          <p><a href="${escapeHtml(image.streamUrl)}" target="_blank" rel="noreferrer">Открыть stream</a></p>
          ${image.s3Url ? `<p><a href="${escapeHtml(image.s3Url)}" target="_blank" rel="noreferrer">Открыть S3</a></p>` : ''}
        </div>`).join('')}
    </div>`;
}

function markBrokenPreview(imageElement) {
  const card = imageElement.closest('.gallery-card, .image-box');
  if (!card || card.querySelector('.image-preview-note')) {
    return;
  }

  const note = document.createElement('p');
  note.className = 'image-preview-note muted';
  note.textContent = 'Превью не поддерживается браузером или файл поврежден. Используйте ссылки открытия файла ниже.';
  card.insertBefore(note, imageElement.nextSibling);
  imageElement.style.display = 'none';
}

function attachPreviewFallbacks(rootElement) {
  rootElement.querySelectorAll('img.image-preview').forEach((imageElement) => {
    const applyState = () => {
      if (imageElement.complete && imageElement.naturalWidth === 0) {
        markBrokenPreview(imageElement);
      }
    };

    imageElement.addEventListener('error', () => markBrokenPreview(imageElement), { once: true });
    imageElement.addEventListener('load', applyState, { once: true });
    applyState();
  });
}

function renderArticleImageSearchResult(supplierId, articleNumber, payload, elapsedMs, httpStatus) {
  articleImageOutput.innerHTML = `
    <div class="result-meta">
      <div class="meta-box"><div class="meta-label">HTTP</div><div class="meta-value">${escapeHtml(httpStatus)}</div></div>
      <div class="meta-box"><div class="meta-label">Время</div><div class="meta-value">${elapsedMs.toFixed(2)} ms</div></div>
      <div class="meta-box"><div class="meta-label">Supplier ID</div><div class="meta-value">${escapeHtml(supplierId)}</div></div>
      <div class="meta-box"><div class="meta-label">Article Number</div><div class="meta-value">${escapeHtml(articleNumber)}</div></div>
    </div>
    <div class="request-box"><div class="meta-label">Endpoint</div><code>${escapeHtml(new URL(`/api/Images/article-search?supplierId=${encodeURIComponent(supplierId)}&articleNumber=${encodeURIComponent(articleNumber)}`, window.location.origin).toString())}</code></div>
    <div class="panel-gap">${renderImagesMarkup(payload)}</div>
    <details class="panel-gap" open>
      <summary>Исходный JSON</summary>
      <div class="json-box mt-12"><pre>${escapeHtml(JSON.stringify(payload, null, 2))}</pre></div>
    </details>`;

  attachPreviewFallbacks(articleImageOutput);
}

function renderGlobalS3SearchResult(articleNumber, maxResults, payload, elapsedMs, httpStatus) {
  globalS3Output.innerHTML = `
    <div class="result-meta">
      <div class="meta-box"><div class="meta-label">HTTP</div><div class="meta-value">${escapeHtml(httpStatus)}</div></div>
      <div class="meta-box"><div class="meta-label">Время</div><div class="meta-value">${elapsedMs.toFixed(2)} ms</div></div>
      <div class="meta-box"><div class="meta-label">Article Number</div><div class="meta-value">${escapeHtml(articleNumber)}</div></div>
      <div class="meta-box"><div class="meta-label">Max Results</div><div class="meta-value">${escapeHtml(maxResults)}</div></div>
    </div>
    <div class="request-box"><div class="meta-label">Endpoint</div><code>${escapeHtml(new URL(`/api/Images/s3-search?articleNumber=${encodeURIComponent(articleNumber)}&maxResults=${encodeURIComponent(maxResults)}`, window.location.origin).toString())}</code></div>
    <div class="panel-gap">${renderImagesMarkup(payload)}</div>
    <details class="panel-gap" open>
      <summary>Исходный JSON</summary>
      <div class="json-box mt-12"><pre>${escapeHtml(JSON.stringify(payload, null, 2))}</pre></div>
    </details>`;

  attachPreviewFallbacks(globalS3Output);
}

async function runArticleSearch() {
  const query = document.getElementById('articleQuery').value.trim();
  const mode = document.getElementById('searchMode').value;
  const supplierId = document.getElementById('supplierId').value.trim();
  if (!query) {
    searchOutput.innerHTML = '<div class="error">Нужно заполнить запрос.</div>';
    return;
  }

  const url = '/api/ArticleSearch/search';
  const requestBody = {
    query,
    searchMode: mode,
    page: 1,
    pageSize: 10,
    sortBy: 'relevance'
  };

  if (supplierId) {
    requestBody.supplierId = Number(supplierId);
  }

  searchOutput.innerHTML = '<div class="muted">Выполняю запрос...</div>';
  try {
    const { response, elapsedMs } = await timedFetch(url, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(requestBody)
    });
    const payload = await response.json();
    renderSearchResult(url, requestBody, payload, elapsedMs, response.status);
  } catch (error) {
    searchOutput.innerHTML = `<div class="error">${escapeHtml(error.message)}</div>`;
  }
}

async function loadImage() {
  const supplierId = document.getElementById('imageSupplierId').value.trim();
  const fileName = document.getElementById('imageFileName').value.trim();
  if (!supplierId || !fileName) {
    imageOutput.innerHTML = '<div class="error">Нужно указать supplierId и fileName.</div>';
    return;
  }

  const basePath = `/api/Images/${encodeURIComponent(supplierId)}/${encodeURIComponent(fileName)}`;
  imageOutput.innerHTML = '<div class="muted">Проверяю картинку...</div>';

  try {
    const [meta, exists] = await Promise.all([
      fetch(basePath).then(async r => ({ status: r.status, body: await r.json() })),
      fetch(`${basePath}/exists`).then(async r => ({ status: r.status, body: await r.json() }))
    ]);

    imageOutput.innerHTML = `
      <div class="grid two">
        <div class="image-box">
          <div class="meta-label">Public URL</div>
          <code>${escapeHtml(meta.body.url)}</code>
          <p><a href="${escapeHtml(meta.body.url)}" target="_blank" rel="noreferrer">Открыть ссылку</a></p>
          <img class="image-preview" src="${escapeHtml(meta.body.url)}" alt="Preview">
        </div>
        <div class="image-box">
          <div class="meta-label">Exists response</div>
          <code>${escapeHtml(JSON.stringify(exists.body, null, 2))}</code>
          <div class="meta-label panel-gap">Stream URL</div>
          <code>${escapeHtml(new URL(`${basePath}/stream`, window.location.origin).toString())}</code>
          <p><a href="${escapeHtml(`${basePath}/stream`)}" target="_blank" rel="noreferrer">Открыть stream</a></p>
        </div>
      </div>`;

    attachPreviewFallbacks(imageOutput);
  } catch (error) {
    imageOutput.innerHTML = `<div class="error">${escapeHtml(error.message)}</div>`;
  }
}

async function loadArticleImages() {
  const supplierId = document.getElementById('articleImageSupplierId').value.trim();
  const articleNumber = document.getElementById('articleImageArticleNumber').value.trim();
  if (!supplierId || !articleNumber) {
    articleImageOutput.innerHTML = '<div class="error">Нужно указать supplierId и articleNumber.</div>';
    return;
  }

  articleImageOutput.innerHTML = '<div class="muted">Ищу картинки по артикулу...</div>';
  try {
    const query = new URLSearchParams({ supplierId, articleNumber });
    const { response, elapsedMs } = await timedFetch(`/api/Images/article-search?${query.toString()}`);
    const payload = await response.json();
    if (!response.ok) {
      throw new Error(payload.message || 'Не удалось получить картинки');
    }

    renderArticleImageSearchResult(supplierId, articleNumber, payload, elapsedMs, response.status);
  } catch (error) {
    articleImageOutput.innerHTML = `<div class="error">${escapeHtml(error.message)}</div>`;
  }
}

async function loadGlobalS3Search() {
  const articleNumber = document.getElementById('globalArticleNumber').value.trim();
  const maxResults = document.getElementById('globalMaxResults').value.trim() || '10';
  if (!articleNumber) {
    globalS3Output.innerHTML = '<div class="error">Нужно указать articleNumber.</div>';
    return;
  }

  globalS3Output.innerHTML = '<div class="muted">Ищу картинки по всем папкам S3...</div>';
  try {
    const query = new URLSearchParams({ articleNumber, maxResults });
    const { response, elapsedMs } = await timedFetch(`/api/Images/s3-search?${query.toString()}`);
    const payload = await response.json();
    if (!response.ok) {
      throw new Error(payload.message || 'Не удалось выполнить глобальный поиск по S3');
    }

    renderGlobalS3SearchResult(articleNumber, maxResults, payload, elapsedMs, response.status);
  } catch (error) {
    globalS3Output.innerHTML = `<div class="error">${escapeHtml(error.message)}</div>`;
  }
}

document.getElementById('runArticleSearch').addEventListener('click', runArticleSearch);
document.getElementById('loadImage').addEventListener('click', loadImage);
document.getElementById('loadArticleImages').addEventListener('click', loadArticleImages);
document.getElementById('loadGlobalS3Search').addEventListener('click', loadGlobalS3Search);
refreshIndexStatus();