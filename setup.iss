; Inno Setup script for Bill Payment Manager
[Setup]
AppName=Bill Payment Manager
AppVersion=1.0.0
DefaultDirName={autopf}\BillPaymentManager
DisableProgramGroupPage=yes
OutputBaseFilename=BillPaymentManager-Installer
SetupIconFile=icon.ico
UninstallDisplayIcon={app}\BillPaymentManager.exe
Compression=lzma2
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64

[Files]
; The publish folder. Ensure you run the "dotnet publish" command before compiling this script.
; Command: dotnet publish src/BillPaymentManager/BillPaymentManager.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "icon.ico"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\Bill Payment Manager"; Filename: "{app}\BillPaymentManager.exe"; IconFilename: "{app}\icon.ico"
Name: "{autoprograms}\Uninstall Bill Payment Manager"; Filename: "{uninstallexe}"
Name: "{autodesktop}\Bill Payment Manager"; Filename: "{app}\BillPaymentManager.exe"; Tasks: desktopicon; IconFilename: "{app}\icon.ico"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Run]
Filename: "{app}\BillPaymentManager.exe"; Description: "Launch Bill Payment Manager"; Flags: nowait postinstall skipifsilent
