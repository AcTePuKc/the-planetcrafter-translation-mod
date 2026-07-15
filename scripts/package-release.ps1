param(
    [string]$Version = "0.2.0",
    [string]$Configuration = "Release",
    [string]$GameDir = "",
    [string]$OutputDir = "dist"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$buildScript = Join-Path $PSScriptRoot "build-plugin.ps1"
$distRoot = Join-Path $repoRoot $OutputDir
$pluginFolderName = "AcTePuKc UI Translation"
$packageRoot = Join-Path $distRoot $pluginFolderName
$zipPath = Join-Path $distRoot "PlanetCrafterTranslationMod-$Version.zip"
$buildDllPath = Join-Path $repoRoot "src\PlanetCrafterTranslationMod\bin\$Configuration\netstandard2.1\PlanetCrafterTranslationMod.dll"
$labelsPath = Join-Path $repoRoot "src\PlanetCrafterTranslationMod\translations\labels.txt"

if (Test-Path -LiteralPath $distRoot) {
    Remove-Item -LiteralPath $distRoot -Recurse -Force
}

$buildParams = @{
    Configuration = $Configuration
}

if ($GameDir) {
    $buildParams.GameDir = $GameDir
}

& $buildScript @buildParams

if (-not (Test-Path -LiteralPath $buildDllPath)) {
    throw "Build DLL not found: $buildDllPath"
}

if (-not (Test-Path -LiteralPath $labelsPath)) {
    throw "Translation file not found: $labelsPath"
}

$translationsDir = Join-Path $packageRoot "translations"
New-Item -ItemType Directory -Path $translationsDir -Force | Out-Null
Copy-Item -LiteralPath $buildDllPath -Destination (Join-Path $packageRoot "PlanetCrafterTranslationMod.dll") -Force
Copy-Item -LiteralPath $labelsPath -Destination (Join-Path $translationsDir "labels.txt") -Force

Compress-Archive -Path (Join-Path $packageRoot "*") -DestinationPath $zipPath -Force

[pscustomobject]@{
    PackageRoot = (Resolve-Path -LiteralPath $packageRoot).Path
    ZipPath = (Resolve-Path -LiteralPath $zipPath).Path
}
