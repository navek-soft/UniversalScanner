[Setup]
AppName=UniversalScanner
;GetFileVersion(UniversalScanner.exe)
AppVersion=0.1
VersionInfoVersion=0.1
VersionInfoCopyright=Julien Blitte
DefaultDirName={pf}\Universal Scanner
DefaultGroupName=Universal Scanner
UninstallDisplayIcon={app}\UniversalScanner.exe
Compression=lzma2
SolidCompression=yes
OutputBaseFilename=UniversalScanner Setup
OutputDir=.

[Files]
Source: "UniversalScanner.exe"; DestDir: "{app}"

[Icons]
Name: "{group}\Universal Scanner"; Filename: "{app}\UniversalScanner.exe"
Name: "{group}\Uninstall Universal Scanner"; Filename: "{uninstallexe}"
