# Скрипт PowerShell для сборки и публикации образа TecDoc API в Docker Hub
# Использование: .\docker-push.ps1 [VERSION]
# Если версия не указана, публикуется только тег amd64-v2

param(
    [string]$Version = ""
)

$ImageName = "bondarevevgeni/tecdoc-api"
$PrimaryTag = "${ImageName}:amd64-v2"
$VersionTag = if ([string]::IsNullOrEmpty($Version)) { "" } else { "${ImageName}:${Version}" }

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Сборка и публикация образа TecDoc API" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
if ([string]::IsNullOrEmpty($VersionTag)) {
    Write-Host "Тег: $PrimaryTag" -ForegroundColor Yellow
} else {
    Write-Host "Теги: $PrimaryTag и $VersionTag" -ForegroundColor Yellow
}
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Сборка образа..." -ForegroundColor Green
if ([string]::IsNullOrEmpty($VersionTag)) {
    docker buildx build --platform linux/amd64 -t $PrimaryTag --push .
} else {
    docker buildx build --platform linux/amd64 -t $PrimaryTag -t $VersionTag --push .
}

if ($LASTEXITCODE -ne 0) {
    Write-Host "Ошибка при сборке образа!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Образ успешно собран!" -ForegroundColor Green
Write-Host ""

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "✅ Образ успешно опубликован!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Primary: $PrimaryTag" -ForegroundColor Yellow
if (-not [string]::IsNullOrEmpty($VersionTag)) {
    Write-Host "Version: $VersionTag" -ForegroundColor Yellow
}
Write-Host "==========================================" -ForegroundColor Cyan

