# Script para diagnosticar error 500 en IIS
# Ejecutar con privilegios de administrador

Write-Host "=== DIAGNÓSTICO DE ERROR 500 ===" -ForegroundColor Cyan
Write-Host ""

# 1. Verificar logs de IIS
Write-Host "1. Verificando logs de IIS..." -ForegroundColor Yellow
$iisLogPath = "C:\inetpub\logs\LogFiles"
if (Test-Path $iisLogPath) {
    Write-Host "   Ubicación logs IIS: $iisLogPath" -ForegroundColor Green
    $latestLog = Get-ChildItem -Path $iisLogPath -Recurse -Filter "*.log" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if ($latestLog) {
        Write-Host "   Último log: $($latestLog.FullName)" -ForegroundColor Green
        Write-Host "   Últimas 20 líneas:" -ForegroundColor Cyan
        Get-Content $latestLog.FullName -Tail 20
    }
} else {
    Write-Host "   No se encontró la carpeta de logs de IIS" -ForegroundColor Red
}

Write-Host ""

# 2. Verificar Event Viewer - Application logs
Write-Host "2. Verificando Event Viewer (últimos errores ASP.NET)..." -ForegroundColor Yellow
try {
    $events = Get-EventLog -LogName Application -Source "ASP.NET*" -EntryType Error -Newest 10 -ErrorAction SilentlyContinue
    if ($events) {
        foreach ($event in $events) {
            Write-Host "   [$($event.TimeGenerated)] $($event.Message.Substring(0, [Math]::Min(200, $event.Message.Length)))..." -ForegroundColor Red
        }
    } else {
        Write-Host "   No se encontraron errores recientes de ASP.NET" -ForegroundColor Green
    }
} catch {
    Write-Host "   No se pudo acceder al Event Viewer" -ForegroundColor Yellow
}

Write-Host ""

# 3. Verificar stdout logs de la aplicación
Write-Host "3. Buscando logs stdout de la aplicación..." -ForegroundColor Yellow
$possiblePaths = @(
    "C:\inetpub\wwwroot\BERRecepcion.Front\logs",
    "C:\Pemex\BER\bovedaelectronicarecepcion-frontend\BERRecepcion.Front\logs",
    "C:\Pemex\BER\bovedaelectronicarecepcion-frontend\BERRecepcion.Front\bin\Release\net8.0\logs",
    "C:\Pemex\BER\bovedaelectronicarecepcion-frontend\BERRecepcion.Front\bin\Debug\net8.0\logs"
)

$foundLogs = $false
foreach ($path in $possiblePaths) {
    if (Test-Path $path) {
        Write-Host "   Encontrada carpeta de logs: $path" -ForegroundColor Green
        $stdoutLogs = Get-ChildItem -Path $path -Filter "stdout_*.log" -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending
        if ($stdoutLogs) {
            $latestStdout = $stdoutLogs | Select-Object -First 1
            Write-Host "   Último log stdout: $($latestStdout.FullName)" -ForegroundColor Green
            Write-Host "   Últimas 30 líneas:" -ForegroundColor Cyan
            Get-Content $latestStdout.FullName -Tail 30
            $foundLogs = $true
            break
        }
    }
}

if (-not $foundLogs) {
    Write-Host "   No se encontraron logs stdout. Buscando en todas las ubicaciones..." -ForegroundColor Yellow
    Get-ChildItem -Path "C:\" -Filter "stdout_*.log" -Recurse -ErrorAction SilentlyContinue | 
        Sort-Object LastWriteTime -Descending | 
        Select-Object -First 5 | 
        ForEach-Object { Write-Host "   Encontrado: $($_.FullName)" -ForegroundColor Cyan }
}

Write-Host ""

# 4. Verificar Application Pools
Write-Host "4. Verificando Application Pools de IIS..." -ForegroundColor Yellow
Import-Module WebAdministration -ErrorAction SilentlyContinue
if (Get-Module WebAdministration) {
    $appPools = Get-ChildItem IIS:\AppPools | Where-Object { $_.Name -like "*BER*" -or $_.Name -like "*Recepcion*" }
    foreach ($pool in $appPools) {
        Write-Host "   Pool: $($pool.Name) - Estado: $($pool.State)" -ForegroundColor $(if ($pool.State -eq "Started") { "Green" } else { "Red" })
    }
    
    # Verificar sitios
    Write-Host ""
    Write-Host "   Sitios web relacionados:" -ForegroundColor Yellow
    $sites = Get-ChildItem IIS:\Sites | Where-Object { $_.Name -like "*BER*" -or $_.Name -like "*Recepcion*" }
    foreach ($site in $sites) {
        Write-Host "   Sitio: $($site.Name) - Estado: $($site.State)" -ForegroundColor $(if ($site.State -eq "Started") { "Green" } else { "Red" })
        Write-Host "   Path físico: $($site.PhysicalPath)" -ForegroundColor Cyan
    }
} else {
    Write-Host "   Módulo WebAdministration no disponible. Ejecutar como Administrador." -ForegroundColor Red
}

Write-Host ""

# 5. Verificar archivo web.config
Write-Host "5. Verificando web.config..." -ForegroundColor Yellow
$webConfigPaths = @(
    "C:\inetpub\wwwroot\BERRecepcion.Front\web.config",
    "C:\Pemex\BER\bovedaelectronicarecepcion-frontend\BERRecepcion.Front\web.config"
)

foreach ($configPath in $webConfigPaths) {
    if (Test-Path $configPath) {
        Write-Host "   Encontrado: $configPath" -ForegroundColor Green
        $config = Get-Content $configPath -Raw
        if ($config -match "stdoutLogEnabled=`"true`"") {
            Write-Host "   ✓ stdoutLogEnabled está habilitado" -ForegroundColor Green
        } else {
            Write-Host "   ✗ stdoutLogEnabled NO está habilitado" -ForegroundColor Red
        }
        
        if ($config -match "stdoutLogFile=`"([^`"]+)`"") {
            Write-Host "   Ruta de logs stdout: $($matches[1])" -ForegroundColor Cyan
        }
    }
}

Write-Host ""
Write-Host "=== FIN DEL DIAGNÓSTICO ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "RECOMENDACIONES:" -ForegroundColor Yellow
Write-Host "1. Revisa los logs arriba para identificar el error exacto" -ForegroundColor White
Write-Host "2. Si no hay logs, habilita stdoutLogEnabled en web.config" -ForegroundColor White
Write-Host "3. Recicla el Application Pool: Restart-WebAppPool 'NombreDelPool'" -ForegroundColor White
Write-Host "4. Si el error persiste, revisa el Event Viewer manualmente" -ForegroundColor White
