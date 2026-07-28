param(
    [string]$GameDomain,
    [string]$GameScopedModId
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($GameDomain)) {
    throw "Pass the Nexus game domain, for example planetcrafter."
}

if ([string]::IsNullOrWhiteSpace($GameScopedModId)) {
    throw "Pass the mod ID from the Nexus URL, for example 212."
}

$envFile = Join-Path $PSScriptRoot "..\.env"
if (-not (Test-Path -LiteralPath $envFile)) {
    throw "Create a temporary .env file with NEXUS_API_KEY before running this script."
}

$apiKey = $null
foreach ($line in Get-Content -LiteralPath $envFile) {
    if ($line -match '^\s*NEXUS_API_KEY\s*=\s*(.+?)\s*$') {
        $apiKey = $Matches[1].Trim().Trim('"').Trim("'")
        break
    }
}

if ([string]::IsNullOrWhiteSpace($apiKey)) {
    throw "NEXUS_API_KEY was not found in .env."
}

$headers = @{
    apikey = $apiKey
    "User-Agent" = "AcTePuKc-NexusIdLookup/1.0"
}

$mod = Invoke-RestMethod `
    -Uri "https://api.nexusmods.com/v3/games/$GameDomain/mods/$GameScopedModId" `
    -Headers $headers

$modId = [string]$mod.data.id
$files = Invoke-RestMethod `
    -Uri "https://api.nexusmods.com/v3/mods/$modId/files" `
    -Headers $headers

Write-Output "NEXUS_MOD_ID=$modId"
Write-Output "NEXUS_FILE_ID values:"
$files.data.mod_files |
    Select-Object id, name, is_active, versions_count, last_file_uploaded_at |
    Format-Table -AutoSize
