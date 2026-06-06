# STATUS — Nucleus build ledger

> Machine-readable progress ledger. The autonomous loop reads this FIRST every wake to find the next
> action. Gate codes: ① spec ② test ③ review ④ playtest. State: TODO / WIP / GATE-n / DONE / BLOCKED.
> Update this on every state transition. Source of truth for "what's next" — survives context compaction.

**Branch:** `nucleus-platform` · **Baseline (known-good):** build 0 warnings · 118 Core · 11 GameContract (2026-06-06)
**Current phase:** Phase 2 — Extract GameSdk + Ui + retarget codegen (Phase 1 COMPLETE: all 4 pure libs extracted, src/Core gone)
**Next action:** P2-gamesdk — extract `libs/Nucleus.GameSdk` from src/Game/** (minus CommanderService, which is Commander-specific → stays for Phase 3). Move codegen gameGenDir → the lib; retarget GameSdk/NativeAssets generated paths. Then P2-ui (generic widgets → libs/Nucleus.Ui). Guard: GameContract + arch. NOTE: GameSdk/Ui touch Unity → import build/GameReferences.props; arch test allows them Unity refs (they're not pure libs).
**Gate now:** `pwsh scripts/audit.ps1` → AUDIT: PASS (build 0w · unit-core 118 · arch 9 [Domain/Squads/Production/Campaign all non-vacuous, full DAG] · contract 11)

## Phase status
| Phase | Title | State | Notes |
|-------|-------|-------|-------|
| 0 | Tooling & ledger foundation | DONE | sln+props+build helpers, arch test (9, synthetic-proven), gate scripts, active hooks, warnings-clean, CI headless gate. TestKit/Sim/coverage/api-snap sequenced into P1; LogAudit→pre-P3 |
| 1 | Extract pure libs (Domain/Squads/Production/Campaign) | TODO | namespaces frozen at CommanderLayer.* |
| 2 | Extract GameSdk + Ui + retarget codegen | TODO | |
| 3 | Stand up host; Commander first | TODO | riskiest — Canvas/tick ownership inversion |
| 4 | Split Build | TODO | one host queue, no double-buy |
| 5 | Split Squad | TODO | external SquadRoster ctor |
| 6 | Warfare + SDK packaging + dual-faction + persistence | TODO | north-star |
| 7 | Rename pass (CommanderLayer.*→Nucleus.*, repo+folder no_nucleus) | TODO | human folder-rename touchpoint |

## Work-items in flight
| ID | Phase | Item | Gate | Owner | Last gate result | Next action |
|----|-------|------|------|-------|------------------|-------------|
| P2-gamesdk | 2 | extract Nucleus.GameSdk | — | loop | next | move src/Game/** (minus CommanderService), retarget codegen, gate |

## Pending playtests (Unity-gated, awaiting human)
_(none yet)_

## Gates / commands
- Fast: `pwsh scripts/check.ps1` (build + changed-project unit + arch)
- Full: `pwsh scripts/audit.ps1` (everything → PASS/FAIL dashboard + artifacts/audit-summary.json)
- Baseline today (pre-monorepo): `dotnet build src/CommanderLayer.csproj -c Release` + `dotnet test tests/Core` + `dotnet test tests/GameContract`
