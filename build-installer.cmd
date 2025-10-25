@echo off
REM ForceDNS Installer Build Script (Batch version)

echo =====================================
echo ForceDNS Installer Build Script
echo =====================================
echo.

powershell -ExecutionPolicy Bypass -File "%~dp0build-installer.ps1"

pause
