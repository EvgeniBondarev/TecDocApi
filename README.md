# TecDoc API

Современное ASP.NET Core Web API приложение для работы с каталогом автозапчастей TecDoc с поддержкой быстрого полнотекстового поиска через Elasticsearch.

## 📋 Содержание

- [Описание](#описание)
- [Возможности](#возможности)
- [Быстрый старт](#-быстрый-старт)
- [Архитектура](#архитектура)
- [Технологии](#технологии)
- [API Эндпоинты](#api-эндпоинты)
- [Настройка](#настройка)
- [Elasticsearch](#elasticsearch)
- [Docker](#docker)
- [Локальная разработка](#локальная-разработка)
- [Производительность](#производительность)
- [Мониторинг](#мониторинг)
- [Структура базы данных](#структура-базы-данных)

## Описание

TecDoc API предоставляет RESTful интерфейс для работы с каталогом автозапчастей TecDoc. Приложение поддерживает:

- **Быстрый поиск** артикулов и поставщиков через Elasticsearch
- **Классический поиск** через MySQL для точных запросов
- **Автоматическую синхронизацию** данных из MySQL в Elasticsearch
- **Полнотекстовый поиск** с поддержкой русского языка
- **Частичный поиск** по номерам артикулов (ngram)

## Возможности

### 🔍 Поиск

- **Elasticsearch поиск** - быстрый полнотекстовый поиск по артикулам и поставщикам
- **MySQL поиск** - точный поиск по номеру артикула и ID поставщика
- **Ngram анализ** - поиск по части артикула (например, "123" найдет "12345")
- **Изображения из S3** - быстрый доступ к изображениям из S3 хранилища с кэшированием
- **Русский язык** - поддержка морфологии и стемминга для русского языка
- **Fuzzy search** - поиск с учетом опечаток
- **Сортировка** - по релевантности, номеру артикула, описанию, количеству артикулов

### 🏗️ Архитектура

- **Clean Architecture** - разделение на слои Domain, Application, Infrastructure, API
- **Repository Pattern** - абстракция доступа к данным
- **Unit of Work** - управление транзакциями
- **DTOs** - разделение доменных моделей и моделей представления
- **Dependency Injection** - управление зависимостями

### 🚀 Производительность

- **Response Caching** - кэширование ответов для повышения производительности
- **Memory Cache** - кэширование часто запрашиваемых данных
- **AsNoTracking** - отключение отслеживания изменений для read-only запросов
- **DbContextPool** - пул контекстов для снижения накладных расходов
- **Rate Limiting** - защита от DDoS и brute-force атак

### 🔒 Безопасность

- **Rate Limiting** - ограничение количества запросов
- **Security Headers** - защитные заголовки HTTP
- **Global Exception Handler** - централизованная обработка ошибок
- **Валидация** - FluentValidation для валидации входных данных

## 🚀 Быстрый старт

### Через Docker (рекомендуется)

```bash
# 1. Создайте файл .env из примера
cp .env.example .env

# 2. Отредактируйте .env и укажите строку подключения к БД
# TECDOC_DATABASE_CONNECTION_STRING=Server=your-server;Port=3306;Database=TD2018;User=your-user;Password=your-password;...

# 3. Запустите приложение
docker-compose up -d --build

# 4. Проверьте логи
docker-compose logs -f api
```

После запуска:
- **API:** http://localhost:8082
- **Swagger UI:** http://localhost:8082/swagger
- **ReDoc:** http://localhost:8082/docs
- **Elasticsearch:** http://localhost:9200
- **Kibana:** http://localhost:5601

### Локальный запуск

```bash
# 1. Установите .NET 8.0 SDK
# 2. Скопируйте appsettings.Development.json.example в appsettings.Development.json
# 3. Заполните строку подключения к БД
# 4. Запустите приложение
dotnet run --project src/TecDocApi.API
```

## Архитектура

Проект построен на основе **Clean Architecture** и состоит из следующих слоев:

```
TecDocApi/
├── TecDocApi.Domain/          # Доменный слой
│   ├── Entities/             # Сущности базы данных
│   └── Interfaces/           # Интерфейсы репозиториев
│
├── TecDocApi.Application/    # Слой приложения
│   ├── Models/               # DTOs и модели для Elasticsearch
│   ├── Services/            # Бизнес-логика и сервисы
│   ├── Mappings/            # AutoMapper профили
│   └── Validators/         # FluentValidation валидаторы
│
├── TecDocApi.Infrastructure/ # Инфраструктурный слой
│   ├── Data/                # DbContext и конфигурация EF Core
│   └── Repositories/       # Реализация репозиториев
│
└── TecDocApi.API/           # Слой представления
    ├── Controllers/         # API контроллеры
    ├── DTOs/               # Data Transfer Objects
    ├── Middleware/         # Промежуточное ПО
    └── Extensions/         # Расширения для конфигурации
```

### Основные компоненты

- **Domain Layer** - содержит сущности и интерфейсы, не зависит от других слоев
- **Application Layer** - содержит бизнес-логику, сервисы и DTOs
- **Infrastructure Layer** - реализация доступа к данным (Entity Framework Core, Elasticsearch)
- **API Layer** - контроллеры, middleware, конфигурация приложения

## Технологии

### Основной стек

- **.NET 8.0** - платформа разработки
- **ASP.NET Core Web API** - фреймворк для создания RESTful API
- **Entity Framework Core 8.0** - ORM для работы с базой данных
- **MySQL** (Pomelo.EntityFrameworkCore.MySql) - база данных

### Поиск и индексация

- **Elasticsearch 8.18.8** - распределенный поисковый движок
- **Kibana 8.18.8** - визуализация и анализ данных Elasticsearch
- **NEST** - .NET клиент для Elasticsearch

### Вспомогательные библиотеки

- **AutoMapper** - маппинг объектов
- **FluentValidation** - валидация входных данных
- **Swashbuckle.AspNetCore** - генерация Swagger документации
- **Swashbuckle.AspNetCore.Filters** - примеры для Swagger
- **Polly** - обработка временных сбоев (retry, circuit breaker)

### Инфраструктура

- **Docker & Docker Compose** - контейнеризация и оркестрация
- **Health Checks** - проверка состояния сервисов

## API Эндпоинты

### 🔍 Elasticsearch поиск (рекомендуется для быстрого поиска)

#### Поиск артикулов

**POST** `/api/ArticleSearch/search`
- Поиск по полям `FoundString` и `NormalizedDescription`
- Поддерживает частичный поиск (ngram), нечеткий поиск и сортировку
- **Тело запроса:**
  ```json
  {
    "query": "12345",
    "page": 1,
    "pageSize": 20,
    "supplierId": null,
    "sortBy": "relevance",
    "sortDescending": false
  }
  ```

**POST** `/api/ArticleSearch/search-by-supplier`
- Поиск артикулов по модели поставщика (`SupplierDescription`, `SupplierMatchcode`)
- **Тело запроса:** аналогично `/search`

**GET** `/api/ArticleSearch/health`
- Проверка состояния индекса артикулов в Elasticsearch

#### Поиск поставщиков

**POST** `/api/SupplierSearch/search`
- Поиск по полям `Description` и `Matchcode`
- **Тело запроса:**
  ```json
  {
    "query": "BOSCH",
    "page": 1,
    "pageSize": 20,
    "sortBy": "relevance",
    "sortDescending": false
  }
  ```

**GET** `/api/SupplierSearch/health`
- Проверка состояния индекса поставщиков в Elasticsearch

### 📊 Классический поиск (MySQL)

#### Артикулы

**GET** `/api/v1/articles/search?articleNumber={номер}&supplierId={id}`
- Поиск артикулов по номеру и ID поставщика
- Возвращает полную информацию об артикуле, включая кросс-номера, изображения и т.д.

**GET** `/api/v1/articles/{supplierId}/{articleNumber}`
- Получение артикула по точному совпадению
- Возвращает полную информацию со всеми связями

**GET** `/api/v1/articles/search/ean/{eanCode}`
- Поиск артикула и поставщика по EAN коду (штрих-коду)
- Возвращает полную информацию об артикуле с поставщиком
- **Пример:** `GET /api/v1/articles/search/ean/4001512345678`
- **Ответ:**
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
          "quantityPerPackingUnit": 4
        },
        "supplier": {
          "id": 101,
          "description": "BOSCH",
          "matchcode": "BOSCH"
        },
        "eanCodes": [
          { "ean": "4001512345678" }
        ],
        "crosses": [...],
        "oeNumbers": [...],
        "images": [...]
      }
    ]
  }
  ```

#### Поставщики

**GET** `/api/v1/suppliers/search?matchcode={код}&supplierId={id}`
- Поиск поставщиков по matchcode или ID

### 🖼️ Изображения из S3

#### Получение URL изображения

**GET** `/api/Images/{supplierId}/{fileName}`
- Получение публичного URL изображения из S3 хранилища Timeweb
- Быстрый метод с кэшированием URL на 24 часа
- Автоматическая проверка существования изображения
- **Пример:** `GET /api/Images/101/101_116209_1.jpg`
- **Ответ:**
  ```json
  {
    "url": "https://s3.timeweb.cloud/25f554fc-.../TD2018/images/101/101_116209_1.jpg",
    "supplierId": 101,
    "fileName": "101_116209_1.jpg"
  }
  ```

**GET** `/api/Images/{supplierId}/{fileName}/stream`
- Получение изображения напрямую из S3 как поток данных
- Полезно для проксирования или дополнительной обработки
- Автоматическое определение Content-Type

**GET** `/api/Images/{supplierId}/{fileName}/exists`
- Быстрая проверка существования изображения в S3
- Не загружает файл, только проверяет метаданные
- **Ответ:**
  ```json
  {
    "exists": true,
    "supplierId": 101,
    "fileName": "101_116209_1.jpg"
  }
  ```
- Получение артикула по точному совпадению SupplierId и DataSupplierArticleNumber

#### Поставщики

**GET** `/api/v1/suppliers/search?matchcode={код}&id={id}`
- Поиск поставщиков по коду и ID

**GET** `/api/v1/suppliers/{id}`
- Получение поставщика по ID

### 📖 Документация API

- **Swagger UI:** http://localhost:8082/swagger - интерактивная документация с возможностью тестирования
- **ReDoc:** http://localhost:8082/docs - красивая документация с примерами

## Настройка

### Переменные окружения

#### Для Docker (файл `.env`)

Создайте файл `.env` в корне проекта на основе `.env.example`:

```env
# Строка подключения к базе данных MySQL
# ВАЖНО: Если пароль содержит специальные символы ($, }, (, ), и т.д.), 
# заключите значение в одинарные кавычки:
TECDOC_DATABASE_CONNECTION_STRING='Server=your-server;Port=3306;Database=TD2018;User=your-user;Password=your-password;Allow User Variables=true;MaximumPoolSize=50;ConnectionIdleTimeout=30;ConnectionTimeout=10;SslMode=None;AllowPublicKeyRetrieval=True;Charset=utf8mb4;'

# Настройки S3 для изображений
S3_ACCESS_KEY=your-access-key
S3_SECRET_KEY=your-secret-key
S3_ENDPOINT_URL=https://s3.timeweb.cloud
S3_REGION_NAME=ru-1
S3_BUCKET_NAME=your-bucket-name
```

**Важно:** 
- Файл `.env` содержит секретные данные и автоматически игнорируется Git
- Если пароль содержит специальные символы, используйте одинарные кавычки `'...'`
- После изменения `.env` перезапустите контейнер: `docker-compose restart api`

#### Для локальной разработки

Скопируйте `appsettings.Development.json.example` в `appsettings.Development.json` и заполните строку подключения:

```json
{
  "ConnectionStrings": {
    "TecDocDatabase": "Server=your-server;Port=3306;Database=TD2018;User=your-user;Password=your-password;..."
  }
}
```

### Конфигурация Elasticsearch

Настройки Elasticsearch находятся в `appsettings.json`:

```json
{
  "Elasticsearch": {
    "Url": "http://elasticsearch:9200",
    "IndexName": "articles",
    "SupplierIndexName": "suppliers",
    "BulkSize": 1000,
    "SupplierBulkSize": 500,
    "SyncIntervalMinutes": 5,
    "SupplierSyncIntervalMinutes": 10
  }
}
```

## Elasticsearch

### Архитектура поиска

Приложение использует Elasticsearch для быстрого полнотекстового поиска:

1. **Индексация данных:**
   - Артикулы индексируются в индекс `articles`
   - Поставщики индексируются в индекс `suppliers`
   - Автоматическая синхронизация из MySQL каждые 5-10 минут

2. **Особенности поиска:**
   - **Ngram анализ** - поиск по части артикула (например, "123" найдет "12345")
   - **Русский язык** - поддержка морфологии и стемминга для русского языка
   - **Fuzzy search** - поиск с учетом опечаток
   - **Релевантность** - результаты сортируются по релевантности с учетом boost значений

3. **Поля для поиска артикулов:**
   - `FoundString` - нормализованный номер артикула (boost 5.0 для точного совпадения, 3.0 для ngram)
   - `NormalizedDescription` - описание артикула (boost 2.0, полнотекстовый поиск)
   - `Description` - дополнительное описание (boost 1.0)
   - `SupplierDescription` - описание поставщика
   - `SupplierMatchcode` - код поставщика

4. **Поля для поиска поставщиков:**
   - `Matchcode` - код поставщика (boost 5.0 для точного совпадения, 4.0 для ngram)
   - `Description` - описание поставщика (boost 3.0, полнотекстовый поиск)

### Синхронизация данных

Данные автоматически синхронизируются из MySQL в Elasticsearch:

- **Полная синхронизация** - выполняется при первом запуске или если проиндексировано менее 90% данных
- **Инкрементальная синхронизация** - выполняется каждые 5 минут для артикулов и каждые 10 минут для поставщиков
- **Логирование** - все операции синхронизации логируются с подробной информацией

### Управление индексами

Для управления индексами Elasticsearch можно использовать Kibana или прямые запросы:

```bash
# Просмотр всех индексов
curl http://localhost:9200/_cat/indices?v

# Просмотр структуры индекса articles
curl http://localhost:9200/articles/_mapping?pretty

# Просмотр структуры индекса suppliers
curl http://localhost:9200/suppliers/_mapping?pretty

# Поиск в индексе (пример)
curl -X POST "http://localhost:9200/articles/_search?pretty" \
  -H 'Content-Type: application/json' \
  -d '{
    "query": {
      "match": {
        "foundString": "12345"
      }
    }
  }'
```

## Docker

### Структура Docker Compose

Приложение использует Docker Compose для оркестрации следующих сервисов:

- **api** - ASP.NET Core приложение (порт 8082)
- **elasticsearch** - Elasticsearch сервер (порт 9200)
- **kibana** - Kibana dashboard (порт 5601)

### Команды Docker

```bash
# Запуск всех сервисов
docker-compose up -d --build

# Просмотр логов
docker-compose logs -f api
docker-compose logs -f elasticsearch

# Остановка всех сервисов
docker-compose down

# Остановка с удалением volumes (удалит данные Elasticsearch)
docker-compose down -v

# Перезапуск конкретного сервиса
docker-compose restart api

# Пересборка образа
docker-compose build --no-cache api
```

### Health Checks

Все сервисы имеют health checks:

- **Elasticsearch** - проверка доступности через `/_cluster/health`
- **API** - проверка доступности через `/swagger/index.html`

### Volumes

- `elasticsearch-data` - данные Elasticsearch (сохраняются между перезапусками)

## Локальная разработка

### Требования

- .NET 8.0 SDK
- MySQL сервер (или доступ к удаленной БД)
- Elasticsearch (опционально, можно использовать Docker)

### Настройка

1. **Скопируйте конфигурационные файлы:**
   ```bash
   cp src/TecDocApi.API/appsettings.Development.json.example src/TecDocApi.API/appsettings.Development.json
   ```

2. **Заполните строку подключения** в `appsettings.Development.json`

3. **Запустите Elasticsearch** (если используете локально):
   ```bash
   docker run -d -p 9200:9200 -p 9300:9300 -e "discovery.type=single-node" docker.elastic.co/elasticsearch/elasticsearch:8.18.8
   ```

4. **Запустите приложение:**
   ```bash
   dotnet run --project src/TecDocApi.API
   ```

### Миграции базы данных

Для создания миграций:

```bash
dotnet ef migrations add MigrationName --project src/TecDocApi.Infrastructure --startup-project src/TecDocApi.API
```

Для применения миграций:

```bash
dotnet ef database update --project src/TecDocApi.Infrastructure --startup-project src/TecDocApi.API
```

## Производительность

### Оптимизации

- **DbContextPool** - пул контекстов для снижения накладных расходов
- **AsNoTracking** - отключение отслеживания изменений для read-only запросов
- **Параллельные запросы** - использование SemaphoreSlim для контроля параллелизма
- **Кэширование** - Memory Cache и Response Cache для часто запрашиваемых данных
- **Elasticsearch** - быстрый поиск без нагрузки на MySQL

### Rate Limiting

- **API эндпоинты:** 100 запросов за 10 секунд
- **Поисковые эндпоинты:** 50 запросов за 10 секунд

### Таймауты

- **Запросы к БД:** 10 секунд
- **Elasticsearch запросы:** 2 минуты
- **HTTP клиенты:** 5 секунд

### Метрики производительности

В ответах Elasticsearch поиска доступно поле `took` - время выполнения запроса в миллисекундах. Типичные значения:

- Простой поиск: 5-20 мс
- Сложный поиск с фильтрами: 20-100 мс
- Поиск по большому количеству данных: до 500 мс

## Мониторинг

### Health Checks

```bash
# Проверка Elasticsearch для артикулов
curl http://localhost:8082/api/ArticleSearch/health

# Проверка Elasticsearch для поставщиков
curl http://localhost:8082/api/SupplierSearch/health
```

### Логирование

Все операции логируются с использованием встроенного логирования .NET:

- **Информационные сообщения** - о синхронизации, индексации
- **Предупреждения** - о превышении rate limit, проблемах с подключением
- **Ошибки** - с полным стеком вызовов и контекстом

### Kibana Dashboard

Kibana доступна по адресу http://localhost:5601 и позволяет:

- Просматривать индексированные данные
- Выполнять поисковые запросы
- Анализировать производительность поиска
- Создавать визуализации и дашборды
- Мониторить состояние индексов

### Метрики

В логах можно отслеживать:

- Время выполнения запросов (`took` в ответах Elasticsearch)
- Количество проиндексированных документов
- Статус синхронизации
- Ошибки подключения к Elasticsearch
- Rate limit события

## Структура базы данных

База данных TecDoc содержит следующие основные таблицы:

### Основные таблицы

- **articles** - Артикулы. Основная таблица с информацией об артикулах запчастей, содержит нормализованные номера для поиска, описания, флаги применяемости и статусы.
- **suppliers** - Поставщики. Таблица с информацией о производителях неоригинальных запчастей (бренды).
- **supplier_details** - Детали поставщиков. Адресная информация и контакты поставщиков.

### Связанные таблицы артикулов

- **article_cross** - Кросс-номера. Оригинальные номера производителей ТС для артикулов.
- **article_oe** - Оригинальные кросс-номера. OEM номера для артикулов.
- **article_attributes** - Характеристики/Критерии. Характеристики и критерии артикулов (размеры, параметры и т.д.).
- **article_images** - Изображения и файлы. Изображения и документы для артикулов.
- **article_li** - Применяемость. Информация о применяемости артикулов к транспортным средствам и узлам.
- **article_ean** - Штрих-коды. EAN штрих-коды для артикулов (формат EAN-13).
- **article_inf** - Информация/Описание. Дополнительная информация и описания для артикулов.
- **article_acc** - Сопутствующие товары/Аксессуары. Связи артикулов с сопутствующими товарами и аксессуарами.
- **article_nn** - Новые номера. Информация о новых номерах артикулов (замена старых номеров).

### Таблицы транспортных средств

- **manufacturers** - Производители. Производители транспортных средств (BMW, Mercedes и т.д.).
- **models** - Модели. Модели транспортных средств.
- **passanger_cars** - Легковые автомобили. Информация о легковых транспортных средствах.
- **passanger_car_attributes** - Атрибуты легковых автомобилей. Характеристики и атрибуты легковых автомобилей (объем двигателя, мощность и т.д.).

## Безопасность

### Файлы, которые НЕ должны попадать в Git

Следующие файлы содержат секретные данные и автоматически игнорируются через `.gitignore`:

- `.env` - переменные окружения для Docker
- `appsettings.Development.json` - конфигурация для локальной разработки
- `appsettings.Production.json` - конфигурация для production
- Все файлы баз данных (`*.db`, `*.sqlite`, `*.mdf`, и т.д.)

**Используйте `.example` файлы как шаблоны** - они содержат структуру без секретных данных.

### Рекомендации

- Никогда не коммитьте файлы с реальными паролями и ключами
- Используйте переменные окружения для production
- Регулярно проверяйте `.gitignore` на актуальность
- Используйте секреты менеджеры для production окружений

## Примеры использования

### Пример 1: Поиск артикулов через Elasticsearch

**Запрос:**
```bash
POST /api/ArticleSearch/search
Content-Type: application/json

{
  "query": "12345",
  "page": 1,
  "pageSize": 20,
  "sortBy": "relevance"
}
```

**Ответ:**
```json
{
  "items": [
    {
      "id": "7_12345",
      "supplierId": 7,
      "dataSupplierArticleNumber": "12345",
      "foundString": "12345",
      "normalizedDescription": "Фильтр масляный",
      "description": "Для легковых автомобилей",
      "articleStateDisplayValue": "Normal",
      "quantityPerPackingUnit": 1,
      "supplierDescription": "BOSCH",
      "supplierMatchcode": "BOS",
      "indexedAt": "2024-01-10T12:00:00Z"
    }
  ],
  "total": 1,
  "page": 1,
  "pageSize": 20,
  "totalPages": 1,
  "took": 15
}
```

### Пример 2: Поиск поставщиков через Elasticsearch

**Запрос:**
```bash
POST /api/SupplierSearch/search
Content-Type: application/json

{
  "query": "BOSCH",
  "page": 1,
  "pageSize": 20,
  "sortBy": "nbrOfArticles",
  "sortDescending": true
}
```

**Ответ:**
```json
{
  "items": [
    {
      "id": "7",
      "supplierId": 7,
      "description": "BOSCH",
      "matchcode": "BOS",
      "dataVersion": 2024,
      "nbrOfArticles": 15000,
      "hasNewVersionArticles": true,
      "indexedAt": "2024-01-10T12:00:00Z"
    }
  ],
  "total": 1,
  "page": 1,
  "pageSize": 20,
  "totalPages": 1,
  "took": 8
}
```

### Пример 3: Поиск артикулов через MySQL

**Запрос:**
```bash
GET /api/v1/articles/search?articleNumber=12345&supplierId=7
```

**Ответ:**
```json
{
  "articles": [
    {
      "article": {
        "supplierId": 7,
        "dataSupplierArticleNumber": "12345",
        "foundString": "12345",
        "normalizedDescription": "Фильтр масляный",
        "description": "Для легковых автомобилей",
        "articleStateDisplayValue": "Normal",
        "quantityPerPackingUnit": 1
      },
      "supplier": {
        "id": 7,
        "description": "BOSCH",
        "matchcode": "BOS"
      },
      "crosses": [],
      "oeNumbers": [],
      "images": []
    }
  ]
}
```

## Параметры сортировки

### Для артикулов (`sortBy`)

- `relevance` - по релевантности (по умолчанию)
- `foundString` - по номеру артикула
- `description` - по описанию

### Для поставщиков (`sortBy`)

- `relevance` - по релевантности (по умолчанию)
- `description` - по описанию
- `matchcode` - по коду
- `nbrOfArticles` - по количеству артикулов

## Лицензия

Этот проект является внутренним проектом и не предназначен для публичного распространения.

## Поддержка

Для вопросов и предложений обращайтесь к команде разработки.
