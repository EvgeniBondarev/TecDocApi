#!/bin/bash

# Скрипт для сборки и публикации образа TecDoc API в Docker Hub
# Использование: ./docker-push.sh [VERSION]
# Если версия не указана, используется текущая дата в формате YYYY.MM.DD

set -e

# Определяем версию
if [ -z "$1" ]; then
    VERSION=$(date +%Y.%m.%d)
else
    VERSION=$1
fi

IMAGE_NAME="bondarevevgeni/tecdoc-api"
LATEST_TAG="${IMAGE_NAME}:latest"
VERSION_TAG="${IMAGE_NAME}:${VERSION}"

echo "=========================================="
echo "Сборка и публикация образа TecDoc API"
echo "=========================================="
echo "Версия: ${VERSION}"
echo "Теги: ${LATEST_TAG} и ${VERSION_TAG}"
echo "=========================================="
echo ""

# Сборка образа с двумя тегами
echo "Сборка образа..."
docker build -t ${LATEST_TAG} -t ${VERSION_TAG} .

echo ""
echo "Образ успешно собран!"
echo ""

# Публикация образа latest
echo "Публикация образа ${LATEST_TAG}..."
docker push ${LATEST_TAG}

echo ""
echo "Публикация образа ${VERSION_TAG}..."
docker push ${VERSION_TAG}

echo ""
echo "=========================================="
echo "✅ Образ успешно опубликован!"
echo "=========================================="
echo "Latest: ${LATEST_TAG}"
echo "Version: ${VERSION_TAG}"
echo "=========================================="

