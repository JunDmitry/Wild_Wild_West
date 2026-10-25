# ADR-0033: Performance thresholds for the production cut-over

- **Status:** Accepted
- **Date:** 2026-10-25
- **Authors:** architect-role
- **Reviewers:** user (thresholds approved with the `t8-audit-t9-plan` plan on 2026-10-25)
- **Related ADRs:** [ADR-0007](ADR-0007-unity-isolation-and-layer-dependencies.md), [ADR-0015](ADR-0015-standalone-core-build.md), [ADR-0029](ADR-0029-scene-reload-through-application-port.md), [ADR-0030](ADR-0030-composition-root-scopes.md)

## Context

`docs/architect/migration/criteria-for-switching-production-flow.md` § Performance requires measurable thresholds for five metrics before T-9, and requires the target implementation not to exceed "agreed thresholds compared with legacy without an accepted performance ADR". No threshold exists anywhere in the repository, and no profiling session has been recorded.

T-9 cannot start while that criterion is undefined: an undefined gate is either skipped, which the migration criteria forbid, or invented after the fact to match whatever the first measurement returns. The project also has no profiling infrastructure, no reference hardware statement, and no automated frame-time capture, so the decision has to name a procedure that a person can repeat rather than promise continuous measurement.

`qa-role` is not operating yet, so the architect records this decision as an explicit exception.

## Decision

We will fix five measurable thresholds and one measurement procedure in this ADR, and require a single recorded measurement run at T-9 entry to prove them.

The metrics, thresholds, and sources:

| Metric | Threshold | Source |
|---|---|---|
| Average frame time | ≤ 16.7 ms over a 60 s steady-state sample | Unity Profiler, player build, wave-1 combat |
| Worst frame time | P99 ≤ 33.3 ms; no frame above 50 ms | Unity Profiler capture over the same sample |
| Managed allocation per frame | 0 B after a 300-frame warm-up | Unity Profiler GC Alloc per frame |
| Maximum active enemies | 24 | `ArenaRunSnapshot.Enemies.Count` peak, largest catalogue wave |
| Physics queries per frame | ≤ 5 | Unity Profiler `Physics.Query` counters |

Measurement procedure:

1. Build the Arena scene stack (bootstrap → arena) as a **player build**, not the editor PlayMode loop; editor frame time is not a cut-over measurement.
2. Run one 60 s session of wave-1 combat with scripted input (movement, one ranged attack per second, one melee attack every five seconds), at 1920×1080, default quality level.
3. Discard the first 5 s (warm-up), then capture the remaining 55 s for frames 1–3 and the peak for metrics 4–5.
4. Repeat once on a clean process; record both runs.
5. Compare the same scene content run on the legacy stack on the same machine; the target must not exceed legacy by more than 10% on any metric.

Consequences:

- **Modules affected:** none. This ADR changes no code, no registration, and no scene.
- **New dependencies:** none. Measurement uses Unity's built-in Profiler.
- **Out of scope:** automated performance regression tests in CI, per-platform thresholds, memory footprint beyond per-frame allocations, and the legacy measurements themselves.
- A threshold that the measurement exceeds is a T-9 blocker: the fix is either an optimisation or a new ADR that revises this one. It is not silently waived.
- A threshold this ADR gets demonstrably wrong for a platform reason is revised by a superseding ADR, never by editing a recorded result.

## Options considered

### Option A — fix the thresholds before measuring (chosen)
- Pros: the gate exists before the result, so a passing measurement means something; the procedure is repeatable by anyone with the project and a machine; no tooling investment.
- Cons: the numbers are reasoned defaults, not derived from this project's observations; the 10% legacy comparison depends on running the legacy scene, which is frozen but still runnable.

### Option B — measure first, then set thresholds from the observation
- Pros: thresholds would reflect measured reality.
- Cons: retro-fitted thresholds can never fail, so the criterion becomes decorative; the measurement needs a target of its own to be meaningful.

### Option C — no thresholds; monitor after cut-over
- Pros: nothing to define now.
- Cons: directly contradicts the migration criteria, and a regression found after cut-over is a rollback, not a gate.

## Consequences

### Positive

- T-9 has a concrete, repeatable entry gate.
- The five metrics map onto the architecture's own boundaries: frame time and allocations are the Application tick and dispatcher; enemy count is the Domain's largest wave; physics queries are Infrastructure's four adapters.
- No production code, scene, or assembly changes.

### Negative / trade-offs

- The thresholds are defaults reasoned from a 60 FPS budget, not from this project's profile; the first measurement may prove one of them too tight.
- A player build with scripted input is manual work; it is not automated.
- The 10% comparison requires a legacy run, which is possible only while the legacy scene still exists (before T-10).

### Neutral

- The measurement is a T-9 entry task, not a recurring one.
- If the legacy stack cannot be measured for platform reasons, the target-vs-legacy clause is recorded as not applicable with a reason, and the absolute thresholds stand on their own.

## Compliance with paradigm

- No deviation. This ADR adds no type, no dependency, and no layering change; it constrains T-9's entry evidence.
- ADR-0007 is preserved: the metrics are observed at Unity boundaries (Profiler, physics counters) and never inside Domain or Application.
- ADR-0015 is preserved: the standalone core build is unaffected.

## Migration plan

1. Record this ADR (this file) and reference it from `docs/coder/delivery-plan.md` § T-9.
2. At T-9 entry, perform the measurement run described above and record the numbers with the build identifier.
3. Compare against the legacy stack, or record the not-applicable reason.
4. T-9 proceeds only when every metric is within threshold; otherwise the difference becomes a task before cut-over.

## References

- `docs/architect/migration/criteria-for-switching-production-flow.md` § Performance
- `docs/coder/delivery-plan.md` § T-9
- `docs/architect/tasks/t8-audit-t9-plan.md`
- `Assets/_Project/Contexts/Arena/Application/Sessions/ArenaRunSession.cs`
- `Assets/_Project/Contexts/Arena/Infrastructure/` (physics query sites)
