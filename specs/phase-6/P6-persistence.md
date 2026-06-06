# P6 — Campaign persistence (save / resume)

**North-star bullet:** "the war is persistent/long-lived (save/resume a multi-hour campaign)."

## Goal
Give one faction's `CommanderState` a lossless, deterministic save/resume seam so a multi-hour
Dynamic Warfare campaign can be written to disk and continued exactly. Pure (engine-free), so it lives
in `Nucleus.Campaign`, stays under the architecture rules (no Unity), and is provable headlessly.

## Scope — what is persistent campaign state
Captured: top-level (`Autonomy`, `HomeBase`, the operation-id counter, the squad auto-form batch
counter), tunables (`Doctrine`, `BrainConfig`, `SquadConfig`), `Objectives`, `Squads` (id/name/family/
origin/status/autonomy/assigned-op/target-composition/members), `Operations` (id/objective/squad-ids/
autonomy/status/phase/combat-phase/order-id + the baseline `InitialThreat`), `ConfirmedObjectives`,
`LastObjectiveByUnit`.

NOT captured (transient, re-derived from the live game each tick): fog-of-war `KnownEnemies`, the live
`Roster`, per-tick `Proposals`/`ProductionNeeds` (rebuilt every tick), the `BattleLog` ring buffer
(a UI feed, not decision state).

## Design
- `Persistence/CampaignSnapshot.cs` — a pure value snapshot holding the captured state above (reusing the
  domain types directly: `Objective`/`Squad`/`Operation`/`EnemyView`, plus scalars).
- `Persistence/CampaignState.cs` — `Capture(CommanderState) → CampaignSnapshot` and
  `Restore(CampaignSnapshot) → CommanderState`. Reconstructs operations' objectives by id (and folds in
  any op objective missing from the list so the snapshot is self-contained).
- `Persistence/CampaignSave.cs` — dependency-free, versioned, line/record text format
  (`Serialize(snapshot) → string`, `Deserialize(string) → snapshot`). No NuGet/JSON dependency, so it is
  Mono/BepInEx-safe inside the game and keeps the pure lib leaf-clean. Invariant-culture, round-trippable
  floats (`"R"`).
- Two minimal pure accessors so the serializer can read/restore the counters that drive deterministic id
  generation: `CommanderState.OperationIdSeed` (internal — same assembly) and `SquadRoster.BatchSeed`
  (public — different assembly).

## Acceptance criteria (all headless, in the gate)
1. **Lossless round-trip (object model):** capture → restore yields a `CommanderState` equal field-by-field
   (objectives, squads incl. composition+members, operations incl. phase+initial-threat counts, autonomy,
   home base, confirmed set, last-objective map, all counters).
2. **Lossless round-trip (string):** `Deserialize(Serialize(snapshot))` equals `snapshot`.
3. **Continuation determinism:** run the real brain N ticks → save → deserialize → restore → continue M
   ticks; an unsaved control continues the same M ticks. The two resulting traces are identical
   (`DualSimWorld`-style fingerprint match). Proves resume changes nothing.
4. **Id-counter continuity:** after restore, `NextOperationId()` and the next auto-formed squad batch do
   not collide with restored ids.
5. **Format robustness:** version header present; tabs/newlines inside strings (names) survive; an empty
   campaign round-trips; unknown trailing record types are ignored (forward-compat).

## Green-gate commands
- `dotnet build Nucleus.sln -c Release -p:TreatWarningsAsErrors=true`
- `pwsh scripts/audit.ps1` → AUDIT: PASS (new persistence tests under unit-core + a continuation test under sim)
