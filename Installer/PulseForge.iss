#define MyAppName "PulseForge Studio"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "PulseForge"
#define MyAppExeName "PulseForgeStudio.exe"

[Setup]
AppId={{9FC19ADC-4F0F-4EA1-9E7B-9F52B23A6721}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\PulseForge Studio
DefaultGroupName=PulseForge Studio
OutputDir=..\dist
OutputBaseFilename=PulseForgeStudio-Setup-x64
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\{#MyAppExeName}

[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\PulseForge Studio"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\PulseForge Studio"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional icons:"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch PulseForge Studio"; Flags: nowait postinstall skipifsilent
