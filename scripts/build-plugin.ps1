param(
    [string]$Configuration = "Debug",
    [string]$GameDir = "",
    [switch]$NoRestore
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$solutionPath = Join-Path $repoRoot "PlanetCrafterTranslationMod.sln"

if (-not (Test-Path -LiteralPath $solutionPath)) {
    throw "Solution file not found: $solutionPath"
}

$buildArgs = @(
    "build",
    $solutionPath,
    "-c", $Configuration
)

if ($NoRestore) {
    $buildArgs += "--no-restore"
}

if ($GameDir) {
    $buildArgs += "-p:GameDir=$GameDir"
}

Write-Host "Running: dotnet $($buildArgs -join ' ')"
& dotnet @buildArgs

if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed with exit code $LASTEXITCODE"
}
