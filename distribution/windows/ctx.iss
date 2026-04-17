; CTX Windows installer scaffold
; Requires Inno Setup to build a Git-for-Windows-style EXE installer.

#define AppName "CTX"
#define AppVersion "1.0.7"
#define AppPublisher "CTX Project"
#define AppExeName "ctx.cmd"

[Setup]
AppId={{6E2F1A98-3F9C-4EDB-8A2D-CTXDIST000001}}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={autopf}\CTX
DefaultGroupName=CTX
OutputDir=..\..\artifacts\distribution\installer\windows
OutputBaseFilename=ctx-setup-win-x64
Compression=lzma
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64

[Files]
Source: "..\..\artifacts\distribution\win-x64\bundle\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

[Icons]
Name: "{group}\CTX"; Filename: "{app}\bin\{#AppExeName}"
Name: "{group}\CTX Viewer"; Filename: "{app}\bin\ctx-viewer.cmd"

[Tasks]
Name: "addtopath"; Description: "Add CTX to PATH"; Flags: unchecked

[Registry]
Root: HKCU; Subkey: "Environment"; ValueType: expandsz; ValueName: "Path"; ValueData: "{olddata};{app}\bin"; Tasks: addtopath; Check: NeedsAddPath(ExpandConstant('{app}\bin'))

[Code]
function NeedsAddPath(Path: string): Boolean;
var
  Paths: string;
begin
  if not RegQueryStringValue(HKCU, 'Environment', 'Path', Paths) then
  begin
    Result := True;
    exit;
  end;

  Result := Pos(Lowercase(Path), Lowercase(Paths)) = 0;
end;
