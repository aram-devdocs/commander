# Nucleus playtest worksheet

Copy this file, fill it in during a playtest, and paste it back (or save under `playtests/results/`). Keep the
Action/Expected rows; put your result in **Observed** + tick Pass/Fail. The agent parses this into the backlog.

**Build under test:** branch `___________`  commit `___________`  date `___________`
**Mission:** Commander Debug (or: `___________`)

## Scenarios

| # | Action | Expected | Observed | Pass/Fail |
|---|--------|----------|----------|-----------|
| 1 | Launch to main menu | "Commander mod loaded" badge shows | | ☐P ☐F |
| 2 | Load mission, open map, click **CMD** | Commander panel opens | | ☐P ☐F |
| 3 | Watch the open panel ~5s | Panel content (orders/squads/feed) updates | | ☐P ☐F |
| 4 | Set mode = **AUTO**, wait ~45s | Objectives/squads appear; "buying convoy" lines | | ☐P ☐F |
| 5 | Arm an order + click the map | Units get tasked; markers/lines draw | | ☐P ☐F |
| 6 | Set mode = **OFF** | Commander stops; native AI runs | | ☐P ☐F |
| 7 | (if present) **MODS** menu in main menu | Lists Commander; toggle works | | ☐P ☐F |

## Log + exceptions
- BepInEx log path: `.sandbox/game/BepInEx/LogOutput.log`
- Saw the four `Patched:` lines?  ☐ yes ☐ no
- Saw `CommanderRuntime first Tick`?  ☐ yes ☐ no
- **Any exceptions / `Update tick threw`?**  ☐ none ☐ yes →
  ```
  (paste exception text here)
  ```
- Paste the relevant log tail:
  ```
  (paste log here)
  ```

## Screenshots
- (attach: open panel; AUTO running; map with an order placed)

## What felt off (free-form)
- UX/perf/clarity issues, jerky UI, anything surprising:
  ```

  ```
