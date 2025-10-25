@echo off
REM ForceDNS Release Build Script (Batch version)
REM This script builds the ForceDNS solution in Release configuration

echo =====================================
echo ForceDNS Release Build Script
echo =====================================
echo.

powershell -ExecutionPolicy Bypass -File "%~dp0build-release.ps1"

pause
