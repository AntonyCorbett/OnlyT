#define MyAppName "OnlyT"
#define MyAppPublisher "Antony Corbett"
#define MyAppURL "https://github.com/AntonyCorbett/OnlyT"
#define MyAppExeName "OnlyT.exe"

#define MyAppVersion GetFileVersion('Staging\OnlyT.exe');

[Setup]
AppId={{42BA2BBE-E9BB-4F67-9307-7F98FB73C6FF}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={commonpf}\OnlyT
DefaultGroupName={#MyAppName}
OutputDir="Output"
OutputBaseFilename=OnlyTSetup
SetupIconFile=..\OnlyT\icon4.ico
Compression=lzma
SolidCompression=yes
AppContact=antony@corbetts.org.uk
DisableWelcomePage=false
SetupLogging=True
RestartApplications=False
CloseApplications=False
AppMutex=OnlyTMeetingTimer

PrivilegesRequired=admin

[InstallDelete]
Type: filesandordirs; Name: "{app}\*.*"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "Staging\*"; DestDir: "{app}"; Flags: ignoreversion; Excludes: "*.pdb"

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[ThirdParty]
UseRelativePaths=True
