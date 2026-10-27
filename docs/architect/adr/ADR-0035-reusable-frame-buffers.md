# ADR-0035: Reusable frame buffers for the Arena tick path

- **Status:** Accepted
- **Date:** 2026-10-27
- **Authors:** architect-role
- **Reviewers:** user (strategy approved 2026-10-27 on task `t9-gc-allocation`)
- **Related ADRs:** [ADR-0033](ADR-0033-performance-thresholds-for-cut-over.md) (metric 3), [ADR-0034](ADR-0034-automated-performance-measurement.md) (harness), [ADR-0020](ADR-0020-presentation-domain-read-model-values.md) (read-model values; lifetime amended here), [ADR-0005](ADR-0005-domain-events-and-application-notifications.md), [ADR-0022](ADR-0022-application-notifications-for-one-shot-facts.md), [ADR-0027](ADR-0027-typed-result-dispatcher.md)

## Context

ADR-0033 makes per-frame managed allocation a hard T-9 gate: 0 B after warm-up, summed over the 55 s capture window, in two clean runs. ADR-0034 supplied the harness that measures the real stack in a player build. On the corrected baseline (build `00903f4`, runs 3–4, 3 301 frames each) every captured frame allocates: 6 087 and 6 277 B/frame. T-9 is blocked on it, together with the physics-query budget task.

The allocation is structural, not incidental, and it spans the whole tick path:

| Site | What allocates per frame |
|---|---|
| `ArenaRun.CreateSnapshot()` (`ArenaRun.cs:956`) | the domain state-snapshot graph — one list, ten `EnemyStateSnapshot`, `PlayerStateSnapshot`, `WaveStateSnapshot`, `ArenaRunStateSnapshot` — plus `OrderedEnemyIds()` (`:1256`) |
| `ArenaRunSnapshotMapper.Map` (`:8`) | `PlayerSnapshot`, `EnemySnapshot[10]`, ten `EnemySnapshot`, `WaveSnapshot`, `ArenaRunSnapshot`; the ctor copies the array again and wraps it (`ArenaRunSnapshot.cs:78-92`) |
| `ArenaRunTickResult` ctor (`:56-66`) | `ArenaRunHudSnapshotMapper`, the HUD snapshot, `ArenaRunNotificationMapper` and its list, three arrays and three `ReadOnlyCollection`s; the coordinator also creates a recorder with two lists (`ArenaRunTickCoordinator.cs:20`) |
| Aggregate mutations (`ArenaRun.cs:544, 659, 749, 822, 895, 1218, 1223`) | per-operation event lists, `ArenaRunChange` (itself wrapping a copied array, `ArenaRunChange.cs:27-38`), an `EnemyMovementBatchRequest`/`Resolution`/outcome quartet, and a `Dictionary<EnemyId, Position3D>` per applied batch |
| `ArenaRunResultDispatcher` (`:217, :238, :260`) | one delegate closure per registered handler per dispatch, and one per handler × notification |
| `EnemyPresenter.CollectInside` (`:58`), `UnityPlayerInputSource.Read()` (`:47`) | a `HashSet<EnemyId>` and a `PlayerFrameInput` per frame |

Existing decisions constrain the fix. ADR-0020 lets Presentation read Application read models and Domain value types, and the whole codebase treats published snapshots as immutable values. ADR-0005 and ADR-0022 establish typed Domain events mapped to Application notifications. ADR-0027 fixes the typed dispatcher and its error isolation. The aggregate's own invariants and immutability are a Domain rule and are not in question. Domain and Application must stay Unity-free, the layering graph must not change, and the standalone suite (588 tests) must stay green.

## Decision

We will keep every per-tick type and its public shape, and make the Arena tick path allocate nothing by **reusing preallocated instances of those types, owned by the Application tick service and published through a two-slot ping-pong frame.**

The contract:

1. **Ownership.** One `ArenaTickFrameBuffers` instance is created per active run by the Application tick service. It owns both slots, every snapshot instance, the event and change pools, the recorder, the tick result, the mapper instances and the input instance. Capacity is derived from the configuration's wave catalogue (the largest wave — 24 for the shipped catalogue, ADR-0033 metric 4), never from a hard-coded number.
2. **Two-slot publication.** Each tick fills the slot not published by the previous tick. A published `ArenaRunTickResult` — and everything reachable from it: snapshots, HUD snapshot, domain events, notifications, executed stages — stays valid for the tick that published it **and the tick after it**. Nothing may be retained longer than one further tick. The second slot exists precisely so that a handler comparing "now" against "the previous tick" keeps working; the existing presenters do exactly that.
3. **Fill, do not construct, on the hot path.** Types gain an internal `Fill`/`Reset` path; the public constructors stay for tests, manual construction and cold paths, and public properties stay read-only. A filled instance is a shell whose value is replaced wholesale each time the slot is reused.
4. **Ping-pong for every per-tick graph.** The domain state snapshot, the Application read models, the HUD snapshot, the tick result, the recorder and the interaction objects are all filled into their preallocated instance. Wrapper objects that can be built once over a stable array (`Array.AsReadOnly`, the enemy list) are built once, in the buffer's constructor.
5. **Domain events and changes are pooled per run.** A mutation fills a reused `ArenaRunChange` and reuses event instances from a per-run pool; they are returned to the pool at the start of the following tick, which is the same one-tick lifetime rule. `ArenaRunChange` stops copying its caller's list.
6. **Interaction objects are per-kind singletons.** One reusable instance per interaction class, refilled when a new request opens and never reset while the interaction is pending — the sequential interaction protocol admits at most one pending interaction at a time (ADR-0006).
7. **The dispatcher allocates no delegate.** Handler invocation moves behind direct, per-category `try`/`catch` calls (or a generic method resolved once at registration); the error policy and stage ordering of ADR-0027 are unchanged.
8. **Presentation reuses its own collections** (the per-frame `HashSet`) and may retain a snapshot for at most one further tick — the rule in point 2, which the four combat presenters and `ArenaRunNotificationPresenter` already satisfy.
9. **Infrastructure adapters refill a reusable `PlayerFrameInput`** and the same one-tick validity rule applies to it.

What changes is the **lifetime** of published values, not their type, their shape, or their read-only surface. ADR-0020's rule that Presentation consumes read-model values is unchanged; its implicit assumption that a published snapshot is a durable value is narrowed to the two-tick window above.

Explicitly out of scope: converting read models or events to structs; the physics-query budget; the ADR-0033 thresholds; allocation on cold paths (run start, scene load, defeat/victory transitions); and any change to the aggregate's internal state model.

## Options considered

### Option A — Reusable frame buffers, two-slot ping-pong (chosen)
- Pros: reaches the recorded 0 B; keeps every handler signature, every event type and the class-based event system; one uniform lifetime rule covers snapshots, events, notifications and input; the two-slot window keeps the existing previous-tick comparisons working unchanged; no layering change; constructors survive for the test suite.
- Cons: published values become shells reused in place, so an aliasing mistake is possible in principle; it needs a documented lifetime rule, a debug-only generation guard, and discipline in review; the aggregate's interaction objects become mutable-in-place during a tick.
- Why chosen: it is the only route that meets 0 B without discarding the domain event contract, and its risk is bounded by synchronous dispatch plus a small, known set of retaining handlers.

### Option B — Value-type tick path (readonly structs, fixed buffers)
- Pros: no aliasing at all; value semantics match `PlayerMovementRequest`, `PlayerMovementResolution` and `EnemyMovementBatchResolutionEntry`, which are already structs.
- Cons: `IArenaDomainEvent` cannot hold structs polymorphically without boxing, so the event system becomes a closed struct union — an ADR-0005 rewrite; the read-model contract of ADR-0020, the dispatcher generics and essentially the whole tick-path test surface change.
- Why not chosen: the largest diff in the repository, aimed at a problem the reuse scheme already solves; it trades a bounded aliasing risk for a much larger migration and loses class-based event polymorphism.

### Option C — Incidental cleanups, then revise the metric by a superseding ADR
- Pros: smallest change; keeps ADR-0020 untouched; removes the worst offenders (double copy, closures, per-frame collections).
- Cons: does not reach 0 B — the two snapshot graphs and the interaction objects still allocate, estimated at 2–4 KB/frame; it revises an accepted gate that ADR-0033 deliberately made hard.
- Why not chosen: it concedes the metric instead of meeting it, for a saving that a measured run would have to re-establish anyway.

## Consequences

### Positive

- ADR-0033 metric 3 becomes reachable in two clean runs, unblocking the last T-9 measurement gate together with the physics task.
- One uniform, testable lifetime rule replaces per-type assumptions; the allocation source is concentrated in one owned object instead of scattered across the pipeline.
- The dispatch path stops allocating a delegate per handler, which also removes an allocation that would have grown with the handler count.

### Negative / trade-offs

- A published value is no longer a durable value: a handler that stores a snapshot, event or notification for longer than one tick reads the next tick's data. This is the one new failure mode, and it is invisible to the compiler.
- The Domain interaction objects become mutable-in-place during a tick; a future contributor may read this as licence to mutate snapshots elsewhere. The rules above (constructors preserved, properties read-only, fill internal) contain it.
- Reused events mean a domain event is no longer a self-contained value once its pool slot is recycled; logging or debugging that holds an event reference past the window sees changed data.

### Neutral

- Memory grows by one extra frame of buffers (kilobytes), which is not measured by ADR-0033.
- Tests keep constructing snapshots through their constructors; only tests that build a whole tick through the service see the reused instances.
- The pool is released at the start of the next tick, so a tick that throws still leaves the previous tick's result intact for diagnostics.

## Compliance with paradigm

- The paradigm is unchanged: DDD/IDDD with Clean Architecture layering. No layer, no assembly reference and no dependency direction changes.
- `docs/architect/paradigm.md` needs no exception entry: the decision touches lifetime and allocation, not structure.
- ADR-0020's value-reading rule stands; its durability assumption is amended below.
- ADR-0005's typed domain events and ADR-0022's notification mapping stand — the events are reused, not removed.
- ADR-0027's stage order and error isolation stand; only the invocation mechanism changes.
- ADR-0007 is preserved: Domain and Application stay Unity-free; the buffers are plain C#.
- ADR-0015 is preserved: the buffers live in Application, so the standalone core build covers them.

## Migration plan

1. Add the `ArenaTickFrameBuffers` type and the `Fill`/`Reset` path to the per-tick types; keep the constructors and public surfaces intact. Build and the existing suites stay green at every step.
2. Convert the domain state snapshot and the Application read-model mapping to fill the frame; remove the double array copy and per-tick `ReadOnlyCollection` wrappers.
3. Pool domain events, `ArenaRunChange` and the interaction objects; convert the aggregate's per-operation lists and the per-batch dictionary to reused scratch storage.
4. Remove the dispatcher closures and reuse the recorder, tick result, HUD snapshot and mappers.
5. Reuse the Presentation `HashSet` and the Infrastructure `PlayerFrameInput`; keep the existing one-tick uses intact.
6. Add an allocation-regression test in the editor test suites, where `GC.GetAllocatedBytesForCurrentThread()` works (it does not in a 2022.3.46f1 player): run a steady-state tick loop after warm-up and assert a zero delta.
7. Re-run the ADR-0034 measurement twice; metric 3 must read exactly 0 B in both. Record the rows in `docs/architect/migration/performance-measurement.md` and update the T-9 gate.

## References

- `docs/architect/migration/performance-measurement.md` § Recording results (runs 3–4)
- `docs/architect/tasks/t9-gc-allocation.md`
- `Assets/_Project/Contexts/Arena/Application/Ticks/`
- `Assets/_Project/Contexts/Arena/Application/ReadModels/`
- `Assets/_Project/Contexts/Arena/Domain/Aggregates/ArenaRun/ArenaRun.cs`
- `Assets/_Project/Contexts/Arena/Presentation/Enemies/EnemyPresenter.cs`
- `Assets/_Project/Contexts/Arena/Infrastructure/Input/UnityPlayerInputSource.cs`
