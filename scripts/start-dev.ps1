Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

$connStr = "Host=localhost;Port=5433;Database=mixingai_dev;Username=postgres;Password=devpassword"

Write-Host ""
Write-Host "  ============================================" -ForegroundColor Cyan
Write-Host "   MixingAI - Entwicklungsumgebung starten" -ForegroundColor Cyan
Write-Host "   Workspace: $root" -ForegroundColor DarkCyan
Write-Host "  ============================================" -ForegroundColor Cyan
Write-Host ""

# --- 1. Prozesse beenden ---
Write-Host "1/5  Stoppe laufende MixingAI-Prozesse ..." -ForegroundColor Cyan

# dotnet.exe Prozesse die MixingAI.Api ausfuehren
try {
    $dotnetProcs = Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" |
        Where-Object { $_.CommandLine -like '*MixingAI.Api*' }
    foreach ($p in $dotnetProcs) {
        Write-Host "  Stop dotnet PID $($p.ProcessId) (MixingAI.Api) ..." -ForegroundColor Yellow
        Stop-Process -Id $p.ProcessId -Force -ErrorAction SilentlyContinue
    }
} catch {
    Write-Host "  Warnung: dotnet-Prozesse konnten nicht geprueft werden." -ForegroundColor DarkYellow
}

# node.exe / npm fuer das Frontend (vite dev server)
try {
    $nodeProcs = Get-CimInstance Win32_Process -Filter "Name = 'node.exe'" |
        Where-Object { $_.CommandLine -like '*mixingai*' -or $_.CommandLine -like '*\frontend\*' }
    foreach ($p in $nodeProcs) {
        Write-Host "  Stop node PID $($p.ProcessId) (Frontend) ..." -ForegroundColor Yellow
        Stop-Process -Id $p.ProcessId -Force -ErrorAction SilentlyContinue
    }
} catch {
    Write-Host "  Warnung: node-Prozesse konnten nicht geprueft werden." -ForegroundColor DarkYellow
}

# Prozesse auf Port 5085 (Backend) und 5173 (Frontend) per Port-Scan
foreach ($port in @(5085, 5173)) {
    $netLines = netstat -ano 2>$null | Select-String ":$port\s.*LISTENING"
    foreach ($line in $netLines) {
        $pid = ($line -split '\s+') | Select-Object -Last 1
        if ($pid -match '^\d+$' -and [int]$pid -gt 0) {
            Write-Host "  Port $port: Stop PID $pid ..." -ForegroundColor Yellow
            Stop-Process -Id ([int]$pid) -Force -ErrorAction SilentlyContinue
        }
    }
}

# cmd.exe-Fenster die MixingAI-Sessions sind
try {
    $cmdProcs = Get-CimInstance Win32_Process -Filter "Name = 'cmd.exe'" |
        Where-Object { $_.CommandLine -like '*MixingAI*' }
    foreach ($p in $cmdProcs) {
        Write-Host "  Stop cmd PID $($p.ProcessId) (MixingAI-Session) ..." -ForegroundColor Yellow
        Stop-Process -Id $p.ProcessId -Force -ErrorAction SilentlyContinue
    }
} catch {}

Start-Sleep -Seconds 2
Write-Host "  Erledigt." -ForegroundColor Green

# --- 2. dotnet Build-Server ---
Write-Host ""
Write-Host "2/5  dotnet Build-Server herunterfahren ..." -ForegroundColor Cyan
try {
    dotnet build-server shutdown 2>$null | Out-Null
    Write-Host "  Erledigt." -ForegroundColor Green
} catch {
    Write-Host "  (kein Build-Server aktiv)" -ForegroundColor DarkGray
}

# --- 3. Docker ---
Write-Host ""
Write-Host "3/5  Docker-Container neu starten (PostgreSQL + Ollama) ..." -ForegroundColor Cyan
docker compose -f "$root\docker-compose.dev.yml" down 2>$null | Out-Null
docker compose -f "$root\docker-compose.dev.yml" up -d
if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "  FEHLER: Docker konnte nicht gestartet werden." -ForegroundColor Red
    Write-Host "  Ist Docker Desktop aktiv und vollstaendig hochgefahren?" -ForegroundColor Red
    Write-Host ""
    pause
    exit 1
}
Write-Host "  Container gestartet." -ForegroundColor Green

# --- 4. Warte auf PostgreSQL ---
Write-Host ""
Write-Host "4/5  Warte auf PostgreSQL ..." -ForegroundColor Cyan
$attempts = 0
do {
    Start-Sleep -Seconds 2
    $attempts++
    docker compose -f "$root\docker-compose.dev.yml" exec -T postgres pg_isready -U postgres 2>$null | Out-Null
} while ($LASTEXITCODE -ne 0 -and $attempts -lt 30)

if ($LASTEXITCODE -ne 0) {
    Write-Host "  FEHLER: PostgreSQL nicht erreichbar nach $attempts Versuchen." -ForegroundColor Red
    exit 1
}
Write-Host "  PostgreSQL bereit." -ForegroundColor Green

# --- 5. Backend + Frontend starten ---
Write-Host ""
Write-Host "5/5  Backend und Frontend starten ..." -ForegroundColor Cyan

$backendCmd = "cd /d `"$root\backend\MixingAI.Api`" && set ConnectionStrings__DefaultConnection=$connStr && echo. && echo  Backend wird gestartet... && echo. && dotnet run"
Start-Process cmd -ArgumentList "/k", $backendCmd

Start-Sleep -Seconds 2

$frontendCmd = "cd /d `"$root\frontend`" && echo. && echo  Frontend wird gestartet... && echo. && npm run dev"
Start-Process cmd -ArgumentList "/k", $frontendCmd

Write-Host ""
Write-Host "  ============================================" -ForegroundColor Cyan
Write-Host "   Alles gestartet!" -ForegroundColor Green
Write-Host ""
Write-Host "   Frontend:  http://localhost:5173" -ForegroundColor White
Write-Host "   Backend:   http://localhost:5085/health" -ForegroundColor White
Write-Host "   OpenAPI:   http://localhost:5085/openapi/v1.json" -ForegroundColor White
Write-Host ""
Write-Host "   Login:     admin / Admin123!" -ForegroundColor Yellow
Write-Host "  ============================================" -ForegroundColor Cyan
Write-Host ""

Start-Sleep -Seconds 5
Start-Process "http://localhost:5173"
