# ADR-0036: Pool class-based Domain events per run

- **Status:** Accepted
- **Date:** 2026-10-28
- **Authors:** architect-role
- **Reviewers:** user (approved 2026-10-28 on task `c5`)
- **Related:** amends [ADR-0035](ADR-0035-reusable-frame-buffers.md) (points 5 and 8); [ADR-0005](ADR-0005-domain-events-and-application-notifications.md), [ADR-0020](ADR-0020-presentation-domain-read-model-values.md), [ADR-0027](ADR-0027-typed-result-dispatcher.md), [ADR-0033](ADR-0033-performance-thresholds-for-cut-over.md) (metric 3), [ADR-0034](ADR-0034-automated-performance-measurement.md)

## Context

ADR-0035 makes the Arena tick path allocate nothing by reusing preallocated instances. Its Decision point 5 and requirement 8 assume that Domain events can be pooled as reusable *instances*. All 16 implementers of `IArenaDomainEvent` are `public readonly struct`, so every append to the aggregate's `List<IArenaDomainEvent>` boxes the struct and allocates a heap object; a boxed readonly struct is immutable and cannot be refilled. ADR-0035 requirement 8 and the handoff's Acceptance 6 ("no Domain event type changed") therefore cannot both hold, and no code was written: `coder-role` deferred the task as `c5`.

Two further facts constrain the fix. First, the event set is closed and small: `ArenaRunDefeated`, `ArenaRunVictorious`, `EnemyAttackCancelled`, `EnemyAttackCompleted`, `EnemyAttackStarted`, `EnemyDamaged`, `EnemyDefeated`, `EnemySpawned`, `PlayerAttackCancelled`, `PlayerAttackCompleted`, `PlayerAttackStarted`, `PlayerDamaged`, `PlayerDefeated`, `WaveCompleted`, `WavePhaseChanged`, `WaveStarted`. Second, ADR-0035 point 2 grants a two-tick window to everything reachable from a published result, including domain events, while requirement 8 releases events "at the start of the following tick" — a one-tick window. The amendment must resolve that inconsistency too.

## Decision

We will convert the 16 Domain events from `readonly struct` to `sealed class` and pool them per run, because pooling is the chosen zero-allocation mechanism and reference types are the only representation it can refill.

The contract:

1. **Shape.** Each event keeps its name, namespace, public get-only properties, parameter list, constructor validation and `IEquatable<T>`/`Equals(object)`/`GetHashCode` value semantics. Only the type kind changes. No handler signature, mapper, dispatcher or test assertion changes.
2. **Fill path.** Each event gains an internal `Fill(...)` (assigns all properties) and an internal `Reset()`. `Fill` performs no validation: it is reached only from the aggregate after the same validation the constructor performs has already accepted the values. Constructors remain the public construction path and keep validating as today.
3. **Ownership.** `ArenaRun` owns a two-slot per-run event pool. It already receives the `WaveCatalog`; each pool's capacity is derived from the largest catalogue wave, never a literal — enemy-related event types are sized to the largest wave, run/player/wave-singleton types to one. A new public `ArenaRun.BeginTick()` toggles the slot and releases the events of the slot being reused.
4. **Lifetime.** An event filled at tick N stays untouched through tick N+1 and is released when its slot is next reused at tick N+2. This matches ADR-0035 point 2 and corrects requirement 8: events are released when their slot is reused, not at the start of the following tick.
5. **No list allocation.** The aggregate keeps one reused per-operation event list, cleared at the start of each operation, and `ArenaRunChange` stops copying its caller's list. A change is valid only until the next operation; the recorder drains it synchronously, so no consumer may retain a change across operations. Published results do not expose changes.
6. **Wiring.** `ArenaRunTickCoordinator.ExecuteTick()` calls `run.BeginTick()` immediately after obtaining the active run, before the recovery stage. A restarted run starts with a fresh pool; calling `BeginTick` on it is harmless.
7. **Out of scope.** Notifications (already classes; pooled by ADR-0035), the struct read models and interaction DTOs, the dispatcher, and every other ADR-0035 point.

ADR-0035 point 1 said the Application frame buffers own "the event and change pools"; only the event pool moves to the Domain aggregate, because the aggregate is the only producer of events and no layering change is acceptable. The Application buffers keep every type they construct.

## Options considered

### Option A — Convert events to sealed classes and pool them (chosen)
- Pros: keeps `IArenaDomainEvent`, every consumer and the typed event pipeline; value equality and constructors survive, so tests keep their semantics; no new dependency or port; no boxing; smallest diff that reaches 0 B.
- Cons: changes the kind of 16 public types; `Fill` bypasses constructor validation by design; a null event becomes representable in principle (the aggregate never produces one, and `ArenaRunChange` still rejects null).
- Why chosen: it is the only route that satisfies both the 0 B gate and the typed event contract without rewriting ADR-0005 or the dispatcher.

### Option B — Closed struct union with generic dispatch
- Pros: no aliasing; value semantics throughout.
- Cons: rewrites ADR-0005; changes the recorder, mapper, dispatcher and test surface; largest diff in the repository.
- Why not chosen: ADR-0035 Option B already rejected it; the representation change in Option A is far smaller and preserves every consumer.

### Option C — Keep structs and relax ADR-0033 metric 3
- Pros: no type change.
- Cons: concedes the 0 B gate; the regression test would have to be relaxed; ADR-0033 deliberately made the metric hard.
- Why not chosen: it abandons the approved strategy instead of correcting its premise.

## Consequences

### Positive

- ADR-0035 becomes implementable as written for everything except the event representation; `t9-gc-allocation` unblocks.
- Zero boxing on the tick path; the two-tick rule for events is now consistent with the rest of the frame buffers.
- Value equality is preserved, so `Assert` comparisons, `List.Contains` and any future set/dictionary use keep working (none currently keyed on event types).

### Negative / trade-offs

- A public Domain type kind changes; downstream code that treats an event as a value type would break at compile time (none found: no `default(...)` use, no `==` operators, no event-keyed collections).
- `Fill` can produce an unvalidated instance inside the Domain; the invariant is that the aggregate validates before filling.
- A change object is valid for one operation rather than one tick; this is a new documented lifetime bound that a future consumer could violate.

### Neutral

- Event instances carry a class header and are pooled, so steady-state allocations are zero while peak memory grows by the pool size (kilobytes).
- Tests keep constructing events through their constructors and see the same equality behaviour.

## Compliance with paradigm

- The paradigm is unchanged: DDD/IDDD with Clean Architecture layering. No layer, assembly reference or dependency direction changes.
- ADR-0005's typed Domain events and ADR-0022's notification mapping stand; the types change representation, not semantics.
- ADR-0007 and ADR-0015 are preserved: the pool and the classes are plain C# in Domain, covered by the standalone core build.
- `docs/architect/paradigm.md` needs no exception entry.

## Migration plan

1. Convert the 16 events to `sealed class`; keep constructors, properties, equality; add internal `Fill`/`Reset`.
2. Add the Domain-internal two-slot event pool and `ArenaRun.BeginTick()`; reuse the per-operation event list; stop `ArenaRunChange` from copying.
3. Call `run.BeginTick()` at the top of `ArenaRunTickCoordinator.ExecuteTick()`.
4. Build; run the Domain, Application and PlayMode suites; extend the allocation-regression test to cover a multi-event attack frame.
5. Re-run the ADR-0034 measurement twice; metric 3 must read 0 B in both.

## References

- `docs/architect/coder-inbox/c5.md` — the deferral that triggered this amendment
- `docs/architect/tasks/c5.md`
- `Assets/_Project/Contexts/Arena/Domain/Events/`
- `Assets/_Project/Contexts/Arena/Domain/Aggregates/ArenaRun/ArenaRun.cs`, `ArenaRunChange.cs`
- `Assets/_Project/Contexts/Arena/Application/Ticks/ArenaRunTickCoordinator.cs`
