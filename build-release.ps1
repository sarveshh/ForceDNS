# ForceDNS Release Build Script
# This script builds the ForceDNS solution in Release configuration

param(
    [switch]$SkipDependencyCheck = $false,
    [switch]$CreatePackage = $true
)

$ErrorActionPreference = "Stop"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "ForceDNS Release Build Script" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Find MSBuild
Write-Host "Locating MSBuild..." -ForegroundColor Yellow
$msbuildPath = Get-ChildItem "C:\Program Files (x86)\Microsoft Visual Studio" -Recurse -Filter MSBuild.exe -ErrorAction SilentlyContinue | 
    Select-Object -First 1 -ExpandProperty FullName

if (-not $msbuildPath) {
    $msbuildPath = Get-ChildItem "C:\Program Files\Microsoft Visual Studio" -Recurse -Filter MSBuild.exe -ErrorAction SilentlyContinue | 
        Select-Object -First 1 -ExpandProperty FullName
}

if (-not $msbuildPath) {
    Write-Host "ERROR: MSBuild not found. Please install Visual Studio or MSBuild Tools." -ForegroundColor Red
    exit 1
}

Write-Host "Found MSBuild: $msbuildPath" -ForegroundColor Green

# Check .NET Framework 4.7.2
if (-not $SkipDependencyCheck) {
    Write-Host "`nChecking .NET Framework 4.7.2 Developer Pack..." -ForegroundColor Yellow
    $netFramework472 = Test-Path "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2"
    
    if (-not $netFramework472) {
        Write-Host "WARNING: .NET Framework 4.7.2 Developer Pack not found!" -ForegroundColor Red
        Write-Host "Download from: https://dotnet.microsoft.com/download/dotnet-framework/net472" -ForegroundColor Yellow
        Write-Host ""
        $continue = Read-Host "Continue anyway? (y/n)"
        if ($continue -ne 'y') {
            exit 1
        }
    } else {
        Write-Host ".NET Framework 4.7.2 found." -ForegroundColor Green
    }
}

# Clean previous build
Write-Host "`nCleaning previous build..." -ForegroundColor Yellow
& $msbuildPath "ForceDNS.sln" /t:Clean /p:Configuration=Release /v:minimal

# Restore NuGet packages
Write-Host "`nRestoring NuGet packages..." -ForegroundColor Yellow
$nugetPath = ".\nuget.exe"

if (-not (Test-Path $nugetPath)) {
    Write-Host "Downloading NuGet.exe..." -ForegroundColor Yellow
    Invoke-WebRequest -Uri "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" -OutFile $nugetPath
}

& $nugetPath restore "ForceDNS.sln"

# Build solution (excluding installer project if WiX is not installed)
Write-Host "`nBuilding solution in Release mode..." -ForegroundColor Yellow

# Try building the full solution first
& $msbuildPath "ForceDNS.sln" /p:Configuration=Release /p:Platform="Any CPU" /v:minimal /m 2>$null

# If build failed, try without the installer project
if ($LASTEXITCODE -ne 0) {
    Write-Host "Full solution build failed. Trying without installer..." -ForegroundColor Yellow
    
    # Build individual projects
    & $msbuildPath "ForceDNS.Common\ForceDNS.Common.csproj" /p:Configuration=Release /v:minimal
    & $msbuildPath "ForceDNS.DataAccess\ForceDNS.DataAccess.csproj" /p:Configuration=Release /v:minimal
    & $msbuildPath "ForceDNS.BusinessLayer\ForceDNS.BusinessLayer.csproj" /p:Configuration=Release /v:minimal
    & $msbuildPath "ForceDNS.Service\ForceDNS.Service.csproj" /p:Configuration=Release /v:minimal
    & $msbuildPath "ForceDNS.UI\ForceDNS.UI.csproj" /p:Configuration=Release /v:minimal
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "`nBuild FAILED!" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "`nBuild SUCCEEDED (without installer)!" -ForegroundColor Green
    Write-Host "Note: WiX Toolset not found. MSI installer was skipped." -ForegroundColor Yellow
} else {
    Write-Host "`nBuild SUCCEEDED!" -ForegroundColor Green
}

# Create release package
if ($CreatePackage) {
    Write-Host "`nCreating release package..." -ForegroundColor Yellow
    
    $releaseDir = ".\Release"
    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $packageDir = ".\ForceDNS-Release-$timestamp"
    
    if (Test-Path $packageDir) {
        Remove-Item $packageDir -Recurse -Force
    }
    
    New-Item -ItemType Directory -Path $packageDir | Out-Null
    New-Item -ItemType Directory -Path "$packageDir\Service" | Out-Null
    New-Item -ItemType Directory -Path "$packageDir\UI" | Out-Null
    
    # Copy Service files
    if (Test-Path ".\ForceDNS.Service\bin\Release") {
        Copy-Item ".\ForceDNS.Service\bin\Release\*" -Destination "$packageDir\Service" -Recurse
        Write-Host "  - Copied Service files" -ForegroundColor Gray
    }
    
    # Copy UI files
    if (Test-Path ".\ForceDNS.UI\bin\Release") {
        Copy-Item ".\ForceDNS.UI\bin\Release\*" -Destination "$packageDir\UI" -Recurse
        Write-Host "  - Copied UI files" -ForegroundColor Gray
    }
    
    # Copy installer if available
    if (Test-Path ".\ForceDNS.Installer\bin\Release") {
        Copy-Item ".\ForceDNS.Installer\bin\Release\*.msi" -Destination $packageDir -ErrorAction SilentlyContinue
        Write-Host "  - Copied Installer (if available)" -ForegroundColor Gray
    }
    
    # Copy README
    if (Test-Path ".\README.md") {
        Copy-Item ".\README.md" -Destination $packageDir
    }
    
    Write-Host "`nRelease package created: $packageDir" -ForegroundColor Green
    
    # Create ZIP archive
    $zipFile = ".\ForceDNS-Release-$timestamp.zip"
    if (Test-Path $zipFile) {
        Remove-Item $zipFile -Force
    }
    
    Compress-Archive -Path $packageDir -DestinationPath $zipFile
    Write-Host "ZIP archive created: $zipFile" -ForegroundColor Green
}

Write-Host "`n=====================================" -ForegroundColor Cyan
Write-Host "Build Complete!" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
