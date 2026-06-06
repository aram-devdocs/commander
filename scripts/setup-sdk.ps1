#requires -Version 5
# Populate ./lib with the game/Unity/BepInEx DLLs the Nucleus SDK references (build-time only; never shipped).
# For external mod developers: point -GamePath at your Nuclear Option install that has BepInEx installed.
# Example: pwsh scripts/setup-sdk.ps1 -GamePath "C:\Program Files (x86)\Steam\steamapps\common\NuclearOption"
param(
  [Parameter(Mandatory = $true)]
  [string]$GamePath
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$lib  = Join-Path $root 'lib'
New-Item -ItemType Directory -Force -Path $lib | Out-Null

$managed = Join-Path $GamePath 'NuclearOption_Data\Managed'
$bepCore = Join-Path $GamePath 'BepInEx\core'
if (-not (Test-Path $managed)) { throw "Not found: $managed (is -GamePath the Nuclear Option install root?)" }
if (-not (Test-Path $bepCore)) { throw "Not found: $bepCore (install BepInEx 5 into the game first)" }

# From BepInEx (link against the exact shipped versions, not NuGet HarmonyX).
$fromBep = @('BepInEx.dll', '0Harmony.dll')
# From the game's managed assemblies.
$fromManaged = @(
  'Assembly-CSharp.dll', 'Mirage.dll', 'Unity.TextMeshPro.dll',
  'UnityEngine.dll', 'UnityEngine.CoreModule.dll', 'UnityEngine.PhysicsModule.dll',
  'UnityEngine.TerrainModule.dll', 'UnityEngine.IMGUIModule.dll', 'UnityEngine.InputLegacyModule.dll',
  'UnityEngine.UIModule.dll', 'UnityEngine.TextRenderingModule.dll', 'UnityEngine.UI.dll'
)

function Copy-Dll($srcDir, $name) {
  $src = Join-Path $srcDir $name
  if (-not (Test-Path $src)) { Write-Warning "missing: $src"; return }
  Copy-Item $src (Join-Path $lib $name) -Force
}

foreach ($d in $fromBep)     { Copy-Dll $bepCore $d }
foreach ($d in $fromManaged) { Copy-Dll $managed $d }

Write-Host "[setup-sdk] populated $lib with $((Get-ChildItem $lib -Filter *.dll).Count) DLLs."
Write-Host "[setup-sdk] these are build-time references only (Private=false) and are never shipped in your mod or the SDK packages."
