# Указываем базовый образ Python 3.13 (если доступен)
FROM python:3.13-slim

# Устанавливаем рабочую директорию внутри контейнера
WORKDIR /app

# Копируем файл зависимостей в контейнер
COPY requirements.txt .

# Устанавливаем зависимости
RUN pip install --no-cache-dir -r requirements.txt

# Копируем весь проект в контейнер
COPY . .

# Открываем порт для приложения
EXPOSE 8000

# Указываем команду для запуска FastAPI-приложения
CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8000"]
