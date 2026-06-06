<#
.SYNOPSIS
  Automated in-game smoke test: build + deploy, launch the sandboxed game, wait for the mod's self-test
  markers in the BepInEx log (or an exception / timeout), kill the game, and print a PASS/FAIL verdict via
  the log markers. Lets the dev verify the in-game integration without a human playtest.

  -WaitMarker  : a log substring that signals the test reached its checkpoint (default: menu-button-added).
  -TimeoutSec  : how long to wait for the marker before giving up (default 90).
  -NoBuild     : skip the build/deploy step (test the already-deployed build).
#>
[CmdletBinding()]
param(
    [string]$WaitMarker = "menu-button-added",
    [int]$TimeoutSec = 90,
    [switch]$NoBuild
)
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$exe  = Join-Path $repo '.sandbox\game\NuclearOption.exe'
$log  = Join-Path $repo '.sandbox\game\BepInEx\LogOutput.log'
if (-not (Test-Path $exe)) { throw "Sandbox exe not found at $exe (run scripts/setup-sandbox.ps1)." }

if (-not $NoBuild) {
    Write-Host "[smoke] build + deploy..." -ForegroundColor Cyan
    dotnet build (Join-Path $repo 'Nucleus.sln') -c Release -p:TreatWarningsAsErrors=true | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "build failed" }
}

# Truncate the log so we only read THIS run.
if (Test-Path $log) { Clear-Content $log -ErrorAction SilentlyContinue }

Get-Process NuclearOption -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Write-Host "[smoke] launching game (waiting for marker '$WaitMarker', timeout ${TimeoutSec}s)..." -ForegroundColor Cyan
$proc = Start-Process $exe -WorkingDirectory (Split-Path $exe -Parent) -PassThru

$deadline = (Get-Date).AddSeconds($TimeoutSec)
$hit = $null; $exceptionSeen = $false
while ((Get-Date) -lt $deadline) {
    Start-Sleep -Seconds 3
    if ($proc.HasExited) { break }
    if (Test-Path $log) {
        $text = Get-Content $log -Raw -ErrorAction SilentlyContinue
        if ($text -match [regex]::Escape($WaitMarker)) { $hit = $true; break }
        if ($text -match 'NullReferenceException|MissingMethodException|MissingFieldException') { $exceptionSeen = $true; break }
    }
}

Write-Host "[smoke] stopping game..." -ForegroundColor Cyan
Get-Process NuclearOption -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 1

if (-not (Test-Path $log)) { Write-Host "[smoke] FAIL — no log produced." -ForegroundColor Red; exit 1 }

Write-Host "`n=== [NUCLEUS] log lines ===" -ForegroundColor Yellow
Select-String -Path $log -Pattern 'NUCLEUS|Patched:|loaded\.|Exception|Error' | ForEach-Object { $_.Line }

$exc = Select-String -Path $log -Pattern 'NullReferenceException|MissingMethodException|MissingFieldException|StackTrace' | Measure-Object
Write-Host ""
if ($hit -and $exc.Count -eq 0) { Write-Host "[smoke] PASS — marker '$WaitMarker' reached, no exceptions." -ForegroundColor Green; exit 0 }
elseif ($exceptionSeen -or $exc.Count -gt 0) { Write-Host "[smoke] FAIL — exception in log." -ForegroundColor Red; exit 1 }
else { Write-Host "[smoke] INCONCLUSIVE — marker '$WaitMarker' not seen within ${TimeoutSec}s (game may need manual menu nav)." -ForegroundColor Yellow; exit 2 }
