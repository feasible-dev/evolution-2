@echo off
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Lancer-Evolution2.ps1"
if errorlevel 1 pause
