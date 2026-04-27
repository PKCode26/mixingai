@echo off
chcp 65001 >nul
title MixingAI Dev-Start

echo.
echo  ============================================
echo   MixingAI - Entwicklungsumgebung starten
echo  ============================================
echo.

cd /d "%~dp0"

echo [1/5] Alte Prozesse auf Port 5085 und 5173 beenden...
for /f "tokens=5" %%a in ('netstat -ano 2^>nul ^| findstr ":5085 " ^| findstr "LISTENING"') do (
    taskkill /F /PID %%a >nul 2>&1
)
for /f "tokens=5" %%a in ('netstat -ano 2^>nul ^| findstr ":5173 " ^| findstr "LISTENING"') do (
    taskkill /F /PID %%a >nul 2>&1
)
echo    Erledigt.

echo.
echo [2/5] Laufende Dev-Container stoppen...
docker compose -f docker-compose.dev.yml down >nul 2>&1
echo    Erledigt.

echo.
echo [3/5] Docker-Container starten (PostgreSQL + Ollama)...
docker compose -f docker-compose.dev.yml up -d
if errorlevel 1 (
    echo.
    echo  FEHLER: Docker konnte nicht gestartet werden.
    echo  Ist Docker Desktop aktiv und vollstaendig hochgefahren?
    echo.
    pause
    exit /b 1
)

echo.
echo [4/5] Warte auf PostgreSQL...
:wait_pg
docker compose -f docker-compose.dev.yml exec -T postgres pg_isready -U postgres >nul 2>&1
if errorlevel 1 (
    set /p dummy="."<nul
    timeout /t 2 /nobreak >nul
    goto wait_pg
)
echo    PostgreSQL bereit.

echo.
echo [5/5] Backend und Frontend starten...
start "MixingAI - Backend  [localhost:5085]" cmd /k "cd /d "%~dp0backend\MixingAI.Api" && set ConnectionStrings__DefaultConnection=Host=localhost;Port=5433;Database=mixingai_dev;Username=postgres;Password=devpassword && echo. && echo  Backend wird gestartet... && echo. && dotnet run"
timeout /t 2 /nobreak >nul
start "MixingAI - Frontend [localhost:5173]" cmd /k "cd /d "%~dp0frontend" && echo. && echo  Frontend wird gestartet... && echo. && npm run dev"

echo.
echo  ============================================
echo   Alles gestartet!
echo.
echo   Frontend:  http://localhost:5173
echo   Backend:   http://localhost:5085/health
echo   OpenAPI:   http://localhost:5085/openapi/v1.json
echo.
echo   Login:     admin / Admin123!
echo  ============================================
echo.

timeout /t 4 /nobreak >nul
start "" "http://localhost:5173"

exit
