# Spec — P1-domain: extract Nucleus.Domain (pure leaf library)

**North-star ref:** Platform/SDK foundation — the shared domain vocabulary every mod + the SDK reference.
**Phase:** 1 (Extract pure libs). First of four ordered extractions: Domain → Squads → Production → Campaign.
**Headless-verifiable:** yes.

## Goal
Move the pure, Unity-free *base vocabulary* out of `src/Core` into a new `libs/Nucleus.Domain`
(netstandard2.1, assembly name `Nucleus.Domain`). The monolith `src/CommanderLayer.csproj` and
`tests/Core` reference it via `ProjectReference`. **Namespaces stay `CommanderLayer.*`** (folder/assembly ≠
namespace; the rename is Phase 7). This proves the lib-extraction mechanics and stands up the first node of
the architecture DAG that `Nucleus.Architecture.Tests` enforces.

## Scope IN (the Domain leaf — depends on nothing but itself)
Candidate set (final assignment confirmed by the dependency analysis + the compiler — anything that fails to
compile in a no-Unity lib does not belong here):
- `Core/Model/*` — Vec3, ColorRgba, FactionInfo, Domain, Orders, Views, Roles, AssignmentPreview
- `Core/Command/RoleFamily.cs`, `Composition.cs`, `AutonomyLevel.cs` (cross-cutting primitives used by
  Squads/Production/Campaign — must sit at the leaf so those libs stay independent)
- `Core/Roles/RoleClassifier.cs`
- `Core/Ports/*` (IPlayerContext, IMapProjection, CommanderPorts — pure interfaces)
- `Core/Generated/*` (GameEnums, GameRef — pure mirrors; codegen output path retargets in Phase 2)

## Scope OUT
- Squad/Production/Campaign/Planning types (later P1 sub-steps).
- Any namespace rename. Any behavior change. Any Unity reference.
- Moving codegen *output paths* (that is Phase 2; for now Generated files physically move but the codegen
  tool still writes the old `src/Core/Generated` path — reconciled in Phase 2. If that double-writes, keep
  Generated in place for P1-domain and move it in Phase 2 instead. Decide during execution.)

## Approach
1. Dependency analysis: confirm the candidate set is closed under reference (no member references a type
   outside the set or Unity). Produce the exact file list.
2. Create `libs/Nucleus.Domain/Nucleus.Domain.csproj` (netstandard2.1, `<AssemblyName>Nucleus.Domain</AssemblyName>`,
   no game refs; imports nothing from `build/GameReferences.props`). Add to `Nucleus.sln`.
3. `git mv` the files from `src/Core/...` into `libs/Nucleus.Domain/...` (physical move — never copy).
4. `src/CommanderLayer.csproj`: add `<ProjectReference Include="..\libs\Nucleus.Domain\Nucleus.Domain.csproj" />`;
   its default-compile glob no longer sees the moved files (they left `src/`).
5. `tests/Core/CommanderLayer.Tests.csproj`: keep the `Compile Include="..\..\src\Core\**"` for the remaining
   Core files, AND add a `ProjectReference` to `Nucleus.Domain` for the moved ones. (When all of Core is
   extracted in later sub-steps, the glob goes away entirely.)
6. Build + test; fix any file that fails to compile in the no-Unity lib by leaving it in `src/Core` (it then
   belongs to a higher lib, not Domain).

## Acceptance criteria
- `libs/Nucleus.Domain` builds as netstandard2.1 with **no** reference to UnityEngine/Assembly-CSharp/BepInEx/Mirage.
- The full solution builds 0 warnings under `-p:TreatWarningsAsErrors=true`.
- `tests/Core` still passes **118** (no test lost; the glob-or-projectref split covers all Core sources once).
- `Nucleus.Architecture.Tests`: `Nucleus.Domain` now present, `Pure_libs_are_unity_and_game_free` and the
  DAG facts pass non-vacuously for it (Domain has zero Nucleus refs).
- `tests/GameContract` still **11** (plugin still builds + behaves identically).
- Plugin still deploys to `.sandbox` unchanged.

## Green gate (definition of done)
`pwsh scripts/audit.ps1` → `AUDIT: PASS` with steps: build(0w) · unit-core(118) · arch(≥9, Domain non-vacuous) · contract(11).

## Runtime checks
None new (no behavior change). Existing in-game behavior unaffected — defer any playtest; this is purely structural.

## Risks / open decisions
- **tests/Core glob vs ProjectReference overlap** — a file must not be both glob-compiled AND in the referenced
  lib (duplicate type). Mitigation: moved files leave `src/Core`, so the glob can't see them; the ref supplies them.
- **Codegen Generated path** — if moving `Core/Generated` now double-writes on next codegen run, leave Generated
  in `src/Core` for P1-domain and relocate in Phase 2 (logged above).
- **Closure correctness** — the compiler is the oracle: if Domain references something outside the set, it won't
  build, and that file belongs to a higher lib.

## Estimated commits: 1–3
