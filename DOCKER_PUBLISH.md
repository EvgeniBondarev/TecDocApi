# Публикация образа в Docker Hub

Инструкция по сборке и публикации образа TecDoc API в Docker Hub.

Перед публикацией убедитесь, что в репозитории нет локальных `.env`, generated reports и временных наборов тестовых данных: в публичный репозиторий должны попадать только код, документация и шаблоны конфигурации.

## Быстрый старт

### Windows (PowerShell)

```powershell
# Публикация тега amd64-v2
.\docker-push.ps1

# С указанием версии
.\docker-push.ps1 -Version "1.0.0"
```

### Linux/Mac (Bash)

```bash
# Сделать скрипт исполняемым
chmod +x docker-push.sh

# Публикация тега amd64-v2
./docker-push.sh

# С указанием версией
./docker-push.sh 1.0.0
```

## Ручная публикация

Если нужно выполнить команды вручную:

### 1. Вход в Docker Hub

```bash
docker login
```

Введите ваш username и password от Docker Hub.

### 2. Сборка образа

```bash
# Сначала подготовьте локальные переменные окружения
cp .env.example .env

# Сборка основного тега для сервера
docker buildx build --platform linux/amd64 -t bondarevevgeni/tecdoc-api:amd64-v2 --push .

# Опционально: дополнительный version tag
docker buildx build --platform linux/amd64 -t bondarevevgeni/tecdoc-api:amd64-v2 -t bondarevevgeni/tecdoc-api:$VERSION --push .
```

### 3. Публикация образов

```bash
# `buildx --push` уже публикует теги в Docker Hub
```

## Примеры версий

- **Семантическое версионирование:** `1.0.0`, `1.1.0`, `2.0.0`
- **Версия по дате:** `2026.01.12`, `2026.01.13`
- **Версия с коммитом:** `1.0.0-abc1234`

## Проверка публикации

После публикации проверьте образы на Docker Hub:

1. Откройте https://hub.docker.com/r/bondarevevgeni/tecdoc-api
2. Убедитесь, что тег `amd64-v2` и опциональный version tag присутствуют
3. Проверьте размер образа и время последнего обновления

## Использование опубликованного образа

После публикации образ можно использовать в `docker-compose.yml`:

```yaml
services:
  api:
    image: bondarevevgeni/tecdoc-api:amd64-v2  # или конкретная версия
    pull_policy: always
    # ...
```

## Обновление версии в docker-compose.yml

Если хотите использовать конкретную версию вместо amd64-v2:

```yaml
services:
  api:
    image: bondarevevgeni/tecdoc-api:2026.01.12  # конкретная версия
    pull_policy: always
    # ...
```

## Примечания

- **Размер образа:** Обычно около 200-300 MB (после сборки)
- **Время сборки:** Зависит от скорости интернета и процессора (обычно 5-15 минут)
- **Авторизация:** Убедитесь, что вы авторизованы в Docker Hub (`docker login`)
- **Права доступа:** Убедитесь, что у вас есть права на публикацию в репозиторий `bondarevevgeni/tecdoc-api`
- **Секреты:** Никогда не коммитьте `.env`, production `appsettings.*.json` и реальные S3 ключи в `docker-compose*.yml`

