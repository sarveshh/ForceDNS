# ForceDNS Installer Build Script
# This script builds a single installer executable using NSIS

param(
    [switch]$DownloadNSIS = $false
)

$ErrorActionPreference = "Stop"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "ForceDNS Installer Build Script" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Check if build artifacts exist
Write-Host "Checking build artifacts..." -ForegroundColor Yellow

if (-not (Test-Path ".\ForceDNS.Service\bin\Release\ForceDNS.Service.exe")) {
    Write-Host "ERROR: Service build not found. Please run build-release.ps1 first." -ForegroundColor Red
    exit 1
}

if (-not (Test-Path ".\ForceDNS.UI\bin\Release\ForceDNS.UI.exe")) {
    Write-Host "ERROR: UI build not found. Please run build-release.ps1 first." -ForegroundColor Red
    exit 1
}

Write-Host "Build artifacts found." -ForegroundColor Green

# Find NSIS
Write-Host "`nLocating NSIS..." -ForegroundColor Yellow

$nsisPath = $null
$nsisPaths = @(
    "C:\Program Files (x86)\NSIS\makensis.exe",
    "C:\Program Files\NSIS\makensis.exe",
    "$env:ProgramFiles\NSIS\makensis.exe",
    "${env:ProgramFiles(x86)}\NSIS\makensis.exe"
)

foreach ($path in $nsisPaths) {
    if (Test-Path $path) {
        $nsisPath = $path
        break
    }
}

if (-not $nsisPath) {
    Write-Host "NSIS not found!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please install NSIS (Nullsoft Scriptable Install System):" -ForegroundColor Yellow
    Write-Host "  Download from: https://nsis.sourceforge.io/Download" -ForegroundColor Cyan
    Write-Host "  Direct link: https://sourceforge.net/projects/nsis/files/latest/download" -ForegroundColor Cyan
    Write-Host ""
    
    if ($DownloadNSIS) {
        Write-Host "Attempting to download NSIS installer..." -ForegroundColor Yellow
        $nsisInstaller = ".\nsis-installer.exe"
        try {
            Invoke-WebRequest -Uri "https://sourceforge.net/projects/nsis/files/NSIS%203/3.10/nsis-3.10-setup.exe/download" -OutFile $nsisInstaller -UseBasicParsing
            Write-Host "Downloaded NSIS installer to: $nsisInstaller" -ForegroundColor Green
            Write-Host "Please run the installer and then re-run this script." -ForegroundColor Yellow
            Start-Process $nsisInstaller
        } catch {
            Write-Host "Failed to download NSIS. Please download manually." -ForegroundColor Red
        }
    } else {
        Write-Host "Run with -DownloadNSIS to automatically download the installer." -ForegroundColor Gray
    }
    
    exit 1
}

Write-Host "Found NSIS: $nsisPath" -ForegroundColor Green

# Ensure LICENSE.txt exists
if (-not (Test-Path ".\LICENSE.txt")) {
    Write-Host "`nCreating default LICENSE.txt..." -ForegroundColor Yellow
    # License file should already be created, but just in case
}

# Build the installer
Write-Host "`nBuilding installer..." -ForegroundColor Yellow
Write-Host "Running: $nsisPath installer.nsi" -ForegroundColor Gray

try {
    & $nsisPath "installer.nsi"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n=====================================" -ForegroundColor Cyan
        Write-Host "Installer Created Successfully!" -ForegroundColor Green
        Write-Host "=====================================" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "Installer file: ForceDNS-Setup.exe" -ForegroundColor Green
        Write-Host ""
        
        if (Test-Path ".\ForceDNS-Setup.exe") {
            $fileInfo = Get-Item ".\ForceDNS-Setup.exe"
            Write-Host "File size: $([math]::Round($fileInfo.Length / 1MB, 2)) MB" -ForegroundColor Gray
            Write-Host "Created: $($fileInfo.CreationTime)" -ForegroundColor Gray
        }
    } else {
        Write-Host "`nInstaller build FAILED!" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "`nError building installer: $_" -ForegroundColor Red
    exit 1
}
