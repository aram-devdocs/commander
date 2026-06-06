# Playtest packet — P3c: host-driven tick

**What changed (and why it needs your eyes):** the per-frame tick that drives the Commander now flows through
the new in-process mod host (`DynamicMap.Update` patch → `Host.Tick` → `ModRegistry.TickAll` → `CommanderMod`
→ the existing `CommanderRuntime`). Everything else (Canvas, CMD button, panel, AUTO commander) is the
**unchanged** proven code — only the tick’s entry point moved. This is build-green + 5 headless gate layers
green, but “the tick still fires in-game” can only be confirmed by you. If the panel opens and updates, the
reroute works.

**Build is already deployed** to `.sandbox/game/BepInEx/plugins/CommanderLayer/` (plugin + 7 Nucleus libs).
To (re)launch: `pwsh scripts/run.ps1` (or run the sandbox NuclearOption.exe). Mission: **Commander Debug**.

## Steps & what to confirm
1. **Main menu** — the “Commander mod loaded” badge appears (mod loaded OK).
2. **Load Commander Debug**, open the map, click the **CMD** bezel button → the Commander panel opens.
   - ✅ PASS if the panel opens and its content (orders/squads/feed) renders and updates over a few seconds.
   - ❌ FAIL if the panel never opens or never updates (would mean the host tick isn’t reaching the runtime).
3. **Set mode to AUTO** in the panel. Watch ~30–60s.
   - ✅ PASS if the commander does its thing (objectives/squads appear, production/“buying convoy” lines).
4. **Place a manual order** (arm + click map) — units get tasked as before.
5. **No exceptions** in the BepInEx console.

## Capture & return
Paste into `playtests/results/P3-host-tick.md`:
- The **BepInEx log** (`.sandbox/game/BepInEx/LogOutput.log`) — especially:
  - `Commander Layer loaded.` and the four `Patched: ...` lines,
  - `CommanderRuntime first Tick — driver alive.` (proves the host-driven tick reached the runtime),
  - `Commander panel built.` / `[panel] ...`,
  - any `Update tick threw:` or other exceptions (should be none).
- One line: **PASS/FAIL** for steps 2, 3, 4, and whether you saw any exceptions.
- Optional: a screenshot of the open panel.

That’s all I need — I’ll fold the result back into the loop and proceed to the loader UI (P3d) + the
Build/Squad mod splits. Meanwhile the loop continues on headless-verifiable work.
