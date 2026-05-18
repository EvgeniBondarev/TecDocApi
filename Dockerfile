# Dockerfile для сборки образа TecDoc API
# Опубликованный образ: bondarevevgeni/tecdoc-api:amd64-v2
# 
# Для сборки и публикации linux/amd64:
#   docker buildx build --platform linux/amd64 -t bondarevevgeni/tecdoc-api:amd64-v2 -t bondarevevgeni/tecdoc-api:1.0.0 --push .
#
# Или используйте скрипты:
#   Windows: .\docker-push.ps1 [VERSION]
#   Linux/Mac: ./docker-push.sh [VERSION]

# Этап 1: Сборка приложения
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем файлы проектов для восстановления зависимостей
COPY ["src/TecDocApi.API/TecDocApi.API.csproj", "src/TecDocApi.API/"]
COPY ["src/TecDocApi.Application/TecDocApi.Application.csproj", "src/TecDocApi.Application/"]
COPY ["src/TecDocApi.Domain/TecDocApi.Domain.csproj", "src/TecDocApi.Domain/"]
COPY ["src/TecDocApi.Infrastructure/TecDocApi.Infrastructure.csproj", "src/TecDocApi.Infrastructure/"]

# Восстанавливаем зависимости
RUN dotnet restore "src/TecDocApi.API/TecDocApi.API.csproj"

# Копируем весь исходный код
COPY . .

# Собираем приложение в режиме Release
WORKDIR "/src/src/TecDocApi.API"
RUN dotnet build "TecDocApi.API.csproj" -c Release -o /app/build

# Этап 2: Публикация приложения
FROM build AS publish
RUN dotnet publish "TecDocApi.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Этап 3: Финальный образ для запуска
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Устанавливаем curl для healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Создаем пользователя для запуска приложения (безопасность)
RUN useradd -m -s /bin/bash appuser && chown -R appuser:appuser /app
USER appuser

# Копируем опубликованное приложение из этапа publish
COPY --from=publish /app/publish .

# Открываем порт для приложения
EXPOSE 8082

# Переменные окружения
ENV ASPNETCORE_URLS=http://+:8082
ENV ASPNETCORE_ENVIRONMENT=Production

# Точка входа
ENTRYPOINT ["dotnet", "TecDocApi.API.dll"]