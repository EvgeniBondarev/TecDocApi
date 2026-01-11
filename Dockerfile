# Этап 1: Сборка приложения
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
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
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Устанавливаем curl для healthcheck (опционально, можно использовать wget)
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Создаем пользователя для запуска приложения (безопасность)
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Копируем опубликованное приложение из этапа publish
COPY --from=publish /app/publish .

# Открываем порт для приложения
EXPOSE 8080

# Переменные окружения
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Точка входа
ENTRYPOINT ["dotnet", "TecDocApi.API.dll"]

