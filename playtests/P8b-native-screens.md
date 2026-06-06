# Playtest P8b — native MFD screens (Increment 1)

**What changed (fixes #2, #5, #6; CMD now native):** each bezel button is now a **native MFD button paired
with a native `MFDScreen`**. The button is cloned from a real bezel button (native look) and repositioned
into a free slot down the left/right bezels (no more stacking — **#2**); pressing it goes through the game's
`PressLeftButton/PressRightButton`, which lights the native green **highlight** border (**#5**) and is hidden
by the game's `onMapMinimized` so the panel closes when the map closes (**#6**). The Commander panel now
renders *inside* its MFD screen's content area (no separate overlay canvas). BLD/SQD/WAR open native screens
with a placeholder for now (their real content is the next increment).

## Run
1. `make dev`  (or `scripts/run.ps1`)
2. Load a mission with an aircraft; **maximize the map**.
3. Observe the bezel:
   - **#2** CMD/BLD/SQD/WAR appear in the game's native button style, **spread down the left and right
     bezels** (roughly 2 per side), not stacked in one spot.
   - **#5** click a button → it should get the **green highlight border** like native screens; click again →
     highlight clears.
   - CMD opens the Commander panel **inside the MFD screen area**; BLD/SQD/WAR open a placeholder panel.
   - **#6** close the map → the open panel closes; reopen the map → it's closed (not lingering).
4. Exit, then `make logaudit`.

## Log lines to confirm (paste the log back)
- `[NUCLEUS:METRIC] mfdMaximizeHook=1 leftButtons=<n> rightButtons=<n>` (hook fired; native button counts).
- `[NUCLEUS:METRIC] bezelButtons=4` + `[NUCLEUS:SELFTEST] PASS bezel-buttons-attached`.
- `Commander panel built into native MFD screen.`
- No exceptions.

## What to note for tuning
- Are the four buttons placed sensibly (spacing/side split), or overlapping/off-screen? (slot pitch tuning)
- Does the green highlight appear correctly?
- Does the Commander panel fit inside the MFD screen area, or overflow? (content sizing — next pass)
- Does closing the map close the panel?

## Known not-yet-done (next increments)
- **#3/#4**: BLD/SQD/WAR show placeholders; real per-mod content (buy menu / squads / campaign) + slimming
  CMD to orders+mode is Increment 2.
- **#1**: native "NUCLEUS" main-menu entry is Increment 3.
