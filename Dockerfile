# Указываем базовый образ Python 3.13 (если доступен)
FROM python:3.13-slim

# Устанавливаем рабочую директорию внутри контейнера
WORKDIR /app

# Устанавливаем необходимые системные зависимости
# unixodbc и unixodbc-dev нужны для работы с ODBC
# curl и gnupg нужны для добавления репозитория Microsoft
# добавляем установку git
RUN apt-get update && apt-get install -y \
    unixodbc \
    unixodbc-dev \
    curl \
    gnupg \
    lsb-release \
    git \
    && rm -rf /var/lib/apt/lists/*

# Добавляем ключ Microsoft и репозиторий для драйвера ODBC
RUN curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > /etc/apt/trusted.gpg.d/microsoft.gpg && \
    curl https://packages.microsoft.com/config/ubuntu/22.04/prod.list > /etc/apt/sources.list.d/mssql-release.list

# Устанавливаем драйвер ODBC для MS SQL Server и sqlcmd
RUN apt-get update && \
    ACCEPT_EULA=Y apt-get install -y msodbcsql17 mssql-tools && \
    apt-get install -y odbcinst && \
    rm -rf /var/lib/apt/lists/*

# Добавляем sqlcmd в PATH
ENV PATH="$PATH:/opt/mssql-tools/bin"

# Копируем файл зависимостей в контейнер
COPY requirements.txt .

# Устанавливаем зависимости Python
RUN pip install --no-cache-dir -r requirements.txt

# Копируем весь проект в контейнер
COPY . .
COPY .env .env

# Открываем порт для приложения
EXPOSE 8000

# Указываем команду для запуска FastAPI-приложения
CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8000"]
