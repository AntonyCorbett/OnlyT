REM Run from dev command line
D:
cd \ProjectsPersonal\OnlyT
rd OnlyT\bin /q /s
rd Installer\Output /q /s

REM build

dotnet publish OnlyT\OnlyT.csproj -p:PublishProfile=FolderProfile -c:Release
dotnet publish OnlyTFirewallPorts\OnlyTFirewallPorts.csproj -p:PublishProfile=FolderProfile -c:Release

REM copy items into delivery
xcopy OnlyTFirewallPorts\bin\Release\net5.0\publish\*.* OnlyT\bin\Release\net5.0-windows\publish /q /s /y /d

REM Create installer
"C:\Program Files (x86)\Inno Setup 6\iscc" Installer\onlytsetup.iss

REM delete unwanted translations
rd OnlyT\bin\Release\publish\id-ID /q /s

REM create portable zip
powershell Compress-Archive -Path OnlyT\bin\Release\net5.0-windows\publish\* -DestinationPath Installer\Output\OnlyTPortable.zip 