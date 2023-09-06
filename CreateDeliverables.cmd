REM Run from dev command line

@ECHO OFF

VERIFY ON

D:
cd \ProjectsPersonal\OnlyT
rd OnlyT\bin /q /s
rd OnlyTFirewallPorts\bin /q /s
rd Installer\Output /q /s
rd Installer\Staging /q /s

ECHO.
ECHO Publishing OnlyT
dotnet publish OnlyT\OnlyT.csproj -p:PublishProfile=FolderProfile -c:Release
IF %ERRORLEVEL% NEQ 0 goto ERROR

ECHO.
ECHO Publishing OnlyTFirewallPorts
dotnet publish OnlyTFirewallPorts\OnlyTFirewallPorts.csproj -p:PublishProfile=FolderProfile -c:Release
IF %ERRORLEVEL% NEQ 0 goto ERROR

md Installer\Staging

ECHO.
ECHO Copying OnlyTFirewallPorts items into staging area
xcopy OnlyTFirewallPorts\bin\Release\net7.0\publish\*.* Installer\Staging /q /s /y /d
IF %ERRORLEVEL% NEQ 0 goto ERROR

ECHO.
ECHO Copying OnlyT items into staging area
xcopy OnlyT\bin\Release\net7.0-windows\publish\*.* Installer\Staging /q /s /y /d
IF %ERRORLEVEL% NEQ 0 goto ERROR

ECHO.
ECHO Copying Sample task_schedule file into staging area
xcopy talk_schedule.xml Installer\Staging /q /y
IF %ERRORLEVEL% NEQ 0 goto ERROR

ECHO.
ECHO Removing unwanted x32 DLLs
del Installer\Staging\libmp3lame.32.dll
IF %ERRORLEVEL% NEQ 0 goto ERROR

ECHO.
ECHO Removing unwanted language files
rd Installer\Staging\no-NO /q /s
rd Installer\Staging\pap-PAP /q /s

ECHO.
ECHO Copying Satellite assemblies for language files
ECHO Czech
xcopy Installer\Staging\cs\*.*  Installer\Staging\cs-CZ /q
rd Installer\Staging\cs /q /s
ECHO German
xcopy Installer\Staging\de\*.*  Installer\Staging\de-DE /q
rd Installer\Staging\de /q /s
ECHO French
xcopy Installer\Staging\fr\*.*  Installer\Staging\fr-FR /q
rd Installer\Staging\fr /q /s
ECHO Italian
xcopy Installer\Staging\it\*.*  Installer\Staging\it-IT /q
rd Installer\Staging\it /q /s
ECHO Polish
xcopy Installer\Staging\pl\*.*  Installer\Staging\pl-PL /q
rd Installer\Staging\pl /q /s
ECHO Russian
xcopy Installer\Staging\ru\*.*  Installer\Staging\ru-RU /q
rd Installer\Staging\ru /q /s
ECHO Spanish
xcopy Installer\Staging\es\*.*  Installer\Staging\es-ES /q
xcopy Installer\Staging\es\*.*  Installer\Staging\es-MX /q
rd Installer\Staging\es /q /s
ECHO Turkish
xcopy Installer\Staging\tr\*.*  Installer\Staging\tr-TR /q
rd Installer\Staging\tr /q /s
ECHO Korean
xcopy Installer\Staging\ko\*.*  Installer\Staging\ko-KR /q
rd Installer\Staging\ko /q /s


ECHO.
ECHO Creating installer
"D:\Program Files (x86)\Inno Setup 6\iscc" Installer\onlytsetup.iss
IF %ERRORLEVEL% NEQ 0 goto ERROR

ECHO.
ECHO Creating portable zip
powershell Compress-Archive -Path Installer\Staging\* -DestinationPath Installer\Output\OnlyTPortable.zip
IF %ERRORLEVEL% NEQ 0 goto ERROR

goto SUCCESS

:ERROR
ECHO.
ECHO ******************
ECHO An ERROR occurred!
ECHO ******************

:SUCCESS

PAUSE