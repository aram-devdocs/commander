#requires -Version 5
# Fast inner-loop gate (seconds): build the solution with warnings-as-errors, then the always-on
# headless tests (Core unit + architecture rules). Use scripts/audit.ps1 for the full dashboard.
# Treat any failure as the stop signal.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

Write-Host '[check] build (warnings-as-errors)...'
dotnet build "$root\Nucleus.sln" -c Release /p:TreatWarningsAsErrors=true
if ($LASTEXITCODE -ne 0) { throw 'build failed' }

Write-Host '[check] unit (Core)...'
dotnet test "$root\tests\Core\Nucleus.Domain.Tests.csproj" -c Release --no-build
if ($LASTEXITCODE -ne 0) { throw 'Core tests failed' }

Write-Host '[check] architecture rules...'
dotnet test "$root\tests\Nucleus.Architecture.Tests\Nucleus.Architecture.Tests.csproj" -c Release --no-build
if ($LASTEXITCODE -ne 0) { throw 'architecture tests failed' }

Write-Host '[check] OK'
