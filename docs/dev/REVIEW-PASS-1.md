# Review Pass 1 — competing-agent adversarial review work-list

Source: Workflow `adversarial-review-pass` (6 competing reviewer lenses → adversarial verification).
29 findings raised, **29 confirmed real, 0 rejected**. Sorted by value (impact×confidence), then severity.
All confirmed headless-safe (no-deploy build + unit tests; NO game launch needed).

Constraints while working: user is GAMING → headless only, `-p:Sandbox=C:\__nodeploy__`, local commits only,
NO push, NO smoke/visual probe. Verify each wave: full no-deploy build (0 warn) + relevant tests; commit locally.

## Waves (grouped for cohesive commits)

### Wave A — correctness (highest value) ✅ planned
- [A1] (v4, HIGH) **Move/retype objective never re-routes already-tasked units** — `CommanderBrain.cs:187-189`.
  De-dup keyed only on objective id; Move/EditObjective mutate Position/Kind in place → unit keeps old dest.
  Fix: store id + Fnv1a signature of (position,targetId,verb) in LastObjectiveByUnit; re-task when it differs.
  Keep dict<string,string> (save schema intact). NO string.GetHashCode. +BrainTests re-task-on-move test.
- [A4] (v3, MED) **Home-defense starved when auto-objective cap full** — `CommanderBrain.cs:102-128`.
  GenerateDefense() only runs inside `if(room>0)`. Hoist defense out of the cap; Take(room) applies to offense
  only. +BrainRepertoireTests test (6 offensive autos + home threat → DefendArea still created).

### Wave B — persistence robustness
- [B5] (v3, LOW) **CampaignSave.Deserialize crashes on truncated known records** — `CampaignSave.cs:120-169`.
  Add bounds-checked `PS(f,i)` accessor / skip-short-record; +PersistenceTests truncated-record test.
- [B6] (v3, LOW) **Save not crash-safe (Delete-then-Move no-file window)** — `CampaignStore.cs:20-23` + `WarfareSave.cs:128-131`.
  Use `File.Replace(tmp,path,null)` when dest exists, else Move. +CampaignStoreTests overwrite test. Fix doc comment.

### Wave C — dead-code removal (legacy manual-order UI; must be done together)
- [C7]  (v3, HIGH) Dead `PanelSections.Orders` block + `Render(orders,…)` overload + supporting members — `CommanderPanel.cs`.
- [C8]  (v3, HIGH) Dead `MapOverlay.Render(orders…)` + entire SetHover/hover-ring machinery — `MapOverlay.cs`.
- [C9]  (v3, MED) Dead `CommanderService.PlaceOrder/PreviewAt` (+_mgr order-tracking, RefreshAirIntent manual loop, Clear/ClearAll, ICampaign.Orders) — `CommanderService.cs` + `ICampaign.cs`.
- [C10] (v3, MED) `OrderColors.cs` 100% dead once C7/C8 land (also a latent palette skew vs ObjectiveVisuals) — delete file.
- [C26] (v2, LOW) `BattlePlan.Label(OrderPhase)` sole caller is dead C7 code — delete Label, keep PhaseOf.
  Drop the now-null `onArm/onClearAll/onClearOrder` ctor params; update 4 call sites (CommanderRuntime/WarfareMod/SquadMod/BuildMod).

### Wave D — performance (render path)
- [D2]  (v4, HIGH) **WarfareMod renders panel+scoreboard every frame** — `WarfareMod.cs:179-182`. Throttle to ~0.14s like CommanderRuntime.
- [D15] (v3, MED) HqView.Build LINQ + per-squad dict/Join allocs — `HqView.cs:134-191`. Pre-sized foreach + reused scratch dict in CompositionLabel. KEEP the MemberUnitIds defensive copy.
- [D16] (v3, MED) AutoHq rebuilds ~400-entry role dict every render — `CommanderService.cs:154-159`. Cache `_roleMap`, refresh after each `LastRoster=`.
- [D17] (v3, LOW) PositionsById allocs dict every render — `CommanderRuntime.cs:264-269`. Cache keyed off ReferenceEquals(LastRoster).
- [D28] (v2, LOW) GameRoster id-string alloc per unit per build — `GameRoster.cs:23-43`. Memoize id-string by instanceId (NOT Describe/Classify — CaptureStrength is mutable).

### Wave E — player-facing legibility (SSOT)
- [E18] (v3, HIGH) **Feed bark "AI: CAP" collides with map/HUD "CAP" capture tag** — `CommanderBrain.cs:300`. → "AI: air patrol ".
- [E19] (v3, MED) Panel shows raw PascalCase enum, not `ObjectiveVisuals.Name` — `CommanderPanel.cs:426/443/466/780`.
- [E20] (v3, MED) CombatPhase shown as raw enum ("Sead","AirSuperiority") everywhere — add `ObjectiveVisuals.PhaseLabel(CombatPhase)`; use in FlightHud/MapOverlay/CommanderPanel.
- [E21] (v3, MED) BUY affordability ignores queued spend (contradicts "After" warning) — `CommanderPanel.cs:631`. Net of `hq.QueuedCost`.
- [E22] (v3, MED) ASSIGN list comment promises RELEASE that doesn't exist — `CommanderPanel.cs:476/497`. Min: fix comment (b).
- [E29] (v2, LOW) `SquadStatus.Forming` unreachable in derived view — `SquadRoster.cs:59-65`. Make reachable (target>0 && Strength<target) or drop from docs.

### Wave F — test coverage (pure logic)
- [F3]  (v4, MED) Recon op auto-completion branch untested — +BrainRepertoireTests `Recon_op_completes_only_after_all_contacts_become_accurate`.
- [F23] (v3, HIGH) Brain FAILED ("lost the force") path untested — +BrainTests (assert Log Blocked + Operations/Squads empty after prune; do NOT assert Operations[0]).
- [F24] (v3, MED) SquadRoster Depleted transition untested — +SquadTests (player squad, TargetComposition.Total=4; strict '<' boundary at Strength==2).
- [F25] (v3, MED) "Defense funded first at tight cap" untested — +BrainRepertoireTests (MaxAutoObjectives=1, home raider + far offense → single DefendArea).

### Wave G — public API quality (SDK polish)
- [G11] (v3, HIGH) Mod-contract DTOs use public mutable fields — `ModData.cs`. → `{get;init;}` + fail-fast guard in ModRegistry.Add (do NOT add required ctor).
- [G12] (v3, MED) Vec3/ColorRgba lack value equality — add IEquatable/==/Equals/GetHashCode (deterministic FNV mix over SingleToInt32Bits, NOT HashCode.Combine) + ColorRgba.ToString.
- [G13] (v3, MED) Package ids (Nucleus.Domain) diverge from namespace (Nucleus.Core.*) — at min fix the FALSE "assembly==namespace" comment in Nucleus.Domain.csproj. (Namespace rename = broad; defer.)
- [G14] (v3, MED) Libs build `<Nullable>disable</Nullable>` — set `annotations` in libs/Directory.Build.props + `?` on nullable public members. (Keep build warning-clean.)
- [G27] (v2, LOW) WarScore ctor: 5 positional floats, no validation — add `ArgumentOutOfRangeException` guards (>=0); +tests. (Skip options-record.)

## Status log
- **A1 DONE** (local) — re-task on move/retype. De-dup in CommanderBrain.Tick step 5 now keys on a deterministic
  TaskSignature (Fnv1a over id+verb+position-bits+targetId), not the bare objective id. Extracted AddAutoObjective
  (DRY). +BrainTests.Tick_re_tasks_units_when_their_objective_moves. Core 140 + Sim 41 + arch 9 PASS, 0 warn.
- **A4 DEFERRED** — "home-defense exempt from the offensive cap" BROKE the self-play canary (8 Sim tests →
  "brain went inactive", TasksTotal=0). Root cause: HomeBase is the friendly CENTROID (moves every tick) in BOTH
  the sim AND the real game (CommanderService:97), so an uncapped, position-deduped DefendArea is re-spawned/chased
  against a moving home and dominates squad assignment. The determinism canary correctly caught a real emergent
  regression. A4 needs a different design (fixed home anchor, or reserve exactly one cap slot + better dedup) before
  it's safe — NOT shipping the naive "exempt" version. Revisit in a later pass.
- **E18 DONE** — ControlAirspace feed bark "AI: CAP …" → "AI: air patrol …" (the "CAP" token also means
  CapturePoint on the map/HUD; collision removed; matches the plain-verb sibling barks).
- **E19 DONE** — CommanderPanel objective/op rows + drop-hint + editor now render ObjectiveVisuals.Name(kind)
  (no more raw "DestroyTarget"/"ControlAirspace" PascalCase; panel agrees with map/HUD).
- **E20 DONE** — added ObjectiveVisuals.PhaseLabel(CombatPhase) SSOT ("SEAD"/"Air superiority"/"Scouting"…);
  adopted in CommanderPanel (rows/editor/op-row), FlightHud, MapOverlay selected-info. (No headless unit test —
  PhaseLabel lives in the Unity-referencing Ui assembly; trivial pure switch, compile-verified.)
  Verified: full no-deploy build 0 warn + Sim 41 + Core 140 (bark is pure Campaign; no test asserted the string).
- **B5 DONE** — CampaignSave.Deserialize: added bounds-checked PS(f,i) for direct string reads + min-column
  guards on every known record's key fields, so a truncated/older known record is skipped (not IndexOutOfRange
  that aborts the whole load). +CampaignStoreTests.Load_tolerates_truncated_known_records_without_throwing.
- **B6 DONE** — crash-safe save: CampaignStore.Save + WarfareSave.Save now use File.Replace (atomic on NTFS) when
  the destination exists, else File.Move — removing the no-file window of Delete-then-Move. Doc comment corrected.
  Covered by the existing Overwriting_an_existing_save_keeps_the_latest test. Core 141 PASS, 0 warn.
- **D2 DONE** — WarfareMod.Tick now throttles the HQ panel + scoreboard render to ~7Hz (RenderInterval 0.14f)
  like CommanderRuntime, instead of rebuilding the full LINQ HqSnapshot every frame.
- **D16 DONE** — CommanderService caches the unit-id→Role map (_roleMap), rebuilt only when the roster changes
  (via SetRoster, routing all 3 assignment sites) instead of allocating a ~400-entry dict on every AutoHq render.
- **D17 DONE** — CommanderRuntime.PositionsById caches its dict keyed on ReferenceEquals(LastRoster); rebuilt only
  when the throttled tick swaps in a new roster list. Full build 0 warn + Core 141 + Sim 41 + arch 9 PASS.
- D15 (HqView.Build micro-allocs) + D28 (GameRoster id-string) — DEFERRED to a later pass (lower value; D15 has a
  MemberUnitIds-aliasing caveat to handle carefully).
