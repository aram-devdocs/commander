# Playtest P8 — all six issues (one comprehensive pass)

All four increments are deployed. This one run verifies every reported issue. `make dev` → load a mission
with an aircraft → maximize the map → exercise the bezel → check the main menu → exit → `make logaudit`,
then paste the log + notes back.

## Setup
- `make mission` (installs both missions, incl. "Nucleus Dynamic Warfare") — already deployed, but harmless to re-run.
- `make dev` (builds, deploys all 5 mods, launches).

## Checks (map by reported issue)

**#1 — native main-menu button**
- On the main menu, a **NUCLEUS** button appears (game's native style, in the menu's button row — not a
  floating overlay).
- Click it → a **NUCLEUS MODS** panel opens listing Commander / Build / Squad / Warfare, each a button
  reading `ON`/`OFF`. Click one → it toggles (that mod enables/disables; persists).
- Log: `[NUCLEUS:SELFTEST] PASS menu-button-added`, `[NUCLEUS:METRIC] menuMods=4`.

**#2 — bezel button placement** (load a mission with an aircraft, maximize the map)
- CMD / BLD / SQD / WAR appear in the game's native bezel style, **spread down the left and right bezels**
  (≈2 per side), not stacked in one corner.
- Log: `[NUCLEUS:METRIC] mfdMaximizeHook=1 leftButtons=… rightButtons=…`, `bezelButtons=4`,
  `[NUCLEUS:SELFTEST] PASS bezel-buttons-attached`.

**#3 — CMD no longer shows everything**
- Click **CMD** → its screen shows ONLY **manual orders** (domains / range / Attack-Defend-Capture-… / orders
  list) **+ the commander mode selector** (OFF/MANUAL/ASSIST/AUTO). No build/squads/operations in CMD.

**#4 — the other buttons do something**
- **BLD** → a buy menu (convoys with cost; BUY buttons).
- **SQD** → the squad list (name · family ×N — activity) with per-squad AUTO/MANUAL.
- **WAR** → the campaign view: operations + battle feed.

**#5 — green "open" highlight**
- Clicking any bezel button lights the **native green highlight border** on its screen (like the game's own
  screens); clicking again clears it. Only one screen per side is open at a time (native behavior).

**#6 — closes with the map**
- With a screen open, **close the map** → the screen closes (native `onMapMinimized`). Reopen the map → it's
  closed, not lingering.

## Return
- `make logaudit` output (and paste `.sandbox/game/BepInEx/LogOutput.log`).
- For each of #1–#6: pass/fail + a note. Especially: button placement (spacing/side split), whether each
  panel fits the MFD screen area or overflows, and the green highlight.

## Notes / known tuning risks (UI can't be verified headlessly)
- Bezel slot spacing is computed from the native buttons; it may need nudging.
- The mod panels render into the native MFD screen's content area; sizing may need a tweak to fit.
- If the NUCLEUS panel appears centered/over other menu items, its position/size is easy to adjust.
- The "Nucleus Dynamic Warfare" mission reuses the Commander Debug unit layout (a known-good combined-arms
  sandbox) — the bespoke two-faction layout comes later.
