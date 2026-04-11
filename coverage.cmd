@echo off
setlocal
set "DOTNET_CLI_HOME=%~dp0.dotnet"
if not exist "%DOTNET_CLI_HOME%" mkdir "%DOTNET_CLI_HOME%"
powershell -ExecutionPolicy Bypass -File "%~dp0scripts\test-coverage.ps1"
exit /b %errorlevel%
