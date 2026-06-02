param(
    [string]$Configuration = "Release",
    [string]$GameDir = ""
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$buildScript = Join-Path $PSScriptRoot "build-plugin.ps1"
$distRoot = Join-Path $repoRoot "dist"

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
