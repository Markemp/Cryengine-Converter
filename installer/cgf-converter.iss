; Inno Setup script for Cryengine Converter
;
; Build instructions:
;   1. Publish the executable first:
;        dotnet publish cgf-converter\cgf-converter.csproj -c Release
;   2. From this directory, compile the installer:
;        iscc cgf-converter.iss
;   3. Output goes to .\output\cgf-converter-setup-<version>.exe
;
; Inno Setup 6.2.0 or newer required (https://jrsoftware.org/isinfo.php).

#define MyAppName "Cryengine Converter"
#define MyAppVersion "2.0.0"
#define MyAppPublisher "Heffay Presents"
#define MyAppURL "https://github.com/Markemp/Cryengine-Converter"
#define MyAppExeName "cgf-converter.exe"

; Path to the published binary, relative to this script.
#define PublishDir "..\cgf-converter\bin\Release\net9.0\win-x64\publish"

[Setup]
; AppId uniquely identifies this application.
; Do NOT change this between versions or upgrades will create duplicate entries.
AppId={{267AC84D-FD30-4860-A971-393139539822}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/issues
AppUpdatesURL={#MyAppURL}/releases
VersionInfoVersion={#MyAppVersion}.0

; Install destination. {autopf} resolves to:
;   - %ProgramFiles%\Cryengine Converter   when installing for all users (admin)
;   - %LocalAppData%\Programs\Cryengine Converter   when installing per-user
DefaultDirName={autopf}\{#MyAppName}
UsePreviousAppDir=yes

; Uninstaller registers under HKCU or HKLM based on install mode.
UninstallDisplayName={#MyAppName} {#MyAppVersion}
UninstallDisplayIcon={app}\{#MyAppExeName}

; Per-user install by default (no UAC prompt). User can choose all-users at startup.
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

; No Start Menu folder — this is a CLI tool.
DisableProgramGroupPage=yes
DisableDirPage=auto
DisableReadyPage=no

; Compression
Compression=lzma2/ultra64
SolidCompression=yes
LZMAUseSeparateProcess=yes

; UI
WizardStyle=modern
WizardSizePercent=120

; Output
OutputDir=output
OutputBaseFilename=cgf-converter-setup-{#MyAppVersion}

; License
LicenseFile=..\LICENSE

; Allow the installer to broadcast WM_SETTINGCHANGE so newly opened terminals
; pick up our PATH edit without requiring a logoff.
ChangesEnvironment=yes

; Architecture
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "addtopath"; Description: "Add {#MyAppName} to PATH (recommended — required to run cgf-converter from any directory)"; GroupDescription: "Additional tasks:"; Flags: checkedonce

[Files]
Source: "{#PublishDir}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\LICENSE"; DestDir: "{app}"; DestName: "LICENSE.txt"; Flags: ignoreversion
Source: "README.txt"; DestDir: "{app}"; Flags: ignoreversion

[Run]
Filename: "{app}\README.txt"; Description: "View installation README"; Flags: postinstall shellexec skipifsilent unchecked

[Code]
const
  EnvironmentKey_HKCU = 'Environment';
  EnvironmentKey_HKLM = 'System\CurrentControlSet\Control\Session Manager\Environment';

procedure GetEnvKeyContext(var RootKey: Integer; var EnvKey: string);
begin
  if IsAdminInstallMode then begin
    RootKey := HKEY_LOCAL_MACHINE;
    EnvKey := EnvironmentKey_HKLM;
  end else begin
    RootKey := HKEY_CURRENT_USER;
    EnvKey := EnvironmentKey_HKCU;
  end;
end;

function PathContainsDir(const PathValue, AppDir: string): Boolean;
begin
  // Sentinel-wrap with semicolons so we match whole entries only,
  // not e.g. 'C:\Foo' as a substring of 'C:\FooBar'. Case-insensitive.
  Result := Pos(
    ';' + LowerCase(AppDir) + ';',
    ';' + LowerCase(PathValue) + ';'
  ) > 0;
end;

procedure AddDirToPath(const AppDir: string);
var
  RootKey: Integer;
  EnvKey, OrigPath, NewPath: string;
begin
  GetEnvKeyContext(RootKey, EnvKey);

  if not RegQueryStringValue(RootKey, EnvKey, 'Path', OrigPath) then
    OrigPath := '';

  if PathContainsDir(OrigPath, AppDir) then
    Exit;

  if (Length(OrigPath) = 0) then
    NewPath := AppDir
  else if (OrigPath[Length(OrigPath)] = ';') then
    NewPath := OrigPath + AppDir
  else
    NewPath := OrigPath + ';' + AppDir;

  RegWriteExpandStringValue(RootKey, EnvKey, 'Path', NewPath);
end;

procedure RemoveDirFromPath(const AppDir: string);
var
  RootKey: Integer;
  EnvKey, OrigPath, Wrapped, LowerWrapped, LowerEntry: string;
  P, EntryLen: Integer;
begin
  GetEnvKeyContext(RootKey, EnvKey);

  if not RegQueryStringValue(RootKey, EnvKey, 'Path', OrigPath) then
    Exit;

  Wrapped := ';' + OrigPath + ';';
  LowerWrapped := LowerCase(Wrapped);
  LowerEntry := ';' + LowerCase(AppDir) + ';';

  P := Pos(LowerEntry, LowerWrapped);
  if P = 0 then
    Exit;

  EntryLen := Length(AppDir) + 1;  { drop our entry plus its trailing semicolon }
  Delete(Wrapped, P, EntryLen);

  { Strip the leading and trailing semicolons we added for matching. }
  if (Length(Wrapped) > 0) and (Wrapped[1] = ';') then
    Delete(Wrapped, 1, 1);
  if (Length(Wrapped) > 0) and (Wrapped[Length(Wrapped)] = ';') then
    Delete(Wrapped, Length(Wrapped), 1);

  RegWriteExpandStringValue(RootKey, EnvKey, 'Path', Wrapped);
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
    if WizardIsTaskSelected('addtopath') then
      AddDirToPath(ExpandConstant('{app}'));
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then
    RemoveDirFromPath(ExpandConstant('{app}'));
end;
