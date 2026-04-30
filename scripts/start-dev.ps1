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

# ---------------------------------------------------------------------------
# 1. Alle laufenden Instanzen beenden
# ---------------------------------------------------------------------------
Write-Host "1/5  Stoppe alle laufenden MixingAI-Instanzen ..." -ForegroundColor Cyan

# Nur MixingAI-dotnet-Prozesse beenden (nicht andere Projekte!)
$killed = 0
Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" -ErrorAction SilentlyContinue |
    Where-Object { $_.CommandLine -like '*MixingAI*' } |
    ForEach-Object {
        Write-Host "  Stop dotnet  PID $($_.ProcessId) (MixingAI.Api)" -ForegroundColor Yellow
        Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue
        $killed++
    }

# Nur node-Prozesse beenden die zu unserem Frontend gehoeren
Get-CimInstance Win32_Process -Filter "Name = 'node.exe'" -ErrorAction SilentlyContinue |
    Where-Object { $_.CommandLine -like "*$root*" -or $_.CommandLine -like '*mixingai*' } |
    ForEach-Object {
        Write-Host "  Stop node    PID $($_.ProcessId) (MixingAI-Frontend)" -ForegroundColor Yellow
        Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue
        $killed++
    }

# cmd.exe-Fenster die zu unserem Projekt gehoeren (Backend- oder Frontend-Shell)
Get-CimInstance Win32_Process -Filter "Name = 'cmd.exe'" -ErrorAction SilentlyContinue |
    Where-Object {
        $cl = $_.CommandLine
        $cl -like "*MixingAI*" -or
        $cl -like "*$root*" -or
        $cl -like "*ConnectionStrings*" -or
        ($cl -like "*npm*" -and $cl -like "*dev*")
    } |
    ForEach-Object {
        Write-Host "  Stop cmd     PID $($_.ProcessId)" -ForegroundColor Yellow
        Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue
        $killed++
    }

# Port-basierte Absicherung: alles was noch auf 5085 / 5173 lauscht
foreach ($port in @(5085, 5173)) {
    $lines = & netstat -ano | Select-String ":${port}\s+.*LISTENING"
    foreach ($line in $lines) {
        $parts = ($line.ToString().Trim() -split '\s+')
        $procId = $parts[-1]
        if ($procId -match '^\d+$' -and [int]$procId -gt 0) {
            Write-Host "  Port ${port}:  Stop PID $procId" -ForegroundColor Yellow
            Stop-Process -Id ([int]$procId) -Force -ErrorAction SilentlyContinue
            $killed++
        }
    }
}

if ($killed -gt 0) {
    Start-Sleep -Seconds 2
}
Write-Host "  Erledigt ($killed Prozesse beendet)." -ForegroundColor Green

# ---------------------------------------------------------------------------
# 2. dotnet Build-Server
# ---------------------------------------------------------------------------
Write-Host ""
Write-Host "2/5  dotnet Build-Server herunterfahren ..." -ForegroundColor Cyan
$prev = $ErrorActionPreference
$ErrorActionPreference = "SilentlyContinue"
& dotnet build-server shutdown | Out-Null
$ErrorActionPreference = $prev
Write-Host "  Erledigt." -ForegroundColor Green

# ---------------------------------------------------------------------------
# 3. Docker
# ---------------------------------------------------------------------------
Write-Host ""
Write-Host "3/5  Docker-Container neu starten (PostgreSQL + Ollama) ..." -ForegroundColor Cyan
docker compose -f "$root\docker-compose.dev.yml" down | Out-Null
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

# ---------------------------------------------------------------------------
# 4. Warte auf PostgreSQL
# ---------------------------------------------------------------------------
Write-Host ""
Write-Host "4/5  Warte auf PostgreSQL ..." -ForegroundColor Cyan
$attempts = 0
do {
    Start-Sleep -Seconds 2
    $attempts++
    & docker compose -f "$root\docker-compose.dev.yml" exec -T postgres pg_isready -U postgres | Out-Null
} while ($LASTEXITCODE -ne 0 -and $attempts -lt 30)

if ($LASTEXITCODE -ne 0) {
    Write-Host "  FEHLER: PostgreSQL nicht erreichbar nach $attempts Versuchen." -ForegroundColor Red
    exit 1
}
Write-Host "  PostgreSQL bereit." -ForegroundColor Green

# ---------------------------------------------------------------------------
# 5. Backend + Frontend starten
# ---------------------------------------------------------------------------
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
