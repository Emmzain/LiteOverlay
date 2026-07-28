# Build script for native LiteOverlay binaries
$ErrorActionPreference = "Stop"

$rootDir = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $rootDir

$overlayCs = Join-Path $rootDir "overlay\LiteOverlay.cs"
$setupCs   = Join-Path $rootDir "overlay\SetupLiteOverlay.cs"
$iconPath  = Join-Path $rootDir "assets\app.ico"
$binDir    = Join-Path $rootDir "overlay\bin"

if (-not (Test-Path $binDir)) {
    New-Item -ItemType Directory -Force -Path $binDir | Out-Null
}

if (-not (Test-Path $overlayCs)) {
    throw "Missing overlay\LiteOverlay.cs"
}

Get-Process -Name "LiteOverlay" -ErrorAction SilentlyContinue | Stop-Process -Force
Get-Process -Name "SetupLiteOverlay" -ErrorAction SilentlyContinue | Stop-Process -Force

$csc = Join-Path $env:WINDIR "Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if (-not (Test-Path $csc)) {
    $csc = Join-Path $env:WINDIR "Microsoft.NET\Framework\v4.0.30319\csc.exe"
}
if (-not (Test-Path $csc)) {
    throw "C# compiler not found. Install .NET Framework 4.x Developer Pack."
}

# Compile LiteOverlay.exe
$outExe = Join-Path $rootDir "LiteOverlay.exe"
& $csc /nologo /target:winexe /win32icon:$iconPath /out:$outExe `
    /reference:System.Windows.Forms.dll `
    /reference:System.Drawing.dll `
    /reference:System.Management.dll `
    $overlayCs

if ($LASTEXITCODE -ne 0) {
    throw "LiteOverlay.exe build failed."
}

Copy-Item $outExe (Join-Path $binDir "LiteOverlay.exe") -Force

# Compile SetupLiteOverlay.exe
$outSetup = Join-Path $rootDir "SetupLiteOverlay.exe"
& $csc /nologo /target:winexe /win32icon:$iconPath /out:$outSetup `
    /reference:System.Windows.Forms.dll `
    /reference:System.Drawing.dll `
    $setupCs

if ($LASTEXITCODE -ne 0) {
    throw "SetupLiteOverlay.exe build failed."
}

Copy-Item $outSetup (Join-Path $binDir "SetupLiteOverlay.exe") -Force

Write-Host "⚡ Built native LiteOverlay.exe & SetupLiteOverlay.exe successfully into root and overlay/bin/!" -ForegroundColor Green
