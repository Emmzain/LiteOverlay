# Build native LiteOverlay.exe (Pure C# WinForms - no browser)
$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

if (-not (Test-Path "LiteOverlay.cs")) {
    throw "Missing LiteOverlay.cs"
}

Get-Process -Name "LiteOverlay" -ErrorAction SilentlyContinue | Stop-Process -Force

$csc = Join-Path $env:WINDIR "Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if (-not (Test-Path $csc)) {
    $csc = Join-Path $env:WINDIR "Microsoft.NET\Framework\v4.0.30319\csc.exe"
}
if (-not (Test-Path $csc)) {
    throw "C# compiler not found. Install .NET Framework 4.x Developer Pack."
}

& $csc /nologo /target:winexe /win32icon:app.ico /out:LiteOverlay.exe `
    /reference:System.Windows.Forms.dll `
    /reference:System.Drawing.dll `
    LiteOverlay.cs

if ($LASTEXITCODE -ne 0) {
    throw "LiteOverlay.exe build failed."
}

Write-Host "Built native LiteOverlay.exe successfully."
