param(
    [switch]$Test,
    [switch]$Captures,
    [ValidateSet('en', 'fr')][string]$Language = 'en'
)
$ErrorActionPreference = 'Stop'
$projectRoot = Join-Path $PSScriptRoot 'prototype'
$godotFolder = 'C:\dev\tools\godot\Godot_v4.6.1-stable_mono_win64'
$godotConsole = Join-Path $godotFolder 'Godot_v4.6.1-stable_mono_win64_console.exe'
$godotGame = Join-Path $godotFolder 'Godot_v4.6.1-stable_mono_win64.exe'
if (-not (Test-Path -LiteralPath $godotConsole)) {
    throw "Godot .NET 4.6.1 introuvable : $godotConsole. Ajuster godotFolder et prototype/NuGet.Config si Godot est installe ailleurs."
}
$env:NUGET_PACKAGES = Join-Path $PSScriptRoot '.cache\nuget'
Push-Location -LiteralPath $projectRoot
try {
    & dotnet build (Join-Path $projectRoot 'EvolutionTwo.csproj') --nologo
    if ($LASTEXITCODE -ne 0) { throw 'La compilation a echoue.' }
    $needsImport = $false
    foreach ($portraitAsset in (Get-ChildItem -LiteralPath (Join-Path $projectRoot 'assets') -Filter '*.png' -Recurse)) {
        $importedPortrait = Get-ChildItem -LiteralPath (Join-Path $projectRoot '.godot\imported') -Filter ($portraitAsset.Name + '-*.ctex') -ErrorAction SilentlyContinue | Select-Object -First 1
        if (-not $importedPortrait -or $portraitAsset.LastWriteTimeUtc -gt $importedPortrait.LastWriteTimeUtc) { $needsImport = $true }
    }
    if ($needsImport) {
        & $godotConsole --headless --path $projectRoot --editor --import --quit --log-file (Join-Path $projectRoot 'playtest-import.log')
        if ($LASTEXITCODE -ne 0) { throw "L'import Godot a echoue." }
    }
    if ($Test) {
        & $godotConsole --headless --path $projectRoot --log-file (Join-Path $projectRoot 'playtest-smoke.log') -- --smoke-test "--lang=$Language"
        if ($LASTEXITCODE -ne 0) { throw 'Les verifications ont echoue.' }
    } elseif ($Captures) {
        $capturesFolder = Join-Path $PSScriptRoot '.cache\captures\flow'
        New-Item -ItemType Directory -Path $capturesFolder -Force | Out-Null
        & $godotConsole --path $projectRoot --log-file (Join-Path $projectRoot 'playtest-capture.log') -- --capture-flow "--lang=$Language" | ForEach-Object {
            if ($_ -match '^GAME_CAPTURE:([a-z0-9-]+\.png):(.+)$') {
                $capturePath = Join-Path $capturesFolder $Matches[1]
                $captureBytes = [Convert]::FromBase64String($Matches[2])
                Set-Content -LiteralPath $capturePath -Value $captureBytes -Encoding Byte
                Write-Output "CAPTURE: $capturePath"
            } else { Write-Output $_ }
        }
        if ($LASTEXITCODE -ne 0) { throw 'La capture a echoue.' }
    } else {
        & $godotGame --path $projectRoot --log-file (Join-Path $projectRoot 'playtest-game.log') -- "--lang=$Language"
        if ($LASTEXITCODE -ne 0) { throw 'Le jeu a quitte avec une erreur.' }
    }
} finally {
    Pop-Location
}
