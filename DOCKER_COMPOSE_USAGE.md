# Использование Docker Compose файлов

Проект содержит два файла Docker Compose для разных сценариев использования:

## 📦 `docker-compose.yml` - Для сервера/продакшена

Использует опубликованный образ из Docker Hub: `bondarevevgeni/tecdoc-api:latest`

**Использование:**
```bash
# Запуск
docker-compose up -d

# Остановка
docker-compose down

# Обновление образа
docker-compose pull api
docker-compose up -d
```

**Особенности:**
- ✅ Быстрый запуск (не нужно собирать образ)
- ✅ Всегда использует последнюю версию из Docker Hub
- ✅ Подходит для серверов и продакшена
- ⚠️ Требует публикации образа перед использованием

## 🔧 `docker-compose.local.yml` - Для локальной разработки

Собирает образ локально из Dockerfile

**Использование:**
```bash
# Запуск с локальной сборкой
docker-compose -f docker-compose.local.yml up -d --build

# Остановка
docker-compose -f docker-compose.local.yml down

# Пересборка после изменений кода
docker-compose -f docker-compose.local.yml up -d --build
```

**Особенности:**
- ✅ Можно тестировать изменения без публикации образа
- ✅ Быстрая итерация при разработке
- ✅ Использует окружение Development
- ⚠️ Требует больше времени на первый запуск (сборка образа)

## 📝 Различия

| Параметр | docker-compose.yml | docker-compose.local.yml |
|----------|-------------------|-------------------------|
| Образ | `bondarevevgeni/tecdoc-api:latest` | Локальная сборка |
| Окружение | Production | Development |
| Скорость запуска | Быстро | Медленнее (нужна сборка) |
| Обновление | `docker-compose pull` | `docker-compose build` |
| Использование | Сервер/Продакшен | Локальная разработка |

## 🚀 Рекомендации

### Для разработки:
```bash
# Используйте локальный файл для тестирования изменений
docker-compose -f docker-compose.local.yml up -d --build
```

### Для сервера:
```bash
# Используйте основной файл с опубликованным образом
docker-compose up -d
```

### Workflow разработки:

1. **Внесите изменения в код**
2. **Протестируйте локально:**
   ```bash
   docker-compose -f docker-compose.local.yml up -d --build
   ```
3. **Если все работает, соберите и опубликуйте образ:**
   ```bash
   docker build -t bondarevevgeni/tecdoc-api:latest .
   docker push bondarevevgeni/tecdoc-api:latest
   ```
4. **На сервере обновите образ:**
   ```bash
   docker-compose pull api
   docker-compose up -d
   ```

## 🔄 Переключение между файлами

Если вы используете один и тот же набор контейнеров, остановите текущие перед переключением:

```bash
# Остановить текущие контейнеры
docker-compose down
# или
docker-compose -f docker-compose.local.yml down

# Запустить с другим файлом
docker-compose up -d
# или
docker-compose -f docker-compose.local.yml up -d --build
```

