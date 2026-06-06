# STATUS — Nucleus build ledger

> Machine-readable progress ledger. The autonomous loop reads this FIRST every wake to find the next
> action. State: TODO / WIP / DONE / PLAYTEST (Unity-gated, awaiting human) / BLOCKED.
> Update on every state transition. Source of truth for "what's next" — survives context compaction.

**Branch:** `master` (everything consolidated here; feature branches merged + deleted) · **Baseline:** `pwsh scripts/audit.ps1` → AUDIT: PASS
**Gate (7 layers):** build 0w · unit-core 129 · arch 9 · sim 22 · logaudit 7 · contract 11 · integration 9 (2026-06-06)

## Where we are
The full platform is **built, renamed, and headless-green.** The monorepo is the planned shape:
`apps/` (Nucleus.Platform host + Nucleus.Commander/Build/Squad/**Warfare** mods — all 5) over `libs/`
(8 libs: Domain/Squads/Production/Campaign pure; GameSdk/Ui engine; Abstractions host contract), plus
`sdk/` `tools/` `tests/` `build/` `docs/`. Phase 7 rename is **complete** — no `CommanderLayer`
anywhere in source/build/scripts/CI/hooks; folder == assembly == namespace throughout.

What is proven headlessly: every shared library, the host mod-registry lifecycle (register → init →
tick → enable/disable, persisted), the dependency DAG (Cecil arch rules), the game-reflection contract,
the **persistent two-faction dynamic war** (`WarfareCampaign`: both sides run the brain, whole-war
save/resume, continuation determinism), campaign persistence (snapshot/state/save/store), and the
log-audit/self-test instrumentation.

## Phase status
| Phase | Title | State | Notes |
|-------|-------|-------|-------|
| 0 | Tooling & ledger foundation | DONE | sln, props, build helpers, arch test, gate scripts, active hooks, CI |
| 1 | Extract pure libs (Domain/Squads/Production/Campaign) | DONE | per-lib test projects; arch-enforced Unity-free |
| 2 | Extract GameSdk + Ui + retarget codegen | DONE | codegen → lib Generated/ dirs; contract green |
| 3 | Stand up host; Commander first | DONE (in-game ✅) | P3-host-tick PASSED: plugin loaded, 4/4 patches, host tick reached, 0 exceptions |
| 4 | Split Build | DONE | own plugin/bezel; no-skew deploy verified; PLAYTEST pending in-game |
| 5 | Split Squad | DONE | own plugin/bezel; external SquadRoster ctor; PLAYTEST pending in-game |
| 6 | Warfare + SDK packaging + dual-faction + persistence | DONE (headless) | **Nucleus.Warfare app** built (5th mod, WAR button, owns WarfareCampaign + save/resume); dual-faction sim green; SDK packable + template; campaign save/resume seam + continuation-determinism proof |
| 7 | Rename CommanderLayer.* → Nucleus.* | DONE | source/projects/sln/scripts/CI/hooks; repo+folder rename = human touchpoint (below) |

## Remaining work
### Headless — DONE this run
- ✅ **Campaign persistence** — `libs/Nucleus.Campaign/Persistence/` = `CampaignSnapshot` +
  `CampaignState.Capture/Restore` + `CampaignSave.Serialize/Deserialize` + `CampaignStore` (crash-safe disk
  IO). Wired into `CommanderService.SaveCampaign/LoadCampaign`. Proven by 11 unit tests + a
  continuation-determinism Sim test. Spec: `specs/phase-6/P6-persistence.md`.
- ✅ **Nucleus.Warfare app (north-star)** — 5th mod: `WarfareCampaign` (both factions run the brain) +
  `WarfareSave` (whole-war save/resume) in the pure lib, driven by the `Nucleus.Warfare` plugin/IMod (WAR
  button, resumes on load, persists on shutdown). 3 dual-faction Sim tests incl. whole-war continuation
  determinism.

### Headless — still open
- **Host real UI layer** (host-owned Canvas → Build buy-menu + Squad manager + Warfare status panels) —
  *gated on the one verification run below* so it isn't built on unverified UI. Top remaining build item.
- **"Nucleus Dynamic Warfare" mission** + the in-game per-faction view feed that lets `WarfareCampaign.Step`
  drive both sides live (the headless substrate is done; the mission/game-API side is Unity-gated).

### Playtest-gated (awaiting one human verification run)
One run mechanically verifies all five mods + loader + bezel buttons at once:
1. `scripts/run.ps1` → open the map (and the MODS menu from the main menu).
2. `scripts/audit.ps1 -LogPath .sandbox/game/BepInEx/LogOutput.log`
   → confirms `loader-ui-built` / `build-mod-loaded` / `squad-mod-loaded` / `warfare-mod-loaded` /
   `bezel-buttons-attached` from the `[NUCLEUS:SELFTEST]`/`[NUCLEUS:METRIC]` lines.
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
