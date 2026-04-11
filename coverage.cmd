@echo off
setlocal
powershell -ExecutionPolicy Bypass -File "%~dp0scripts\test-coverage.ps1"
exit /b %errorlevel%
