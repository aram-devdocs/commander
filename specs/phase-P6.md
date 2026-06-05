# Phase P6 — Native UI component library (the EXACT game UI), codegen-driven

Status: Spec drafted → self-reviewed → **P6.1 in progress**. Plan ref: `~/.claude/plans/composed-fluttering-crescent.md` (P6).

## Goal
Stop hand-rolling visual values and uGUI atoms. The Commander UI should read as the game's own —
same fonts, colors, icons, borders, toggles — because it *uses the game's own assets and components*,
codegen'd from the assembly so it's 1:1 and regenerates on game updates. **DRY, single source of truth,
no skew:** any value that exists in the game lives once (in the game) and is referenced/mirrored, never
copied. Mod-owned values (panel chrome, per-order distinct colors) are clearly marked as such.

## Player feels
The Commander UI is indistinguishable from the game's own — native font, HUD friend/hostile/neutral
colors, real icons, native border/toggles — not a look-alike.

## Split into verifiable slices
The *rendering/behavior* of cloned native components is **playtest-gated** (I can't run the game). The
**asset single-source-of-truth + type contracts** are fully buildable/verifiable now. So:

### P6.1 — Visual-asset SDK (single source of truth). *Buildable + verifiable now.*
- **Codegen** a typed, drift-guarded snapshot of `GameAssets` visual resources into the SDK:
  `tools/CommanderLayer.CodeGen` gains an `Asset` manifest tag; emits
  `src/Game/Generated/NativeAssets.generated.cs` — a `NativeAssets` class with one typed `readonly`
  field per asset (font, HUD colors incl. Neutral, map/threat icons) + `Capture()` from `GameAssets.i`.
  Cecil discovers each field's real type; the contract test asserts every asset member still exists
  (drift → red). Game update → regenerate → compiler/CI point at exactly what changed.
- **Single capture point:** the composition root captures `NativeAssets` once and pushes into the
  Ui-layer caches (`NativeColors`, `UiFactory.Font`, new `NativeIcons`). No more scattered
  `GameAssets.i` reads.
- **Re-base colors onto native source:** Ui reads friend/hostile from the captured snapshot **now**
  (`MapOverlay`); `NativeColors` also gains `Neutral` and `NativeIcons` holds the map sprites, both
  **captured ready for the P6.2 overlay render** (no consumer yet — same deferral as the icons). `Theme`
  panel chrome + `OrderColors` per-order hues stay **mod-owned, explicitly commented** (no game
  equivalent) — not copies of game values. Manifest discipline: only assets the mod uses (now or in the
  committed P6.2 overlay set) are listed — no speculative `*Selected`/`AirbaseNotAvailable`.
- **Files:** `tools/CommanderLayer.CodeGen/Program.cs` (manifest+emitter), generated
  `src/Game/Generated/NativeAssets.generated.cs`, `tests/GameContract/GameContract.Generated.cs`
  (regenerated), `src/Ui/NativeColors.cs`, `src/Ui/NativeIcons.cs` (new), `src/Composition/CommanderRuntime.cs`
  (capture site).
- **Acceptance:** clean Release build; Core + GameContract green (contract now guards every asset);
  regenerated files match committed; no hardcoded game color/font literals remain in the Ui path
  (mod-owned ones labeled).

### P6.2 — Native component wrappers + runtime harvest. *Type-contract buildable; render playtest-gated.*
- Add `NuclearOption.UI.{BetterBorder, BoxToggle, SliderToggle, BetterToggleGroup}` members to the
  manifest (verified by Cecil + contract). Generate typed wrappers where useful.
- `src/Ui/Native/NativeUi` harvest layer: locate live prefab/instance via `Resources.FindObjectsOfTypeAll<T>()`,
  `Object.Instantiate` a clone, strip/rebind handlers. **Graceful fallback** to the current hand-rolled
  atom when a native source isn't found (never hard-fails / blanks).
- Re-base `UiFactory.Button/Toggle/Border` onto `NativeUi` clones behind that fallback.
- A `CommanderDebug` UI-harvest probe logs which native components were found/cloneable in one playtest.
- **Acceptance:** playtest — buttons/toggles/border visually native; no blank panel; screenshot diff.

## Principles check (self spec-review)
1. **Autonomy ladder** — UI only; no tasking path touched. ✔
2. **Pure brain / Unity-free Core** — `NativeAssets.generated.cs` lives in `src/Game/Generated` (names
   `GameAssets`); **never** in Core. Core/Generated stays game-DLL-free. ✔
3. **No stampede** — no executor/objective path touched. ✔
4. **Do-nothing-safe** — `Capture()` returns null if `GameAssets` unloaded; Ui keeps fallbacks; headless
   tests unaffected. ✔
5. **Quality gate** — codegen verified vs real assembly; contract test regenerated; build clean. ✔

## Risks
- Asset field drift on game update → caught by contract test (that's the point). ✔
- Icons captured but not yet consumed by the overlay (P6.2) → exposed via `NativeIcons`, marked
  "ready for overlay"; not dead code, it's the single-source surface the overlay will read. Acceptable.
- P6.2 cloning of native MonoBehaviours may carry game handlers → strip/rebind + fallback; playtest-gated.

## Self-review verdict
P6.1 is sound, scoped, verifiable without the game, and delivers the literal DRY/single-source ask.
P6.2's buildable part (type contracts) is safe; its render is correctly deferred to playtest. Proceed P6.1.
