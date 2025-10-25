# ForceDNS - Release Build Instructions

## Prerequisites

Before building a Windows release, ensure you have the following installed:

1. **Visual Studio 2019 or later** (or MSBuild Tools)

   - Download from: https://visualstudio.microsoft.com/downloads/

2. **.NET Framework 4.7.2 Developer Pack**

   - Download from: https://dotnet.microsoft.com/download/dotnet-framework/net472
   - Or use the .NET Framework 4.7.2 Targeting Pack

3. **WiX Toolset** (for building the installer)
   - Download from: https://wixtoolset.org/releases/
   - Required only if you want to build the MSI installer

## Quick Build

### Option 1: Using PowerShell Script (Recommended)

1. Open PowerShell in the project root directory
2. Run: `.\build-release.ps1`

### Option 2: Using Batch File

1. Double-click `build-release.cmd` in Windows Explorer
2. Wait for the build to complete

### Option 3: Manual Build

1. Open PowerShell or Command Prompt
2. Run the following command:

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe" ForceDNS.sln /p:Configuration=Release /p:Platform="Any CPU"
```

Or if using Visual Studio 2022:

```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" ForceDNS.sln /p:Configuration=Release /p:Platform="Any CPU"
```

## Build Output

After a successful build, you'll find:

- **Service**: `ForceDNS.Service\bin\Release\`
- **UI Application**: `ForceDNS.UI\bin\Release\`
- **Installer** (if WiX is installed): `ForceDNS.Installer\bin\Release\`

## Creating a Distribution Package

The build script automatically creates a timestamped release package with:

- Service executable and dependencies
- UI application and dependencies
- Installer MSI (if available)
- README documentation

The package is created as both a folder and a ZIP file:

- `ForceDNS-Release-YYYYMMDD-HHMMSS\`
- `ForceDNS-Release-YYYYMMDD-HHMMSS.zip`

## Troubleshooting

### MSBuild not found

Install Visual Studio 2019 or later, or download MSBuild Tools:
https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio-2019

### .NET Framework 4.7.2 error

Download and install the .NET Framework 4.7.2 Developer Pack:
https://dotnet.microsoft.com/download/dotnet-framework/net472

### WiX Toolset errors

If you don't need the MSI installer:

1. Remove the ForceDNS.Installer project from the solution, or
2. Install WiX Toolset: https://wixtoolset.org/releases/

### NuGet restore errors

Run: `nuget.exe restore ForceDNS.sln`

If nuget.exe is not available, download it from:
https://dist.nuget.org/win-x86-commandline/latest/nuget.exe

## Installation

1. **Using the MSI Installer** (if built):

   - Run the `.msi` file from the installer output directory
   - Follow the installation wizard

2. **Manual Installation**:
   - Copy files from `ForceDNS.Service\bin\Release\` to your desired location
   - Copy files from `ForceDNS.UI\bin\Release\` to your desired location
   - Install the service using InstallUtil or the service installer

## Service Installation

To install the Windows Service manually:

```cmd
sc create ForceDNS binPath= "C:\Path\To\ForceDNS.Service.exe"
sc start ForceDNS
```

Or use InstallUtil:

```cmd
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe "C:\Path\To\ForceDNS.Service.exe"
```

## Notes

- The solution includes 5 projects: Service, UI, Common, DataAccess, BusinessLayer, and Installer
- The installer project has dependencies on all other projects
- Build in Release configuration for production deployments
- Build in Debug configuration for development and testing
