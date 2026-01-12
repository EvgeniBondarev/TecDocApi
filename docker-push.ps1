# Скрипт PowerShell для сборки и публикации образа TecDoc API в Docker Hub
# Использование: .\docker-push.ps1 [VERSION]
# Если версия не указана, используется текущая дата в формате YYYY.MM.DD

param(
    [string]$Version = ""
)

# Определяем версию
if ([string]::IsNullOrEmpty($Version)) {
    $Version = Get-Date -Format "yyyy.MM.dd"
}

$ImageName = "bondarevevgeni/tecdoc-api"
$LatestTag = "${ImageName}:latest"
$VersionTag = "${ImageName}:${Version}"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Сборка и публикация образа TecDoc API" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Версия: $Version" -ForegroundColor Yellow
Write-Host "Теги: $LatestTag и $VersionTag" -ForegroundColor Yellow
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Сборка образа с двумя тегами
Write-Host "Сборка образа..." -ForegroundColor Green
docker build -t $LatestTag -t $VersionTag .

if ($LASTEXITCODE -ne 0) {
    Write-Host "Ошибка при сборке образа!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Образ успешно собран!" -ForegroundColor Green
Write-Host ""

# Публикация образа latest
Write-Host "Публикация образа $LatestTag..." -ForegroundColor Green
docker push $LatestTag

if ($LASTEXITCODE -ne 0) {
    Write-Host "Ошибка при публикации latest!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Публикация образа $VersionTag..." -ForegroundColor Green
docker push $VersionTag

if ($LASTEXITCODE -ne 0) {
    Write-Host "Ошибка при публикации версии!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "✅ Образ успешно опубликован!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Latest: $LatestTag" -ForegroundColor Yellow
Write-Host "Version: $VersionTag" -ForegroundColor Yellow
Write-Host "==========================================" -ForegroundColor Cyan

