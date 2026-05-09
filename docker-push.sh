#!/bin/bash

# Скрипт для сборки и публикации образа TecDoc API в Docker Hub
# Использование: ./docker-push.sh [VERSION]
# Если версия не указана, публикуется только тег amd64-v2

set -e

IMAGE_NAME="bondarevevgeni/tecdoc-api"
PRIMARY_TAG="${IMAGE_NAME}:amd64-v2"

if [ -n "$1" ]; then
    VERSION_TAG="${IMAGE_NAME}:$1"
else
    VERSION_TAG=""
fi

echo "=========================================="
echo "Сборка и публикация образа TecDoc API"
echo "=========================================="
if [ -n "${VERSION_TAG}" ]; then
    echo "Теги: ${PRIMARY_TAG} и ${VERSION_TAG}"
else
    echo "Тег: ${PRIMARY_TAG}"
fi
echo "=========================================="
echo ""

echo "Сборка образа..."
if [ -n "${VERSION_TAG}" ]; then
    docker buildx build --platform linux/amd64 -t ${PRIMARY_TAG} -t ${VERSION_TAG} --push .
else
    docker buildx build --platform linux/amd64 -t ${PRIMARY_TAG} --push .
fi

echo ""
echo "Образ успешно собран!"
echo ""

echo ""
echo "=========================================="
echo "✅ Образ успешно опубликован!"
echo "=========================================="
echo "Primary: ${PRIMARY_TAG}"
if [ -n "${VERSION_TAG}" ]; then
    echo "Version: ${VERSION_TAG}"
fi
echo "=========================================="

