# Базовый образ для ASP.NET
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app

# Настройка переменных среды для Kestrel
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

# Этап сборки приложения
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["NuGet.Config", "."]
COPY ["OzonOrdersWeb/OzonOrdersWeb.csproj", "OzonOrdersWeb/"]
COPY ["OzonRepositories/OzonRepositories.csproj", "OzonRepositories/"]
COPY ["OzonDomains/OzonDomains.csproj", "OzonDomains/"]
COPY ["AppRepository/Servcies.csproj", "AppRepository/"]
COPY ["TreeGrouping.Application/TreeGrouping.Application.csproj", "TreeGrouping.Application/"]
COPY ["PartsInfo/PartsInfo.csproj", "PartsInfo/"]
RUN dotnet restore "./OzonOrdersWeb/OzonOrdersWeb.csproj"
COPY . .
WORKDIR "/src/OzonOrdersWeb"
RUN dotnet build "./OzonOrdersWeb.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Этап публикации приложения
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./OzonOrdersWeb.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Финальный этап — готовый контейнер
FROM base AS final
WORKDIR /app

# Удаляем настройки HTTPS, так как SSL терминация на Nginx
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OzonOrdersWeb.dll"]