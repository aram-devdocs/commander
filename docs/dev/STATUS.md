# STATUS — Nucleus build ledger

> Machine-readable progress ledger. The autonomous loop reads this FIRST every wake to find the next
> action. State: TODO / WIP / DONE / PLAYTEST (Unity-gated, awaiting human) / BLOCKED.
> Update on every state transition. Source of truth for "what's next" — survives context compaction.

**Branch:** `nucleus-platform` · **Baseline (known-good):** `pwsh scripts/audit.ps1` → AUDIT: PASS
**Gate (7 layers):** build 0w · unit-core 125 · arch 9 · sim 19 · logaudit 7 · contract 11 · integration 9 (2026-06-06)

## Where we are
The full platform is **built, renamed, and headless-green.** The monorepo is the planned shape:
`apps/` (Nucleus.Platform host + Nucleus.Commander/Build/Squad mods) over `libs/` (8 libs:
Domain/Squads/Production/Campaign pure; GameSdk/Ui engine; Abstractions host contract), plus
`sdk/` `tools/` `tests/` `build/` `docs/`. Phase 7 rename is **complete** — no `CommanderLayer`
anywhere in source/build/scripts/CI/hooks; folder == assembly == namespace throughout.

What is proven headlessly: every shared library, the host mod-registry lifecycle (register → init →
tick → enable/disable, persisted), the dependency DAG (Cecil arch rules), the game-reflection contract,
the dual-faction campaign sim over the real brain, and the log-audit/self-test instrumentation.

## Phase status
| Phase | Title | State | Notes |
|-------|-------|-------|-------|
| 0 | Tooling & ledger foundation | DONE | sln, props, build helpers, arch test, gate scripts, active hooks, CI |
| 1 | Extract pure libs (Domain/Squads/Production/Campaign) | DONE | per-lib test projects; arch-enforced Unity-free |
| 2 | Extract GameSdk + Ui + retarget codegen | DONE | codegen → lib Generated/ dirs; contract green |
| 3 | Stand up host; Commander first | DONE (in-game ✅) | P3-host-tick PASSED: plugin loaded, 4/4 patches, host tick reached, 0 exceptions |
| 4 | Split Build | DONE | own plugin/bezel; no-skew deploy verified; PLAYTEST pending in-game |
| 5 | Split Squad | DONE | own plugin/bezel; external SquadRoster ctor; PLAYTEST pending in-game |
| 6 | Warfare + SDK packaging + dual-faction + persistence | DONE (headless) | dual-faction sim green; SDK packable + template; **campaign save/resume seam done** (CampaignSnapshot/State/Save + continuation-determinism proof) |
| 7 | Rename CommanderLayer.* → Nucleus.* | DONE | source/projects/sln/scripts/CI/hooks; repo+folder rename = human touchpoint (below) |

## Remaining work
### Headless (the loop can do now)
- ✅ **Campaign persistence** — DONE. `libs/Nucleus.Campaign/Persistence/` = `CampaignSnapshot` (resumable
  state), `CampaignState.Capture/Restore` (live ↔ snapshot), `CampaignSave.Serialize/Deserialize`
  (dependency-free versioned text, Mono/BepInEx-safe). Proven by 7 unit tests + a `DualSimWorld`-style
  **continuation-determinism** test (save → continue original vs restore → continue: identical traces).
  Spec: `specs/phase-6/P6-persistence.md`. Next deepening: wire save/load into the live host (Unity-gated).
- **Host real UI layer** (host-owned Canvas → Build buy-menu + Squad manager panels) — *gated on the
  one verification run below* so it isn't built on unverified UI. Now the top remaining build item once the
  verification run lands.

### Playtest-gated (awaiting one human verification run)
One run mechanically verifies all four mods + loader + bezel buttons at once:
1. `scripts/run.ps1` → open the map (and the MODS menu from the main menu).
2. `scripts/audit.ps1 -LogPath .sandbox/game/BepInEx/LogOutput.log`
   → confirms `loader-ui-built` / `build-mod-loaded` / `squad-mod-loaded` / `bezel-buttons-attached`
   from the `[NUCLEUS:SELFTEST]`/`[NUCLEUS:METRIC]` lines.
Packet: `playtests/P-apps-split.md`. Result lands in `playtests/results/`.

### Outward actions (human only — prepared, parked for explicit go)
- `gh repo rename no_nucleus` (current remote is `commander`) + local folder rename
  `C:\Users\aram\dev\nuclear_option_command` → `no_nucleus` (close session, rename, reopen; update remotes).
- Publish: nuget.org (SDK), Thunderstore (mods), Steam Workshop (mission) — accounts + GH secrets per
  `docs/DEPLOYMENT.md`.

## Pending playtests (Unity-gated)
- ✅ **P3-host-tick — PASSED** (`playtests/results/P3-host-tick.md`). Host flip confirmed in-game.
- ⏳ **P-apps-split** — the one combined run above; auto-audited via LogAudit.

## Gates / commands
- Fast: `pwsh scripts/check.ps1` (build + changed-project unit + arch)
- Full: `pwsh scripts/audit.ps1` (7 layers → PASS/FAIL dashboard + `artifacts/audit-summary.json`)
- Log audit: `pwsh scripts/audit.ps1 -LogPath <BepInEx log>` (turns a playtest into a mechanical verdict)
