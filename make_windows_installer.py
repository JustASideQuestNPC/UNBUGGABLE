'''
Generates and then runs a terrible nsis script to turn this thing.
'''

import subprocess
from pathlib import Path
from os import walk, path

APP_VERSION = '0.9.0'

ASSETS_DIR = 'UNBUGGABLE\\Assets'
PUBLISH_DIR = 'UNBUGGABLE\\publish'

NSIS_HEADER = r'''RequestExecutionLevel user ; for some reason the default level is admin?

!include "MUI2.nsh"
!include "UninstallLog.nsh"

InstallDir "$APPDATA\UNBUGGABLE"

Var StartMenuFolder

Name "UNBUGGABLE"
'''

MAIN_NSIS = r'''
; icons
Icon "UNBUGGABLE\InstallerAssets\icon.ico"
!define MUI_ICON "UNBUGGABLE\InstallerAssets\icon.ico"
!define MUI_UNICON "UNBUGGABLE\InstallerAssets\icon.ico"

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_DIRECTORY
!define MUI_STARTMENUPAGE_REGISTRY_ROOT "HKCU" 
!define MUI_STARTMENUPAGE_REGISTRY_KEY "UNBUGGABLE" 
!define MUI_STARTMENUPAGE_REGISTRY_VALUENAME "Start Menu Folder"
!insertmacro MUI_PAGE_STARTMENU Application $StartMenuFolder
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

; uninstaller ui pages
!insertmacro MUI_UNPAGE_WELCOME
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_UNPAGE_FINISH

!insertmacro MUI_LANGUAGE "English"
!define UninstLog "uninstall.log" ; log for only removing installed files
var UninstLog
; registry paths
!define REG_ROOT "HKCU"
!define REG_APP_PATH "Software\UNBUGGABLE"
; error string if you somehow deleted the log without deleting the rest of the app
LangString UninstLogMissing ${LANG_ENGLISH} \
           "${UninstLog} not found!$\r$\nUninstallation cannot proceed!"

LangString DESC_MainAppComponent ${LANG_ENGLISH} "Main UNBUGGABLE files (required)"
LangString DESC_DesktopShortcutComponent ${LANG_ENGLISH} "Add a shortcut to your desktop"
LangString DESC_FileAssociationComponent ${LANG_ENGLISH} \
           "Double-click on .beat.txt files to open them in UNBUGGABLE"

; define macros
!define AddItem "!insertmacro AddItem"
!define BackupFile "!insertmacro BackupFile"
!define BackupFiles "!insertmacro BackupFiles"
!define CopyFiles "!insertmacro CopyFiles"
!define CreateDirectory "!insertmacro CreateDirectory"
!define CreateShortcut "!insertmacro CreateShortcut"
!define File "!insertmacro File"
!define Rename "!insertmacro Rename"
!define RestoreFile "!insertmacro RestoreFile"
!define RestoreFiles "!insertmacro RestoreFiles"
!define SetOutPath "!insertmacro SetOutPath"
!define WriteRegDWORD "!insertmacro WriteRegDWORD"
!define WriteRegStr "!insertmacro WriteRegStr"
!define WriteUninstaller "!insertmacro WriteUninstaller"

Section -openlogfile
    CreateDirectory "$INSTDIR"
    IfFileExists "$INSTDIR\${UninstLog}" +3
        FileOpen $UninstLog "$INSTDIR\${UninstLog}" w
    Goto +4
        SetFileAttributes "$INSTDIR\${UninstLog}" NORMAL
        FileOpen $UninstLog "$INSTDIR\${UninstLog}" a
        FileSeek $UninstLog 0 END
SectionEnd

Section
    ${SetOutPath} $INSTDIR
SectionEnd

Section "Main App" MainAppComponent
    SectionIn RO ; makes this component required
    File "/oname=$OUTDIR\icon.ico" "UNBUGGABLE\InstallerAssets\icon.ico"
    File "/oname=$OUTDIR\config.json" "UNBUGGABLE\config.json"
    Call BruteForceInstallApp
    FileWrite $UninstLog "$OUTDIR\icon.ico$\r$\n"
    FileWrite $UninstLog "$OUTDIR\config.json$\r$\n"
    ${WriteUninstaller} "UNBUGGABLE_Uninstaller.exe"

    ; Add to the "Add or Remove Programs" list on windows
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\UNBUGGABLE" "DisplayName" \
                     "UNBUGGABLE: The chart editor where bugs are illegal and you...follow the law?"
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\UNBUGGABLE" "DisplayIcon" \
                     "$INSTDIR\UNBUGGABLE.exe"
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\UNBUGGABLE" \
                     "UninstallString" "$\"$INSTDIR\UNBUGGABLE_Uninstaller.exe$\""
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\UNBUGGABLE" \
                     "QuietUninstallString" "$\"$INSTDIR\UNBUGGABLE_Uninstaller.exe$\" /S"
SectionEnd

; Section "Associate with .beat.txt Files" FileAssociationComponent
    ; WriteRegStr HKCU "Software\Classes\.beat.txt" "" "UNBUGGABLE.ChartFile"
    ; WriteRegStr HKCU "Software\Classes\UNBUGGABLE.ChartFile" "" "UNBUGGABLE Chart File"
    ; WriteRegStr HKCU "Software\Classes\UNBUGGABLE.ChartFile\shell\open\command" "" \
                     ; "$\"$INSTDIR\UNBUGGABLE.exe$\" $\"%1$\""
    ; WriteRegStr HKCU "Software\Classes\UNBUGGABLE.ChartFile\DefaultIcon" ""\
                     ; "$INSTDIR\UNBUGGABLE.exe"
; SectionEnd

Section "Desktop Shortcut" DesktopShortcutComponent
    ${CreateShortcut} "$Desktop\UNBUGGABLE.lnk" "$INSTDIR\UNBUGGABLE.exe" "" "$INSTDIR\icon.ico" ""
SectionEnd

Section
    !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
    ; Create shortcuts
    WriteRegStr HKCU "Software\UNBUGGABLE" "" $INSTDIR
    ${CreateDirectory} "$SMPROGRAMS\$StartMenuFolder"
    ${CreateShortcut} "$SMPROGRAMS\$StartMenuFolder\UNBUGGABLE.lnk" "$INSTDIR\UNBUGGABLE.exe" "" \
                      "$INSTDIR\icon.ico" ""
    !insertmacro MUI_STARTMENU_WRITE_END
SectionEnd

Section Uninstall
    ; Can't uninstall if uninstall log is missing!
    IfFileExists "$INSTDIR\${UninstLog}" +3
        MessageBox MB_OK|MB_ICONSTOP "$(UninstLogMissing)"
        Abort

    DeleteRegKey HKCU "UNBUGGABLE\"
    DeleteRegKey HKCU "Software\UNBUGGABLE\"
    ; DeleteRegKey HKCU "Software\Classes\.beat.txt"
    ; DeleteRegKey HKCU "Software\Classes\UNBUGGABLE.ChartFile"
    DeleteRegKey HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\UNBUGGABLE"
    
    Push $R0
    Push $R1
    Push $R2
    SetFileAttributes "$INSTDIR\${UninstLog}" NORMAL
    FileOpen $UninstLog "$INSTDIR\${UninstLog}" r
    StrCpy $R1 -1
    
    GetLineCount:
        ClearErrors
        FileRead $UninstLog $R0
        IntOp $R1 $R1 + 1
        StrCpy $R0 $R0 -2
        Push $R0   
        IfErrors 0 GetLineCount
    Pop $R0
    
    LoopRead:
        StrCmp $R1 0 LoopDone
        Pop $R0
    
        IfFileExists "$R0\*.*" 0 +3
        RMDir $R0  #is dir
        Goto +9
        IfFileExists $R0 0 +3
        Delete $R0 #is file
        Goto +6
        StrCmp $R0 "${REG_ROOT} ${REG_APP_PATH}" 0 +3
        DeleteRegKey ${REG_ROOT} "${REG_APP_PATH}" ; is Reg Element
        Goto +3
        StrCmp $R0 "${REG_ROOT} ${UNINSTALL_PATH}" 0 +2
        DeleteRegKey ${REG_ROOT} "${UNINSTALL_PATH}" ; is Reg Element
    
        IntOp $R1 $R1 - 1
        Goto LoopRead
    LoopDone:

    FileClose $UninstLog
    Delete "$INSTDIR\${UninstLog}"
    Delete "$INSTDIR\userData.json"
    ; on the one hand, this will delete EVERYTHING in the logs folder even if i didn't install it.
    ; on the other hand, if you were stupid enough to put important files in the logs folder then
    ; tbh you kinda deserve this.
    RMDir /r "$INSTDIR\logs"
    Pop $R2
    Pop $R1
    Pop $R0
SectionEnd

'''

def relative_path(path: str) -> Path:
    return Path.cwd() / path

full_publish_path = relative_path(PUBLISH_DIR)
full_assets_path = relative_path(ASSETS_DIR)

# look through the assembly to find the version
version = ''
with open(relative_path('UNBUGGABLE/UNBUGGABLE.csproj')) as file:
    for line in file.readlines():
        if '<AssemblyVersion>' in line:
            version_start = line.find('<AssemblyVersion>') + len('<AssemblyVersion>')
            version_end = line.find('</AssemblyVersion>')
            version = line[version_start:version_end]
            break

BASE_NSIS = NSIS_HEADER + f'OutFile "UNBUGGABLE_v{version}_Setup.exe"\n' + MAIN_NSIS

print('starting build...\npublishing .net builds...')

subprocess.run((
    f'dotnet publish UNBUGGABLE/UNBUGGABLE.csproj -c Release -r win-x64 --self-contained true ' +
        '-o UNBUGGABLE/publish'
), shell=True)

print('generating nsis script...\ngenerating file list...')
nsis_lines = [BASE_NSIS]
nsis_lines.append('Function BruteForceInstallApp\n')
nsis_lines.append('    ; app files\n')

def recursive_write_nsis(full_path, level=0):
    dirname = f'{full_path}'[len(f'{full_publish_path}') + 1:]
    print(f'writing from {dirname}')
    nsis_lines.append(f'\n    ; {dirname}\n')
    if level > 0:
        nsis_lines.append(f'    CreateDirectory "$INSTDIR\\{dirname}"\n')

    for file in full_path.iterdir():
        if file.is_dir():
            recursive_write_nsis(file, level + 1)
        else:
            filename = f'{file}'[len(f'{full_publish_path}') + 1:]
            nsis_lines.append(f'    File "/oname={filename}" "{PUBLISH_DIR}\\{filename}"\n')
            nsis_lines.append(f'    FileWrite $UninstLog "$OUTDIR\\{filename}$\\r$\\n"\n')

    nsis_lines.append('\n')

recursive_write_nsis(full_publish_path)

nsis_lines.append('\n    CreateDirectory "$INSTDIR\\Assets"\n')
for file in full_assets_path.iterdir():
    if file.is_file():
        filename = f'{file}'[len(f'{full_assets_path}') + 1:]
        nsis_lines.append(f'    File "/oname=Assets\\{filename}" "{ASSETS_DIR}\\{filename}"\n')
        nsis_lines.append(f'    FileWrite $UninstLog "$OUTDIR\\Assets\\{filename}$\\r$\\n"\n')

nsis_lines.pop()
nsis_lines.append('FunctionEnd\n')

print('writing nsis script...')
with open(relative_path('WindowsInstaller.nsi'), 'w') as file:
    file.writelines(nsis_lines)

print('running nsis script...')   
subprocess.run(('makensis WindowsInstaller.nsi'), shell=True)