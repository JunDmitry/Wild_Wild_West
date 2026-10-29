# ADR-0038: Bound per-frame physics queries with an age-priority budget

- **Status:** Accepted
- **Date:** 2026-10-29
- **Authors:** architect-role
- **Reviewers:** user (approved the selection policy and the five-query peak on 2026-10-29)
- **Related ADRs:** [ADR-0033](ADR-0033-performance-thresholds-for-cut-over.md), [ADR-0034](ADR-0034-automated-performance-measurement.md), [ADR-0011](ADR-0011-domain-authority-over-movement-resolution.md), [ADR-0023](ADR-0023-arena-bounds-sliding.md), [ADR-0028](ADR-0028-live-enemy-collider-registration.md)

## Context

ADR-0033 metric 5 requires at most 5 physics queries per frame. The measured
player build issues 12: one `Physics.SphereCastNonAlloc` for the player's
movement, one per enemy intent in the batch (10 enemies), and one targeting
query on an attack frame. The hit buffers are already preallocated, so the
count is structural: the adapters query every agent every tick.

The query sites are exactly three production files:
`UnityPlayerMovementResolver`, `UnityEnemyMovementResolver`, and
`UnityPlayerAttackTargetingResolver`. The measurement probe resolves exactly
the three `NonAlloc` marker names those files call, so neither the metric nor
the adapter may be changed by swapping in an uncounted physics API.

The Domain contract permits a resolver to shorten an intent but never to
re-aim, extend, add, or drop one (ADR-0011), and `ArenaBounds` sliding is
resolved in the Domain before the adapter is called (ADR-0023). The movement
adapter is a scene-scope singleton, so it can carry per-enemy state across
ticks; a scene reload constructs a new resolver.

The remaining T-9 blocker is therefore not correctness but cadence: how
often each agent's scene-geometry query must run.

## Decision

We will cap enemy movement geometry queries at three per frame with an
age-priority budget, leaving the player's one movement query and at most one
attack query, so an attack frame peaks at five queries and a non-attack frame
at four.

Concretely:

- **Player movement** keeps one sphere cast on every tick in which it has a
  movement request.
- **Attack targeting** keeps its current cadence: at most one ray cast or one
  overlap per attack resolution, and none otherwise.
- **Enemy movement** spends at most three sphere casts per tick. The
  resolver selects, among the enemies in the batch: never-queried enemies
  first; then the enemy whose cached result is oldest; a materially changed
  direction breaks ties; the stable batch index breaks a remaining tie.
- **Cache reuse.** A skipped enemy reuses the reachable distance from its
  last queried geometry result, applied along the current intent's direction
  and clamped to the current requested distance. That position lies on the
  requested path and never exceeds it, so the Domain's admission rule is
  unchanged. Arena Bounds are recomputed from the current intent every tick
  and are never stale.
- **Staleness bound.** With `N` intents in a batch, every enemy is re-queried
  within `ceil(N / 3)` ticks, so a cached geometry result is at most
  `ceil(N / 3) - 1` ticks old: three ticks (about 50 ms at 60 Hz) for the
  measured 10 enemies, seven ticks (about 117 ms) at the 24-enemy ceiling of
  ADR-0033 metric 4.
- **Lifecycle.** A first-sight enemy is served before any cached peer; when more
  than three enemies first appear in the same batch, the budget wins and batch
  order selects which three query, so the rest move without a geometry
  constraint until a later resolve. Entries absent from a batch are pruned. The
  cache belongs to the resolver singleton, so a scene reload rebuilds it empty.
- **No metric evasion.** The adapters continue to use the three `NonAlloc`
  APIs the probe counts. Switching to an uncounted query API is not part of
  this decision.

Concrete consequences:

- **Modules affected:** `UnityEnemyMovementResolver` only, plus its PlayMode
  tests and the tick contract note (§ External Resolution Boundary).
- **New dependencies:** none. No new assembly, package, port, or public
  Domain/Application contract.
- **Out of scope:** the player resolver's cadence, the attack resolver's
  cadence, per-platform thresholds, and any change to ADR-0033's other four
  metrics or to the measurement profile.

## Rationale

- The metric counts query calls, and the only calls that can be removed
  without changing gameplay rules are redundant geometry queries for agents
  whose scene has not changed.
- An age-priority ordering gives a hard bound on staleness that a purely
  invalidation-driven scheme cannot: invalidation can save more queries in
  open field, but only the age fallback guarantees every enemy is served.
- Age priority is deterministic: it uses the resolver's own tick counter and
  the batch order, not wall-clock time or a physics query result.
- Reusing a distance along the current path preserves ADR-0011's
  shorten-only contract and needs no Domain change; the Domain still performs
  final admission.
- Three enemy queries plus one player query plus one attack query is the
  smallest split that meets the metric at the measured profile while leaving
  the player fully responsive.

## Consequences

### Positive

- The peak reaches ADR-0033's limit with no change to Domain or Application,
  and no new module boundary.
- The staleness of scene geometry is expressed as an explicit formula that
  scales with the enemy count rather than as an unstated assumption.
- The change is confined to one Infrastructure adapter and its fixtures.

### Negative / trade-offs

- Enemy geometry can be up to three ticks stale at the measured profile. A
  wall that becomes relevant immediately after an enemy's slot is entered for
  up to three ticks; the movement is still bounds-clamped and still on the
  requested path.
- A stale wall-shortened result may under-travel for a few ticks before the
  next query frees the enemy, which can look like a brief stutter.
- The cache adds per-enemy state and a selection pass to every tick; the
  resolver must stay allocation-free after warm-up, like the rest of the tick
  path.

### Neutral / Risks

- The bound depends on the batch order being stable; the Domain builds it
  from its own enemy order, which is stable within a run.
- A newly spawned enemy is queried on first sight, so it never moves on a
  reused result.
- The measurement is the proof: metric 5 must pass in two clean runs. A
  failure is a new measurement result, not a silent threshold change.

## Alternatives

1. **Fixed round-robin.** Each enemy queries once every `ceil(N / 3)` ticks on
   a slot assigned by batch index.
   - Pros: the simplest state and the most predictable pattern.
   - Why not chosen: a sudden turn toward a wall is ignored until the slot
     comes round, and the order gives no responsiveness benefit.
2. **Invalidation-first budget.** Spend the three slots first on enemies whose
   last query was wall-shortened or whose direction changed materially, with
   age as the fallback.
   - Pros: the best wall responsiveness.
   - Why not chosen: the same bound as the chosen option, with more branches
     and a larger test surface for a benefit the measured profile does not
     demonstrate.
3. **Leave the cadence unchanged and revise metric 5.**
   - Why not chosen: ADR-0033 permits revising a threshold only when it is
     demonstrably wrong for a platform reason, and the count is structural
     rather than platform-driven.

## Compliance with paradigm

- The change stays in Infrastructure: Domain and Application continue to
  reference no Unity type (ADR-0007), and the resolver port keeps its shape.
- ADR-0011 and ADR-0023 are preserved: the adapter still only shortens an
  intent, and Arena Bounds remain Domain-owned and freshly recomputed.
- No new bounded context, module, assembly, or cross-cutting mechanism is
  introduced, so the layer graph in `docs/architect/paradigm.md` is
  unchanged.

## Migration plan

1. Add the per-enemy cache, the tick counter, and the age-priority selection
   to `UnityEnemyMovementResolver`; cap enemy geometry queries at three per
   resolve.
2. Reuse the cached distance for skipped enemies along the current direction,
   clamp it to the requested distance, query on first sight, and prune absent
   entries.
3. Extend the PlayMode fixtures: the cap holds with more than three intents;
   the staleness bound is honoured; a first-sight and a changed-direction
   intent query; a reused result stays on the path and inside the requested
   distance; the existing single- and two-intent tests stay green.
4. Run the existing `EnemyMovementResolverTests`, `EnemyMovementTickTests`,
   and the other movement fixtures; the tick path must stay allocation-free.
5. Re-measure two clean runs and record them in
   `docs/architect/migration/performance-measurement.md`; update the T-9 gate
   only when both runs meet metric 5.

Each step leaves the build green.

## References

- `docs/architect/tasks/t9-physics-query-budget.md`
- `docs/architect/migration/performance-measurement.md` § Recording results
- `docs/architect/adr/ADR-0033-performance-thresholds-for-cut-over.md`
- `docs/architect/adr/ADR-0034-automated-performance-measurement.md`
- `docs/architect/architecture/arena-combat/application-tick-contract.md` § External Resolution Boundary
- `Assets/_Project/Contexts/Arena/Infrastructure/Movement/UnityEnemyMovementResolver.cs`
- `Assets/_Project/Contexts/Arena/Infrastructure/Movement/UnityPlayerMovementResolver.cs`
- `Assets/_Project/Contexts/Arena/Infrastructure/Targeting/UnityPlayerAttackTargetingResolver.cs`
- `Assets/_Project/Performance/Runtime/PerformanceProbe.cs`
