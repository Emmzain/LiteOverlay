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

$logoPng = Join-Path $rootDir "assets\logo.png"
if (Test-Path $logoPng) {
    Add-Type -AssemblyName System.Drawing
    $srcBmp = New-Object System.Drawing.Bitmap($logoPng)
    $ms = New-Object System.IO.MemoryStream
    $srcBmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    $pngBytes = $ms.ToArray()
    $ms.Close()
    $srcBmp.Dispose()

    $bw = New-Object System.IO.BinaryWriter([System.IO.File]::Create($iconPath))
    $bw.Write([uint16]0) # Reserved
    $bw.Write([uint16]1) # Type (1 = ICO)
    $bw.Write([uint16]1) # Count

    # Icon Entry
    $w = if ($srcBmp.Width -ge 256) { 0 } else { [byte]$srcBmp.Width }
    $h = if ($srcBmp.Height -ge 256) { 0 } else { [byte]$srcBmp.Height }
    $bw.Write([byte]0)   # Width (0 = 256)
    $bw.Write([byte]0)   # Height (0 = 256)
    $bw.Write([byte]0)   # Colors
    $bw.Write([byte]0)   # Reserved
    $bw.Write([uint16]1) # Planes
    $bw.Write([uint16]32)# BPP
    $bw.Write([uint32]$pngBytes.Length) # Size
    $bw.Write([uint32]22) # Offset (6 header + 16 entry = 22)

    $bw.Write($pngBytes)
    $bw.Close()
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

# Compile SetupLiteOverlay.exe with embedded LiteOverlay.exe
$outSetup = Join-Path $rootDir "SetupLiteOverlay.exe"
& $csc /nologo /target:winexe /win32icon:$iconPath /out:$outSetup `
    /res:$outExe,LiteOverlay.exe `
    /reference:System.Windows.Forms.dll `
    /reference:System.Drawing.dll `
    $setupCs

if ($LASTEXITCODE -ne 0) {
    throw "SetupLiteOverlay.exe build failed."
}

Copy-Item $outSetup (Join-Path $binDir "SetupLiteOverlay.exe") -Force

Write-Host "⚡ Built native LiteOverlay.exe & SetupLiteOverlay.exe successfully into root and overlay/bin/!" -ForegroundColor Green
