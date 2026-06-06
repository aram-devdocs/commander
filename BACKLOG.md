# BACKLOG — Nucleus

> Ordered queue of work-items. Each is small (≤ ~6 commits), spec-first (no code before an approved
> `specs/<phase>/<item>.md`), and tagged **HV** (headless-verifiable — unit/integration/sim/arch) or
> **PT** (needs a Unity playtest). The loop pulls the top HV item when nothing is mid-pipeline, so it
> never idles waiting on a playtest. Discoveries during work are appended here, never folded into a diff.

## Phase 0 — Tooling & ledger (HV unless noted)
- [x] P0-ledger — STATUS/BACKLOG/DECISIONS/north-star files **HV**
- [x] P0-props — root `Directory.Build.props` (conservative; monolith verified still 0-warnings, 118+11) **HV**
- [x] P0-sln — `Nucleus.sln` (4 existing projects) + `build/GameReferences.props`, `Deploy.targets`, `Packaging.props` (inert until imported) **HV**
- [x] P0-hooks — `.githooks/` modernized (Core+arch / full check.sh) + `core.hooksPath` activated; `scripts/check.ps1` + `scripts/audit.ps1` (PASS/FAIL dashboard + JSON) **HV**
- [x] P0-arch — `tests/Nucleus.Architecture.Tests` (Cecil DAG/Unity-free/ownership rules + 4 synthetic proofs the rules bite) **HV**
- [x] P0-ci — `ci.yml` modernized: always-on headless gate (Core + arch) on ubuntu + full `check.sh` when lib present. (`release.yml`/`nightly.yml` deferred — need pack targets [P6] and the Sim suite first.)
- [~] P0-testkit/sim/coverage/apisnap — **sequenced into Phase 1**: TestKit(FakeGame)/Integration/Sim/coverage/api-snapshot are built against the real extracted libs (they reference Nucleus.Domain/Campaign), so they land right after P1-domain rather than against the monolith. Tracked under Phase 1.
- [ ] P0-logaudit — `tools/Nucleus.LogAudit` CLI (parse BepInEx log → JSON verdict). **Deferred to pre-Phase-3** (first playtest); audit.ps1 already has the `-LogPath` hook stubbed. **HV**

## Phase 1 — Extract pure libs (each extraction guarded by arch + per-lib unit + coverage)
- [x] P1-domain — `libs/Nucleus.Domain` (20 closure-verified pure files); tests/Core repointed (ProjectRef + glob); codegen coreGenDir → libs/Nucleus.Domain/Generated; contract test reads mirror from Domain.dll; deploy bundles Nucleus.*.dll. Gate PASS (0w/118/9/11), deploy verified. **HV**
- [x] P1-primitives — moved RoleFamily + Composition into Domain; split CombatPhase/ForceState out of PhaseGates.cs into Domain (Command/CombatPhase.cs). Unblocks independent Squads/Production. Gate PASS. **HV**
- [x] P1-squads — `libs/Nucleus.Squads` (Squad/SquadFormer[+SquadConfig]/SquadRoster), refs Domain only (arch-verified non-vacuous). src+tests wired. Gate PASS (0w/118/9/11). (Per-lib Nucleus.Squads.Tests deferred — tests/Core aggregate covers it; see DECISIONS.) **HV**
- [x] P1-production — `libs/Nucleus.Production` (ConvoyCatalog/ProductionPlanner/ProductionQueue), Domain-only (arch-verified). Gate PASS. **HV**
- [x] P1-campaign — `libs/Nucleus.Campaign` (CommanderBrain/State/HqView/Operation/PhaseGates/Proposal/TargetPrioritizer/ThreatBoard + Planning/{AssignmentManager,BattlePlan,OrderPlanner}); refs Domain+Squads+Production. **src/Core now empty/removed.** tests/Core fully on ProjectReferences. Gate PASS (0w/118/9/11). **PHASE 1 COMPLETE.** **HV**

## Phase 2–7 — see plan (specs to be drafted as each phase is pulled)
- [x] P2-gamesdk — `libs/Nucleus.GameSdk` (all src/Game except CommanderService) + Generated/; codegen gameGenDir retargeted (regen verified identical); NucleusLog seam added to Domain (libs log without referencing Plugin); InternalsVisibleTo("CommanderLayer") preserves same-assembly accessibility. Gate PASS (0w/118/9/11). **HV**
- [x] P2-ui — `libs/Nucleus.Ui` (UiFactory/Theme/NativeColors/NativeIcons/Native.NativeUi/DragHandle/MainMenuBadge), Domain-only among Nucleus libs (Theme's ColorRgba→Color inlined to drop the GameSdk dep). Commander panels stay in app. Gate PASS (0w/118/9/11). **PHASE 2 COMPLETE** — 7 libs extracted; src is the thin app shell. **HV**
- [~] P3 — host/Platform + IMod + Commander as first mod (single plugin initially; see specs/phase-3/P3-host.md) **PT**
  - [x] P3a — `libs/Nucleus.Abstractions` contract (IMod/IModContext/IModUi/IModTickContext/ILogSink/IGameServices/IButtonRegistry/ModInfo/MapButtonSpec/MenuItemSpec/ModPlatform). Refs Domain+Ui (arch-verified). Gate PASS. **HV**
  - [~] P3b — in-process host. **Core done** (build-gated, not yet live, behavior unchanged): `src/Host/{LogSink(ILogSink), GameServices(IGameServices over GameSdk adapters), ModRegistry(lifecycle: Add/TickAll/SetEnabled/Shutdown)}`. **Pending**: ModContext/IModUi impl + ModHost + button registry + the live flip (Canvas/tick ownership) — playtest-gated. **HV(core)/PT(flip)**
  - [x] P3b-probe — **YES, headless-testable.** ModRegistry moved into Nucleus.Abstractions; tests/Nucleus.Integration.Tests drives it with FakeMod (8 tests: init-on-enable, tick-only-enabled, toggle callbacks, init-once, dup-id, shutdown). Wired into audit.ps1/check.sh (game-coupled). Gate now 5 layers. **HV**
  - [~] P3c — host LIVE (minimal). `src/Host/{ModHost, ModContext(+HostModUi/HostButtonRegistry placeholders), CommanderMod}`; Plugin registers CommanderMod; DynamicMap tick rerouted patch→Host.Tick→registry→CommanderMod→runtime. Wrap-not-rewrite (CommanderRuntime untouched). Build+5 gates green; deploy bundles all 7 libs. **Playtest queued: playtests/P3-host-tick.md** (confirm tick still drives the panel/commander). Buttons/pan/loader stay direct for now. **PT**
  - [ ] P3d — loader UI (MainMenu "MODS" → list/toggle mods, persisted) **PT**
  - [ ] P3-integration — `Nucleus.TestKit` FakeGame + `Nucleus.Integration.Tests` (lifecycle, slots, shared queue, no double-buy) **HV**
- [ ] P4 — split Build **PT**
- [ ] P5 — split Squad **PT**
- [ ] P6 — Warfare + SDK pack + dotnet template + dual-faction sim + persistence **HV (+PT)**
- [ ] P7 — rename CommanderLayer.*→Nucleus.*, `gh repo rename no_nucleus`, folder rename (human), doc rewrite **PT/human**

## Discovered (triage later)
- [x] **codegen nullable warnings** — resolved: codegen inherits `Nullable=disable` from root props
  (build tool, not SDK surface). Solution now 0 warnings under `-p:TreatWarningsAsErrors=true`.
