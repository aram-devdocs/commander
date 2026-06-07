# Codegenned native-UI library — "compose the game's UI, don't rebuild it"

Goal (user): place/use the GAME's own UI elements to build the mod UI, so the mod IS native, not an imitation.

## How it works (3 layers)
1. **Codegen guard (the "codegenned" part).** `tools/Nucleus.CodeGen` already lists the native UI widget types
   in its manifest (`Program.cs` P6 block): `NuclearOption.UI.BetterBorder`, `BaseToggle` (isOn / onValueChanged /
   SetIsOnWithoutNotify), `BoxToggle`, `SliderToggle`, `BetterToggleGroup` (Set/GetIndex/Flags). The generated
   `GameContract` test asserts every one still exists with the expected shape. **A game update that renames a
   widget fails the contract test (regenerate → compiler/CI points at exactly what drifted)** — the mod never
   silently breaks. The game's fonts/sprites/colors are likewise captured once into the generated `NativeAssets`.
2. **Runtime harvest factory** (`libs/Nucleus.Ui/Native/NativeUi.cs`). The game bakes its UI into scenes (no
   reusable widget prefabs on `GameAssets`), so the robust idiom — proven by the bezel buttons and the live
   `BetterBorder` frame — is: find a live instance (`Resources.FindObjectsOfTypeAll<T>()` filtered to
   `gameObject.scene.IsValid()`), `Instantiate` it, and rewire its events (disable the prefab's persistent
   listeners via `SetPersistentListenerState(Off)` + `RemoveAllListeners`, then add ours). `NativeUi` now offers:
   - `Border(panel, accent)` — the game's `BetterBorder` (already live).
   - `Toggle(parent, label, isOn, onChanged)` — clones the game's `BoxToggle` (native look + animation).
   - `Button(parent, label, onClick)` — clones a live sliced-sprite `Button` (the game's chrome).
   Each returns null when no live template exists (e.g. headless) so callers fall back to a built atom — safe.
3. **Composition.** Panels build from `NativeUi.*` (native) with `UiFactory.*` as the styled fallback/layout glue.
   Layout groups, dividers, scroll plumbing stay uGUI (the game doesn't expose those as widgets); the *controls*
   (button/toggle/border) and *assets* (font/sprites/colors) are native.

## Why not pure prefab-instantiation?
`GameAssets` exposes screen prefabs (settings menu, tac screen…) but NOT reusable widget prefabs. So harvesting a
live instance is the only general path. It's the same pattern the mod already relies on (bezel clone, button
sprite, border) — now generalized and contract-guarded.

## Verified now (headless, no game launch)
- `Nucleus.Ui` compiles with the new factory; `BaseToggle.onValueChanged` confirmed assignable to `UnityEvent<bool>`.
- GameContract test PASS (11/11) — the native widget types exist in the shipped assembly.

## Deferred (needs the game / a screenshot)
- Does a cloned `BoxToggle`/`Button` render + behave correctly when re-parented into our MFD panel (no NRE from a
  missing manager; label/animation intact)? — verify with `visual-probe.ps1` when the user is free.
- Adopt `NativeUi.Toggle` for the COMMANDER (AI COMMANDER / AI AUTO-FILL) toggles and `NativeUi.Button` for the
  panel buttons, with the built atoms as fallback. Then migrate the rest section-by-section (strangler).

## Risks (from the audit)
- Some widgets are entangled (need a parent canvas/manager) — `MFDScreen` (handled via VirtualMFD), `ScrollRect`
  (clone the whole hierarchy), and singleton-wired widgets (`HUDOptions` toggle) will NRE if detached → only
  harvest the standalone ones (Button/BoxToggle/SliderToggle/BetterToggleGroup/BetterBorder).
