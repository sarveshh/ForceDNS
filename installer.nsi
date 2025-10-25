; ForceDNS Installer Script
; NSIS Modern User Interface

;--------------------------------
; Includes

!include "MUI2.nsh"
!include "x64.nsh"

;--------------------------------
; General

; Name and file
Name "ForceDNS"
OutFile "ForceDNS-Setup.exe"
Unicode True

; Default installation folder
InstallDir "$PROGRAMFILES\ForceDNS"

; Get installation folder from registry if available
InstallDirRegKey HKLM "Software\ForceDNS" ""

; Request application privileges
RequestExecutionLevel admin

;--------------------------------
; Variables

Var StartMenuFolder

;--------------------------------
; Interface Settings

!define MUI_ABORTWARNING
!define MUI_ICON "${NSISDIR}\Contrib\Graphics\Icons\modern-install.ico"
!define MUI_UNICON "${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"

;--------------------------------
; Pages

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "LICENSE.txt"
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_DIRECTORY

; Start Menu Folder Page Configuration
!define MUI_STARTMENUPAGE_REGISTRY_ROOT "HKLM" 
!define MUI_STARTMENUPAGE_REGISTRY_KEY "Software\ForceDNS" 
!define MUI_STARTMENUPAGE_REGISTRY_VALUENAME "Start Menu Folder"

!insertmacro MUI_PAGE_STARTMENU Application $StartMenuFolder

!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

;--------------------------------
; Languages

!insertmacro MUI_LANGUAGE "English"

;--------------------------------
; Version Information

VIProductVersion "1.0.0.0"
VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductName" "ForceDNS"
VIAddVersionKey /LANG=${LANG_ENGLISH} "CompanyName" "ForceDNS"
VIAddVersionKey /LANG=${LANG_ENGLISH} "LegalCopyright" "Copyright 2025"
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileDescription" "ForceDNS Installer"
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileVersion" "1.0.0.0"

;--------------------------------
; Installer Sections

Section "ForceDNS Service" SecService

  SectionIn RO ; Read-only (always installed)
  
  SetOutPath "$INSTDIR\Service"
  
  ; Copy service files
  File /r "ForceDNS.Service\bin\Release\*.*"
  
  ; Store installation folder
  WriteRegStr HKLM "Software\ForceDNS" "" $INSTDIR
  
  ; Create uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"
  
  ; Add uninstall information to Add/Remove Programs
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\ForceDNS" \
                   "DisplayName" "ForceDNS"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\ForceDNS" \
                   "UninstallString" "$\"$INSTDIR\Uninstall.exe$\""
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\ForceDNS" \
                   "QuietUninstallString" "$\"$INSTDIR\Uninstall.exe$\" /S"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\ForceDNS" \
                   "DisplayIcon" "$INSTDIR\Service\ForceDNS.Service.exe"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\ForceDNS" \
                   "Publisher" "ForceDNS"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\ForceDNS" \
                   "DisplayVersion" "1.0.0"
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\ForceDNS" \
                   "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\ForceDNS" \
                   "NoRepair" 1
  
  ; Stop service if running
  DetailPrint "Stopping ForceDNS service if running..."
  nsExec::ExecToLog 'sc stop ForceDNS'
  Sleep 2000
  
  ; Install the service
  DetailPrint "Installing ForceDNS service..."
  nsExec::ExecToLog '"$INSTDIR\Service\ForceDNS.Service.exe" /install'
  
  ; Alternative method using sc command
  nsExec::ExecToLog 'sc create ForceDNS binPath= "$INSTDIR\Service\ForceDNS.Service.exe" start= auto DisplayName= "ForceDNS Service"'
  nsExec::ExecToLog 'sc description ForceDNS "Forces DNS settings to prevent DNS hijacking"'
  
  ; Start the service
  DetailPrint "Starting ForceDNS service..."
  nsExec::ExecToLog 'sc start ForceDNS'

SectionEnd

Section "ForceDNS UI" SecUI

  SetOutPath "$INSTDIR\UI"
  
  ; Copy UI files
  File /r "ForceDNS.UI\bin\Release\*.*"
  
  ; Create shortcuts
  !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
    
    CreateDirectory "$SMPROGRAMS\$StartMenuFolder"
    CreateShortcut "$SMPROGRAMS\$StartMenuFolder\ForceDNS.lnk" "$INSTDIR\UI\ForceDNS.UI.exe"
    CreateShortcut "$SMPROGRAMS\$StartMenuFolder\Uninstall.lnk" "$INSTDIR\Uninstall.exe"
    
  !insertmacro MUI_STARTMENU_WRITE_END
  
  ; Create desktop shortcut
  CreateShortcut "$DESKTOP\ForceDNS.lnk" "$INSTDIR\UI\ForceDNS.UI.exe"

SectionEnd

Section "Start Menu Shortcuts" SecStartMenu

  ; Shortcuts are created in SecUI section
  
SectionEnd

;--------------------------------
; Descriptions

!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${SecService} "Installs the ForceDNS Windows Service (Required)"
  !insertmacro MUI_DESCRIPTION_TEXT ${SecUI} "Installs the ForceDNS configuration interface"
  !insertmacro MUI_DESCRIPTION_TEXT ${SecStartMenu} "Creates Start Menu shortcuts"
!insertmacro MUI_FUNCTION_DESCRIPTION_END

;--------------------------------
; Uninstaller Section

Section "Uninstall"

  ; Stop and remove the service
  DetailPrint "Stopping ForceDNS service..."
  nsExec::ExecToLog 'sc stop ForceDNS'
  Sleep 2000
  
  DetailPrint "Removing ForceDNS service..."
  nsExec::ExecToLog '"$INSTDIR\Service\ForceDNS.Service.exe" /uninstall'
  nsExec::ExecToLog 'sc delete ForceDNS'
  Sleep 1000
  
  ; Remove files and folders
  RMDir /r "$INSTDIR\Service"
  RMDir /r "$INSTDIR\UI"
  Delete "$INSTDIR\Uninstall.exe"
  RMDir "$INSTDIR"
  
  ; Remove shortcuts
  !insertmacro MUI_STARTMENU_GETFOLDER Application $StartMenuFolder
  
  Delete "$SMPROGRAMS\$StartMenuFolder\ForceDNS.lnk"
  Delete "$SMPROGRAMS\$StartMenuFolder\Uninstall.lnk"
  RMDir "$SMPROGRAMS\$StartMenuFolder"
  
  Delete "$DESKTOP\ForceDNS.lnk"
  
  ; Remove registry keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\ForceDNS"
  DeleteRegKey HKLM "Software\ForceDNS"

SectionEnd
