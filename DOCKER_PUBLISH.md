# Публикация образа в Docker Hub

Инструкция по сборке и публикации образа TecDoc API в Docker Hub.

## Быстрый старт

### Windows (PowerShell)

```powershell
# С автоматической версией (дата)
.\docker-push.ps1

# С указанием версии
.\docker-push.ps1 -Version "1.0.0"
```

### Linux/Mac (Bash)

```bash
# Сделать скрипт исполняемым
chmod +x docker-push.sh

# С автоматической версией (дата)
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

### 2. Сборка образа с двумя тегами

```bash
# Определите версию (например, текущая дата)
VERSION=$(date +%Y.%m.%d)  # Linux/Mac
# или
$VERSION = Get-Date -Format "yyyy.MM.dd"  # PowerShell

# Сборка с двумя тегами
docker build -t bondarevevgeni/tecdoc-api:latest -t bondarevevgeni/tecdoc-api:$VERSION .
```

### 3. Публикация образов

```bash
# Публикация latest
docker push bondarevevgeni/tecdoc-api:latest

# Публикация версии
docker push bondarevevgeni/tecdoc-api:$VERSION
```

## Примеры версий

- **Семантическое версионирование:** `1.0.0`, `1.1.0`, `2.0.0`
- **Версия по дате:** `2026.01.12`, `2026.01.13`
- **Версия с коммитом:** `1.0.0-abc1234`

## Проверка публикации

После публикации проверьте образы на Docker Hub:

1. Откройте https://hub.docker.com/r/bondarevevgeni/tecdoc-api
2. Убедитесь, что оба тега (latest и версия) присутствуют
3. Проверьте размер образа и время последнего обновления

## Использование опубликованного образа

После публикации образ можно использовать в `docker-compose.yml`:

```yaml
services:
  api:
    image: bondarevevgeni/tecdoc-api:latest  # или конкретная версия
    pull_policy: always
    # ...
```

## Обновление версии в docker-compose.yml

Если хотите использовать конкретную версию вместо latest:

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

