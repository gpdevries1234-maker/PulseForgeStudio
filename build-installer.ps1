$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
  throw 'Install .NET 8 SDK from https://dotnet.microsoft.com/download/dotnet/8.0'
}

if (-not (Test-Path 'Assets\PulseForge.ico')) {
  Add-Type -AssemblyName System.Drawing
  $bmp = New-Object Drawing.Bitmap 256,256
  $g = [Drawing.Graphics]::FromImage($bmp)
  $g.Clear([Drawing.Color]::FromArgb(17,21,29))
  $pen = New-Object Drawing.Pen([Drawing.Color]::FromArgb(155,108,255),18)
  foreach ($x in @(55,90,125,160,195)) {
    $height = 50 + ([Math]::Abs(125-$x) * 0.7)
    $g.DrawLine($pen,$x,$height,$x,256-$height)
  }
  $icon = [Drawing.Icon]::FromHandle($bmp.GetHicon())
  $fs = [IO.File]::Create((Join-Path $root 'Assets\PulseForge.ico'))
  $icon.Save($fs); $fs.Close(); $g.Dispose(); $bmp.Dispose()
}

dotnet restore
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish

$iscc = @(
  "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
  "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1

if ($iscc) {
  & $iscc 'Installer\PulseForge.iss'
  Write-Host 'Installer ready: dist\PulseForgeStudio-Setup-x64.exe' -ForegroundColor Green
} else {
  Write-Host 'Portable app ready in publish\. Install Inno Setup 6 and rerun this script to create the installer.' -ForegroundColor Yellow
}
