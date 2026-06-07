# DX / architecture initiatives (user directives — June 7)

Two standing directives beyond the review passes, plus "fix all deferred, no deferring":

> "dont defer anything? just fix it all?"
> "lint for architecture and design and code patterns either with linter or custom validator scripts,
>  same for dead code or other dx, atomic design. also, lets make our UI completely stateless"

Headless/local-only while the user games. Each item ships as a gated local commit (build 0/0 + Core/Sim/Arch
+ touched suite; Sim is the determinism canary — never weaken it). "No deferring" = solve each correctly, NOT
ship a naive version that breaks a canary.

## A. Finish the deferred review items (no defer)
- [x] D15 HqView allocations — DONE (1f91370).
- [x] D28 GameRoster id-string cache — DONE (1f91370).
- [ ] **A4 home-defense vs cap** — REDESIGN (the naive "exempt from cap" broke self-play because HomeBase is a
  moving centroid, spawning/chasing many DefendAreas). Correct design: maintain AT MOST ONE auto home-defense
  objective — if home is threatened, MOVE the existing one to the current home (don't spawn a new one per
  centroid shift); create it once (reserving/evicting within the cap so total stays bounded); let it prune when
  the threat clears. Must pass the Sim canary (TasksTotal>0, war progresses, determinism). If a correct design
  still can't pass, that's a real signal — keep iterating the design, don't weaken the canary.
- [ ] **G14 nullable** — do the FULL accurate pass: <Nullable>annotations</Nullable> in libs/Directory.Build.props
  + annotate the ENTIRE public reference surface of each lib accurately (`?` where genuinely nullable, leave
  non-null otherwise). Build must stay warning-clean. Audit every public member — partial = misleading.

## B. Architecture / design / dead-code / DX validators (custom, automated)
Extend the enforcement net (currently: acyclic DAG + Unity-free purity in Nucleus.Architecture.Tests, Cecil-based)
with more custom validators so patterns are enforced by tests, not review:
- **App-thinness validator**: assert apps/* types contain no business logic (heuristic: no LINQ-heavy methods /
  no domain decision branching) — or at minimum assert apps only orchestrate (depend on libs, don't re-implement).
- **Dead-code validator**: Cecil scan for public/internal members with zero references across the solution
  (beyond the TreatWarningsAsErrors unused-private check) → fail or report. Also a `scripts/deadcode.ps1` report.
- **Design-pattern validators**: SSOT guards (e.g. no second per-ObjectiveKind color/label switch outside
  ObjectiveVisuals; no `string.GetHashCode()`/`HashCode.Combine` anywhere — determinism lint; no `DateTime.Now`/
  `Math.Random`/`UnityEngine.Random` in pure libs); naming conventions.
- **UI-statelessness validator** (pairs with D): assert UI classes hold no business/domain state (only widget
  handles + theme), so the stateless contract can't silently regress.
Wire any new suite into pre-commit/check.sh/check.ps1/CI (the gate-parity rule).

## C. Atomic design for the UI
Organize the uGUI kit into the atomic hierarchy and document it:
- **Atoms**: UiFactory primitives (Label, Button, Panel, Ring, LineImage, Divider, scrollbar, tokens).
- **Molecules**: composite rows (BuildRow, EntityRow), SectionHeader, the AI/YOU toggle, kind buttons.
- **Organisms**: CommanderPanel sections (Objectives, Operations, Squads, Build, Feed, Scoreboard), MapOverlay, FlightHud.
- **Templates/Pages**: the per-mod MFD screens (CMD/BLD/SQD/WAR).
Likely a docs/dev/UI-ATOMIC.md taxonomy + light refactor so files/namespaces reflect the layers. Pairs with D.

## D. Completely stateless UI (this is WS4 — the long-pending Pure Presentation layer)
The UI should be a pure function of an immutable view-model; the native shell only RECONCILES widgets (it may
hold widget handles for retained-mode uGUI — rebuilding every frame is the perf trap the throttle fought — but
NO business/campaign/selection/armed state lives in the UI classes).
- New pure lib **Nucleus.Presentation** (Unity-free, arch-test-guarded): consumes HqSnapshot/Scoreboard/ConvoyCatalog
  + UI input state → immutable VM records (PanelVM/SectionVM/RowVM/MarkerVM/HudVM). This is "what to show",
  unit-testable for the first time. Add Presentation → {Domain,Campaign,Production} to the DAG allow-list.
- Move the panel's remaining UI STATE (_armedObjective, _selectedObjectiveId, scroll, toggle bits) OUT of
  CommanderPanel into the VM/runtime, so the panel renders purely from (VM) and raises events. Strangler-migrate
  section by section behind the VM; the panel stays working throughout.
- The UI-statelessness validator (B) then guards it.
APPROACH: this is large and design-sensitive — run a competing-agent DESIGN workflow first (N approaches → judge →
synthesized plan in docs/dev/STATELESS-UI-PLAN.md), then implement section-by-section, gated. Verify headless
(VM unit tests); the visual equivalence is a PT item for when the user is done gaming.

## Sequencing (loop agenda)
1. A4 redesign, G14 full — finish "no defer". 2. B validators (high-leverage, headless). 3. Pass-2 review triage
(workflow w60zd10pb) interleaved. 4. C+D: design workflow → stateless-UI/atomic implementation, section by section.
Keep iterating; stop a sub-item only if it would break a canary (then redesign, don't weaken).
