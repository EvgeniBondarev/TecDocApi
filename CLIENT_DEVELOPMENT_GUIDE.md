# Руководство по разработке клиента для TecDoc API

Этот документ содержит полные спецификации и правила для разработки клиента API, которые можно передать AI-ассистенту или разработчику для создания максимально полного и правильного кода со всеми нюансами.

## 📋 Содержание

1. [Общая информация](#общая-информация)
2. [Архитектура API](#архитектура-api)
3. [Базовые требования](#базовые-требования)
4. [Обработка ошибок](#обработка-ошибок)
5. [Rate Limiting](#rate-limiting)
6. [Пагинация](#пагинация)
7. [Типы данных](#типы-данных)
8. [Эндпоинты](#эндпоинты)
9. [Примеры использования](#примеры-использования)
10. [Рекомендации по реализации](#рекомендации-по-реализации)

---

## Общая информация

### Базовый URL

- **Production:** `http://localhost:8082`
- **Development:** `https://localhost:5001`

### Формат данных

- **Content-Type:** `application/json`
- **Кодировка:** UTF-8
- **Формат дат:** ISO 8601 (UTC), например: `2024-01-10T12:00:00Z`

### Версионирование

API использует версионирование через URL:
- Elasticsearch эндпоинты: `/api/ArticleSearch/*`, `/api/SupplierSearch/*` (без версии)
- MySQL эндпоинты: `/api/v1/articles/*`, `/api/v1/suppliers/*` (версия v1)

---

## Архитектура API

### Два типа поиска

1. **Elasticsearch поиск** (рекомендуется для быстрого поиска)
   - Быстрый полнотекстовый поиск
   - Поддержка частичного поиска (ngram)
   - Поддержка русского языка
   - Нечеткий поиск (fuzzy search)

2. **MySQL поиск** (для точных запросов)
   - Точный поиск по номеру артикула
   - Полная информация со всеми связями
   - Медленнее, но более детальная информация

### Группы эндпоинтов

- **ArticleSearch** - Поиск артикулов через Elasticsearch
- **SupplierSearch** - Поиск поставщиков через Elasticsearch
- **Articles** - Работа с артикулами через MySQL
- **Suppliers** - Работа с поставщиками через MySQL

---

## Базовые требования

### HTTP заголовки

Все запросы должны включать:

```http
Content-Type: application/json
Accept: application/json
```

### Кодировка

- Все строки должны быть в UTF-8
- Специальные символы должны быть правильно экранированы в JSON

### Таймауты

Рекомендуемые таймауты для клиента:

- **Elasticsearch запросы:** 30 секунд
- **MySQL запросы:** 15 секунд
- **Health checks:** 5 секунд

### Retry логика

Рекомендуется реализовать retry логику для временных ошибок:

- **Retry на:** 500, 502, 503, 504
- **Максимум попыток:** 3
- **Exponential backoff:** 1s, 2s, 4s
- **Не retry на:** 400, 401, 403, 404, 429

---

## Обработка ошибок

### Формат ошибок

Все ошибки возвращаются в едином формате:

```json
{
  "code": "BAD_REQUEST",
  "message": "Максимальный размер страницы: 100",
  "details": "Stack trace...",  // Только в Development
  "path": "/api/ArticleSearch/search",
  "timestamp": "2024-01-10T12:00:00Z"
}
```

### Коды ошибок

#### Общие ошибки

- `INTERNAL_SERVER_ERROR` (500) - Внутренняя ошибка сервера
- `VALIDATION_ERROR` (400) - Ошибка валидации данных
- `NOT_FOUND` (404) - Ресурс не найден
- `BAD_REQUEST` (400) - Некорректный запрос

#### Ошибки безопасности

- `UNAUTHORIZED` (401) - Не авторизован
- `FORBIDDEN` (403) - Доступ запрещен
- `RATE_LIMIT_EXCEEDED` (429) - Превышен лимит запросов

#### Ошибки бизнес-логики

- `CONFLICT` (409) - Конфликт данных
- `ARTICLE_NOT_FOUND` (404) - Артикул не найден
- `ARTICLE_INVALID` (400) - Некорректный артикул
- `SUPPLIER_NOT_FOUND` (404) - Поставщик не найден
- `SUPPLIER_INVALID` (400) - Некорректный поставщик

#### Ошибки базы данных

- `DATABASE_ERROR` (500) - Ошибка базы данных
- `DATABASE_TIMEOUT` (504) - Таймаут запроса к БД

### Обработка HTTP статусов

| Код | Значение | Действие клиента |
|-----|----------|------------------|
| 200 | OK | Успешный ответ, обработать данные |
| 400 | Bad Request | Проверить параметры запроса, показать ошибку пользователю |
| 401 | Unauthorized | Требуется авторизация |
| 403 | Forbidden | Доступ запрещен |
| 404 | Not Found | Ресурс не найден, показать сообщение пользователю |
| 429 | Too Many Requests | Превышен rate limit, подождать и повторить |
| 500 | Internal Server Error | Внутренняя ошибка сервера, можно повторить запрос |
| 503 | Service Unavailable | Сервис недоступен, можно повторить запрос |

### Рекомендации по обработке ошибок

1. **Всегда проверяйте HTTP статус код**
2. **Парсите ErrorResponse для получения деталей**
3. **Логируйте ошибки с полной информацией (код, сообщение, путь)**
4. **Показывайте пользователю понятные сообщения об ошибках**
5. **Не показывайте технические детали (details) в production**

---

## Rate Limiting

### Лимиты

- **API эндпоинты:** 100 запросов за 10 секунд
- **Поисковые эндпоинты:** 50 запросов за 10 секунд

### Заголовки ответа

При превышении лимита сервер возвращает:

```http
HTTP/1.1 429 Too Many Requests
Retry-After: 10
X-RateLimit-Limit: 50
X-RateLimit-Remaining: 0
X-RateLimit-Reset: 1641900000
```

### Рекомендации

1. **Отслеживайте заголовки Rate Limit** в ответах
2. **Реализуйте очередь запросов** для предотвращения превышения лимита
3. **Используйте Retry-After** для определения времени ожидания
4. **Кэшируйте результаты** для уменьшения количества запросов

---

## Пагинация

### Параметры пагинации

Все поисковые эндпоинты поддерживают пагинацию:

```json
{
  "page": 1,        // Номер страницы (начиная с 1)
  "pageSize": 20    // Размер страницы (1-100, по умолчанию 20)
}
```

### Формат ответа с пагинацией

```json
{
  "items": [...],
  "total": 150,           // Общее количество результатов
  "page": 1,              // Текущая страница
  "pageSize": 20,         // Размер страницы
  "totalPages": 8,        // Общее количество страниц
  "took": 15              // Время выполнения запроса (мс)
}
```

### Рекомендации

1. **Всегда используйте разумный pageSize** (20-50 для UI, до 100 для bulk операций)
2. **Проверяйте totalPages** перед запросом следующей страницы
3. **Кэшируйте первую страницу** для улучшения UX
4. **Используйте total** для отображения общего количества результатов

---

## Типы данных

### Числовые типы

- **int32** (integer): -2,147,483,648 до 2,147,483,647
- **int64** (long): -9,223,372,036,854,775,808 до 9,223,372,036,854,775,807
- **uint16** (ushort): 0 до 65,535 (для SupplierId)
- **uint32** (uint): 0 до 4,294,967,295

### Строковые типы

- **string**: UTF-8 строка
- **nullable string**: может быть null
- **Минимальная длина:** обычно 1 символ
- **Максимальная длина:** указана в описании поля

### Булевы типы

- **boolean**: true или false
- **nullable boolean**: может быть null (три состояния: true, false, null)

### Даты и время

- **date-time**: ISO 8601 формат в UTC
- **Пример:** `2024-01-10T12:00:00Z`
- **Всегда в UTC**, конвертируйте в локальное время при необходимости

### Массивы

- **array**: список элементов
- **Может быть пустым:** `[]`
- **Всегда инициализируется** (не null)

---

## Эндпоинты

### Elasticsearch поиск

#### POST /api/ArticleSearch/search

**Описание:** Поиск артикулов по FoundString и NormalizedDescription

**Тело запроса:**
```json
{
  "query": "12345",           // Поисковый запрос (обязательно)
  "page": 1,                 // Номер страницы (опционально, по умолчанию 1)
  "pageSize": 20,            // Размер страницы (опционально, по умолчанию 20, максимум 100)
  "supplierId": 7,           // Фильтр по поставщику (опционально)
  "sortBy": "relevance",     // Сортировка: "relevance", "foundString", "description"
  "sortDescending": false    // Направление сортировки
}
```

**Особенности:**
- `query` может быть null для получения всех артикулов (не рекомендуется)
- `pageSize` валидируется на сервере (максимум 100)
- `sortBy` по умолчанию "relevance"

**Ответ:**
```json
{
  "items": [ArticleDocument],
  "total": 150,
  "page": 1,
  "pageSize": 20,
  "totalPages": 8,
  "took": 15
}
```

#### POST /api/ArticleSearch/search-by-supplier

**Описание:** Поиск артикулов по модели поставщика (SupplierDescription, SupplierMatchcode)

**Тело запроса:** Аналогично `/search`

**Особенности:**
- Ищет по полям поставщика, а не самого артикула
- Полезно для поиска всех артикулов конкретного бренда

#### GET /api/ArticleSearch/health

**Описание:** Проверка состояния индекса артикулов

**Ответ:**
```json
{
  "status": "healthy",
  "indexExists": true,
  "indexedDocuments": 1500000,
  "timestamp": "2024-01-10T12:00:00Z"
}
```

#### POST /api/SupplierSearch/search

**Описание:** Поиск поставщиков по Description и Matchcode

**Тело запроса:**
```json
{
  "query": "BOSCH",
  "page": 1,
  "pageSize": 20,
  "sortBy": "relevance",     // "relevance", "description", "matchcode", "nbrOfArticles"
  "sortDescending": false
}
```

**Особенности:**
- `sortBy: "nbrOfArticles"` полезен для сортировки по популярности
- `sortDescending: true` для получения поставщиков с наибольшим количеством артикулов

#### GET /api/SupplierSearch/health

**Описание:** Проверка состояния индекса поставщиков

**Ответ:** Аналогично ArticleSearch/health

### MySQL поиск

#### GET /api/v1/articles/search

**Описание:** Поиск артикулов по номеру через MySQL

**Параметры запроса:**
- `articleNumber` (обязательно): строка, 1-100 символов
- `supplierId` (опционально): integer, 1-65535

**Особенности:**
- Артикул автоматически нормализуется (без учета регистра, пробелов, спецсимволов)
- Возвращает максимум 50 результатов
- Кэш: 5 минут
- Rate limit: 50 запросов за 10 секунд

**Пример:**
```
GET /api/v1/articles/search?articleNumber=ABC-123&supplierId=7
```

**Ответ:**
```json
{
  "count": 5,
  "results": [ArticleDto]
}
```

#### GET /api/v1/articles/search/ean/{eanCode}

**Описание:** Поиск артикула и поставщика по EAN коду (штрих-коду)

**Параметры пути:**
- `eanCode`: строка, 8-24 символов, EAN-13 штрих-код

**Особенности:**
- EAN код нормализуется автоматически (удаляются пробелы, приведение к верхнему регистру)
- Возвращает полную информацию об артикуле с поставщиком и всеми связями
- Если артикул имеет несколько EAN кодов, все они возвращаются в массиве `eanCodes`
- Кэш: 5 минут
- Rate limit: 50 запросов за 10 секунд

**Пример:**
```
GET /api/v1/articles/search/ean/4001512345678
```

**Ответ:**
```json
{
  "count": 1,
  "ean": "4001512345678",
  "results": [
    {
      "article": {
        "supplierId": 101,
        "dataSupplierArticleNumber": "ABC-123",
        "foundString": "ABC123",
        "normalizedDescription": "Тормозная колодка",
        "description": "Передняя тормозная колодка",
        "articleStateDisplayValue": "Valid",
        "quantityPerPackingUnit": 4,
        "flags": {
          "flagAccessory": false,
          "flagMaterialCertification": true,
          "flagRemanufactured": false,
          "flagSelfServicePacking": false,
          "hasAxle": false,
          "hasCommercialVehicle": false,
          "hasEngine": false,
          "hasLinkItems": true,
          "hasMotorbike": false,
          "hasPassengerCar": true,
          "isValid": true
        }
      },
      "supplier": {
        "id": 101,
        "description": "BOSCH",
        "matchcode": "BOSCH",
        "dataVersion": 2023,
        "nbrOfArticles": 5000
      },
      "eanCodes": [
        { "ean": "4001512345678" },
        { "ean": "4001512345679" }
      ],
      "crosses": [...],
      "oeNumbers": [...],
      "attributes": [...],
      "images": [...],
      "linkages": [...],
      "information": [...],
      "accessories": [...],
      "newNumbers": [...]
    }
  ]
}
```

**Коды ошибок:**
- **400** - Неверный формат EAN кода
- **404** - Артикул с указанным EAN кодом не найден
- **429** - Превышен лимит запросов
- **500** - Внутренняя ошибка сервера

**Примеры использования:**

JavaScript:
```javascript
async function searchByEan(eanCode) {
  const response = await fetch(`http://localhost:8082/api/v1/articles/search/ean/${eanCode}`, {
    method: 'GET',
    headers: { 'Accept': 'application/json' }
  });
  
  if (!response.ok) {
    if (response.status === 404) {
      console.log('Артикул не найден');
      return null;
    }
    throw new Error(`HTTP error! status: ${response.status}`);
  }
  
  return await response.json();
}

// Использование
const result = await searchByEan('4001512345678');
console.log(`Найдено артикулов: ${result.count}`);
```

Python:
```python
import requests

def search_by_ean(ean_code: str) -> dict:
    url = f"http://localhost:8082/api/v1/articles/search/ean/{ean_code}"
    response = requests.get(url, headers={"Accept": "application/json"})
    
    if response.status_code == 404:
        print("Артикул не найден")
        return None
    
    response.raise_for_status()
    return response.json()

# Использование
result = search_by_ean("4001512345678")
print(f"Найдено артикулов: {result['count']}")
```

C#:
```csharp
public async Task<ArticleSearchResponseDto> SearchByEanAsync(string eanCode)
{
    using var client = new HttpClient();
    client.BaseAddress = new Uri("http://localhost:8082");
    
    var response = await client.GetAsync($"/api/v1/articles/search/ean/{eanCode}");
    
    if (response.StatusCode == HttpStatusCode.NotFound)
    {
        Console.WriteLine("Артикул не найден");
        return null;
    }
    
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadFromJsonAsync<ArticleSearchResponseDto>();
}
```

#### GET /api/v1/articles/{supplierId}/{articleNumber}

**Описание:** Получение артикула по точному совпадению

**Параметры пути:**
- `supplierId`: integer, 1-65535
- `articleNumber`: строка, 1-100 символов

**Особенности:**
- Артикул автоматически нормализуется
- Возвращает полную информацию со всеми связями
- Кэш: 10 минут
- Rate limit: 50 запросов за 10 секунд

**Пример:**
```
GET /api/v1/articles/7/ABC-123
```

**Ответ:**
```json
{
  "article": ArticleInfoDto,
  "supplier": SupplierInfoDto,
  "crosses": [CrossDto],
  "oeNumbers": [OeNumberDto],
  "attributes": [AttributeDto],
  "images": [ImageDto],
  "linkages": [LinkageDto],
  "eanCodes": [EanCodeDto],
  "information": [InformationDto],
  "accessories": [AccessoryDto],
  "newNumbers": [NewNumberDto]
}
```

---

## Примеры использования

### Пример 1: Поиск артикулов через Elasticsearch

```javascript
// JavaScript/TypeScript пример
async function searchArticles(query, page = 1, pageSize = 20) {
  const response = await fetch('http://localhost:8082/api/ArticleSearch/search', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      query: query,
      page: page,
      pageSize: pageSize,
      sortBy: 'relevance',
      sortDescending: false
    })
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(`Ошибка поиска: ${error.message}`);
  }

  return await response.json();
}

// Использование
try {
  const result = await searchArticles('12345', 1, 20);
  console.log(`Найдено: ${result.total} артикулов`);
  console.log(`Время выполнения: ${result.took} мс`);
  result.items.forEach(article => {
    console.log(`${article.dataSupplierArticleNumber}: ${article.normalizedDescription}`);
  });
} catch (error) {
  console.error('Ошибка:', error.message);
}
```

### Пример 2: Обработка ошибок с retry

```javascript
async function searchWithRetry(query, maxRetries = 3) {
  let lastError;
  
  for (let attempt = 0; attempt < maxRetries; attempt++) {
    try {
      const response = await fetch('http://localhost:8082/api/ArticleSearch/search', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ query })
      });

      if (response.ok) {
        return await response.json();
      }

      // Проверка на rate limit
      if (response.status === 429) {
        const retryAfter = response.headers.get('Retry-After');
        const waitTime = retryAfter ? parseInt(retryAfter) * 1000 : Math.pow(2, attempt) * 1000;
        await new Promise(resolve => setTimeout(resolve, waitTime));
        continue;
      }

      // Проверка на временные ошибки
      if ([500, 502, 503, 504].includes(response.status)) {
        const waitTime = Math.pow(2, attempt) * 1000;
        await new Promise(resolve => setTimeout(resolve, waitTime));
        continue;
      }

      // Постоянные ошибки - не retry
      const error = await response.json();
      throw new Error(`Ошибка ${error.code}: ${error.message}`);
    } catch (error) {
      lastError = error;
      if (attempt < maxRetries - 1) {
        const waitTime = Math.pow(2, attempt) * 1000;
        await new Promise(resolve => setTimeout(resolve, waitTime));
      }
    }
  }

  throw lastError;
}
```

### Пример 3: Пагинация

```javascript
async function getAllArticles(query) {
  const allArticles = [];
  let page = 1;
  let hasMore = true;

  while (hasMore) {
    const result = await searchArticles(query, page, 100); // Максимальный pageSize
    
    allArticles.push(...result.items);
    
    hasMore = page < result.totalPages;
    page++;
    
    // Небольшая задержка между запросами для избежания rate limit
    if (hasMore) {
      await new Promise(resolve => setTimeout(resolve, 100));
    }
  }

  return allArticles;
}
```

### Пример 4: Python клиент

```python
import requests
import time
from typing import Optional, Dict, List

class TecDocAPIClient:
    def __init__(self, base_url: str = "http://localhost:8082"):
        self.base_url = base_url
        self.session = requests.Session()
        self.session.headers.update({
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        })

    def search_articles(
        self,
        query: str,
        page: int = 1,
        page_size: int = 20,
        supplier_id: Optional[int] = None,
        sort_by: str = "relevance",
        sort_descending: bool = False
    ) -> Dict:
        """Поиск артикулов через Elasticsearch"""
        url = f"{self.base_url}/api/ArticleSearch/search"
        payload = {
            "query": query,
            "page": page,
            "pageSize": page_size,
            "sortBy": sort_by,
            "sortDescending": sort_descending
        }
        
        if supplier_id:
            payload["supplierId"] = supplier_id

        try:
            response = self.session.post(url, json=payload, timeout=30)
            response.raise_for_status()
            return response.json()
        except requests.exceptions.HTTPError as e:
            if response.status_code == 429:
                retry_after = int(response.headers.get('Retry-After', 10))
                time.sleep(retry_after)
                return self.search_articles(query, page, page_size, supplier_id, sort_by, sort_descending)
            error_data = response.json()
            raise Exception(f"{error_data.get('code')}: {error_data.get('message')}")
        except requests.exceptions.RequestException as e:
            raise Exception(f"Ошибка сети: {str(e)}")

    def get_article(self, supplier_id: int, article_number: str) -> Dict:
        """Получение артикула по точному совпадению"""
        url = f"{self.base_url}/api/v1/articles/{supplier_id}/{article_number}"
        
        try:
            response = self.session.get(url, timeout=15)
            response.raise_for_status()
            return response.json()
        except requests.exceptions.HTTPError as e:
            if response.status_code == 404:
                error_data = response.json()
                raise Exception(f"Артикул не найден: {error_data.get('message')}")
            raise

# Использование
client = TecDocAPIClient()
result = client.search_articles("12345", page=1, page_size=20)
print(f"Найдено: {result['total']} артикулов")
```

---

## Рекомендации по реализации

### Структура клиента

1. **Базовый класс/модуль** для HTTP запросов
2. **Отдельные классы/модули** для каждой группы эндпоинтов
3. **Модели данных** для всех DTOs
4. **Обработка ошибок** централизованно
5. **Retry логика** для временных ошибок
6. **Кэширование** для часто запрашиваемых данных

### Обязательные функции

1. ✅ **Валидация входных параметров** перед отправкой запроса
2. ✅ **Обработка всех HTTP статусов** и кодов ошибок
3. ✅ **Retry логика** с exponential backoff
4. ✅ **Rate limit handling** с отслеживанием заголовков
5. ✅ **Таймауты** для всех запросов
6. ✅ **Логирование** запросов и ответов (опционально)
7. ✅ **Кэширование** результатов (опционально)
8. ✅ **Пагинация** с автоматической загрузкой всех страниц (опционально)

### Оптимизации

1. **Batch запросы:** Группируйте запросы когда возможно
2. **Кэширование:** Кэшируйте результаты поиска на короткое время
3. **Debouncing:** Используйте debounce для поиска в реальном времени
4. **Lazy loading:** Загружайте дополнительные данные по требованию
5. **Connection pooling:** Переиспользуйте HTTP соединения

### Тестирование

1. **Unit тесты** для всех методов клиента
2. **Integration тесты** с mock сервером
3. **Тесты обработки ошибок** для всех кодов ошибок
4. **Тесты rate limiting** для проверки retry логики
5. **Тесты пагинации** для проверки корректности загрузки страниц

### Документация кода

1. **XML комментарии** для всех публичных методов
2. **Примеры использования** в документации
3. **Описание параметров** с типами и ограничениями
4. **Описание возвращаемых значений** с примерами
5. **Описание возможных исключений**

---

## Чеклист для разработчика клиента

- [ ] Реализованы все эндпоинты API
- [ ] Обработка всех HTTP статусов (200, 400, 401, 403, 404, 429, 500, 503)
- [ ] Парсинг ErrorResponse для всех ошибок
- [ ] Валидация входных параметров
- [ ] Retry логика с exponential backoff
- [ ] Обработка Rate Limiting с заголовками
- [ ] Таймауты для всех запросов
- [ ] Поддержка пагинации
- [ ] Правильная обработка nullable полей
- [ ] Конвертация дат из UTC в локальное время
- [ ] Обработка пустых массивов
- [ ] Логирование ошибок
- [ ] Unit тесты
- [ ] Документация кода
- [ ] Примеры использования

---

## Дополнительные ресурсы

- **OpenAPI спецификация:** `openapi.yaml`
- **Swagger UI:** http://localhost:8082/swagger
- **ReDoc:** http://localhost:8082/docs
- **README:** `README.md`

---

**Версия документа:** 1.0.0  
**Последнее обновление:** 2024-01-10

