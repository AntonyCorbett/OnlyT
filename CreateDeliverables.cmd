REM Run from dev command line

@ECHO OFF

VERIFY ON

D:
cd \ProjectsPersonal\OnlyT
rd OnlyT\bin /q /s
rd Installer\Output /q /s

REM build

ECHO.
ECHO Publishing OnlyT
dotnet publish OnlyT\OnlyT.csproj -p:PublishProfile=FolderProfile -c:Release
IF %ERRORLEVEL% NEQ 0 goto ERROR

ECHO.
ECHO Publishing OnlyTFirewallPorts
dotnet publish OnlyTFirewallPorts\OnlyTFirewallPorts.csproj -p:PublishProfile=FolderProfile -c:Release
IF %ERRORLEVEL% NEQ 0 goto ERROR

ECHO.
ECHO Copying items into delivery
xcopy OnlyTFirewallPorts\bin\Release\net5.0\publish\*.* OnlyT\bin\Release\net5.0-windows\publish /q /s /y /d
IF %ERRORLEVEL% NEQ 0 goto ERROR

ECHO.
ECHO Removing unwanted x64 DLLs
del OnlyT\bin\Release\net5.0-windows\publish\libmp3lame.64.dll
IF %ERRORLEVEL% NEQ 0 goto ERROR

ECHO.
ECHO Removing unwanted translations
rd OnlyT\bin\Release\net5.0-windows\publish\id-ID /q /s
IF %ERRORLEVEL% NEQ 0 goto ERROR

ECHO.
ECHO Create installer
"D:\Program Files (x86)\Inno Setup 6\iscc" Installer\onlytsetup.iss
IF %ERRORLEVEL% NEQ 0 goto ERROR

ECHO.
ECHO Create portable zip
powershell Compress-Archive -Path OnlyT\bin\Release\net5.0-windows\publish\* -DestinationPath Installer\Output\OnlyTPortable.zip 
IF %ERRORLEVEL% NEQ 0 goto ERROR

goto SUCCESS

:ERROR
ECHO.
ECHO ******************
ECHO An ERROR occurred!
ECHO ******************

:SUCCESS

PAUSE