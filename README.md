# Commander

A **commander layer** mod for the game **[Nuclear Option](https://store.steampowered.com/app/2168680/Nuclear_Option/)**
(Steam App `2168680`, by Shockfront Studios). Instead of piloting every unit, you open the map, drop an
objective, and the game's own AI moves suitable units to it — *Majesty*, not an RTS.

> ⚠️ This is an unofficial, community code mod built with BepInEx + HarmonyX. It is not affiliated with or
> endorsed by Shockfront Studios.

## What it does (milestone 1)

- **Main menu badge** — confirms the mod loaded.
- **In-map CMD button** — opening the map (`M`) adds a **CMD** button to a blank MFD bezel slot. Clicking it
  opens the Commander panel (the panel is the "modal" for that button).
- **Give an order** — pick **Attack / Defend / Capture / Resupply**, choose **Air/Land/Sea** + a pull
  **range**, then click the map. The mod **selectively** tasks only the role-appropriate, in-range,
  commandable units (ground vehicles + ships) via per-unit move/hold commands — it does **not** drag your
  whole faction in. A live preview ring shows the radius and how many units will respond before you click.
- **Manage** — a throttled loop reassigns on unit loss, completes an Attack when the area is clear, and
  garrisons (holds) Defend units on arrival. The panel lists active orders (with per-order clear); the map
  draws a color-coded marker + lines to assigned units.

Config (arrive radius, optional arm key) lives in the in-game **F1** ConfigurationManager menu.

## How it works (the integration)

The game is Unity (Mono backend) + uGUI/TextMeshPro. The mod:
- Reads the local faction (`GameManager.GetLocalHQ/GetLocalFaction`) and units (`UnitRegistry.allUnits`),
  classifies each by role from its `UnitDefinition` (`typeIdentity`/`roleIdentity` + the game's
  `VehicleType`/`ShipType` enums), and reads **fog-of-war** threats from `FactionHQ.trackingDatabase`.
- Plans a suitable subset (pure Core logic) and commands them per-unit via `UnitCommand.SetDestination` /
  `SetHoldPosition` (the highest-priority AI behavior) — replacing the old faction-objective "stampede."
- Drives its per-frame loop from a Harmony postfix on `DynamicMap.Update` (this game does not pump a
  MonoBehaviour `Update` on mod-created objects), and attaches the CMD button from a postfix on
  `VirtualMFD.VirtualMFD_onMapMaximized`.
- Tasks **aircraft** (which are *not* `ICommandable`) by steering the pilot's idle no-target state
  (`AIPilotCombatModes.NoTarget`) toward Air-domain order points — deliberately **not** a faction
  Objective, because the decompile shows idle ground vehicles and ships also seek the nearest objective
  (the old "stampede"). Behind the `EnableAircraftTasking` config (off by default; needs in-game tuning).
- Borrows the game's own look: native HUD font (`GameAssets.playerNameFont`), HUD friend/hostile colors
  (`GameAssets.HUDFriendly/HUDHostile`) for the placement ring cue, and a sliced button sprite.

### Type-safety against the game — a generated SDK

The plugin **compiles against the real, unmodified `Assembly-CSharp.dll`** (not a publicized copy), so the
C# compiler enforces the game's actual accessibility — any private access is a build error caught locally,
never a runtime `FieldAccessException`.

On top of that, a **single declarative manifest** in `tools/CommanderLayer.CodeGen` lists every game member
the mod depends on. Running the generator verifies all of them against the real assembly (failing loudly,
listing every drift at once) and emits, all from that one source of truth:
- `src/Core/Generated/GameEnums.generated.cs` — mirror enums so pure Core stays engine-free,
- `src/Core/Generated/GameRef.generated.cs` — verified reflection member-name constants,
- `src/Game/Generated/GameSdk.generated.cs` — **typed** reflection accessors (field types discovered from
  the assembly, no magic strings),
- `tests/GameContract/GameContract.Generated.cs` — a contract test asserting every member still exists.

So when the game updates: **regenerate → codegen and/or the compiler and/or CI point at exactly where the
mod no longer fits.** Bespoke contract checks (enum value-equality, reference resolution, interface graphs)
live alongside in `tests/GameContract/`.

## Architecture (atomic / one-directional dependency flow)

```
src/Core/        pure C#, no Unity — Model + Roles/RoleClassifier + Planning (OrderPlanner,
                 AssignmentManager, ThreatAssessor) + Ports + Generated (codegen)   (unit-tested)
src/Game/        adapters over Assembly-CSharp/Unity — GameClassifier/GameRoster/GameIntel/
                 GameUnitCommands/CommanderService (only layer touching the game)
src/Ui/          atomic uGUI — UiFactory atoms → CommanderPanel/MapOverlay → CommanderMapScreen
src/Patches/     Harmony seams (menu badge, map tick driver + pan guard, MFD CMD button)
src/Composition/ CommanderRuntime — builds the graph, owns the loop
src/Plugin.cs    BepInEx entry / composition root
tools/CommanderLayer.CodeGen   one manifest -> mirror enums + typed SDK accessors + contract tests
tests/Core/      xUnit over Core only (no engine) — planner/classifier/manager
tests/GameContract/  Mono.Cecil tests (generated manifest contract + bespoke checks)
```

`Plugin → Composition → {Patches, Ui, Game} → Core`. The Core planner (`OrderPlanner`/`AssignmentManager`)
is pure and unit-tested; only `Game/` references the engine — so a game update breaks `Game/` +
`Patches/`, not the logic or UI.

## Build, test, run

Prereqs: **.NET SDK 8**, the game installed via Steam, and **Git Bash** (Windows). The build needs the
game's DLLs, which are **not** in this repo (they're the game's IP — see [Legal](#legal)).

```bash
# 1. one-time: build the gitignored .sandbox mirror, install BepInEx, copy game DLLs into lib/ (PowerShell)
pwsh ./scripts/setup-sandbox.ps1

# 2. (optional) install the demo mission so it shows in the in-game browser (PowerShell)
pwsh ./scripts/install-demo-mission.ps1

# 3. quality gate — builds against the real assembly + runs all tests (no game launch needed)
./scripts/check.sh

# 4. build the mod (auto-deploys into .sandbox) and launch the modded game (Steam must be running)
./scripts/run.sh
```

Enable the shared git hooks once per clone:

```bash
git config core.hooksPath .githooks
```

## Quality gates

- **pre-commit** (`.githooks/pre-commit`) — fast: Core logic tests.
- **pre-push** (`.githooks/pre-push`) — full `scripts/check.sh` when the game DLLs are present, else Core
  tests. Mirrors CI so "passes locally" means "passes CI".
- **CI** (`.github/workflows/ci.yml`) — runs Core tests on every push/PR; also builds the plugin and runs
  the game-contract tests when the game DLLs are available to the runner.
- **Branch protection** — `master` requires the CI `test` check to pass (strict) before merge.

The game-contract tests (`tests/GameContract`) load the real `Assembly-CSharp.dll` via Mono.Cecil
(metadata only — no Unity execution) and assert every game member the mod calls or reflects on still exists
with the expected type/accessibility. A game update that renames something fails CI here, not in play.

## Try the demo

1. `./scripts/run.sh` → on the main menu you should see "Commander mod loaded".
2. Load the **Commander Debug** mission → join **Boscali** → spawn the trainer.
3. Press **M** → click the **CMD** bezel button → **Place Objective** → click the map.
4. The two Boscali corvettes head to your objective; the panel and map overlay update.

## Pinned environment

| Thing | Value |
|---|---|
| Game Steam buildid | `23062134` (baseline; ≈0.33.3+) — after any game update, re-run `setup-sandbox.ps1` + `check.sh` |
| Game build-hash | `2fdbba8b7` (internal content hash from build-hash.txt) |
| Mod stack | BepInEx 5 (x64 Mono) + HarmonyX |
| Plugin TFM | `netstandard2.1`; tests `net8.0` |

> The Mono.Cecil contract tests (`tests/GameContract`) are the **version-drift guard**: they assert every
> game member the mod uses still exists with the expected shape against `lib/Assembly-CSharp.dll`. If a game
> update changes those, `check.sh` fails there (pre-launch), not in-game.

## Legal

The game's assemblies (`Assembly-CSharp.dll`, `UnityEngine.*`, etc.) are the property of Shockfront Studios
and are **not** distributed here — `lib/`, `.sandbox/`, and `decompiled/` are gitignored. You must own the
game; `scripts/setup-sandbox.ps1` sources those DLLs from your local install. This project is for
interoperability/modding of software you own. MIT-licensed for the mod's own source (see `LICENSE`).
