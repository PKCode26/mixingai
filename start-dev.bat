@echo off
setlocal
cd /d "%~dp0"
title MixingAI Dev-Start
powershell.exe -NoProfile -ExecutionPolicy Bypass -NoExit -File "%~dp0scripts\start-dev.ps1"
