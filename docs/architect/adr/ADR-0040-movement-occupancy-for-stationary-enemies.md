# ADR-0040: Give movement resolution the live enemy occupancy

- **Status:** Accepted
- **Date:** 2026-10-31
- **Authors:** architect-role
- **Reviewers:** user (selected Option B — Application-aware movement occupancy — on 2026-10-31)
- **Related ADRs:** [ADR-0007](ADR-0007-unity-isolation-and-layer-dependencies.md), [ADR-0011](ADR-0011-domain-authority-over-movement-resolution.md), [ADR-0028](ADR-0028-live-enemy-collider-registration.md), [ADR-0035](ADR-0035-reusable-frame-buffers.md), [ADR-0037](ADR-0037-metric-3-scope-and-baseline.md), [ADR-0038](ADR-0038-physics-query-budget.md), [ADR-0039](ADR-0039-collision-shape-and-spawn-occupancy.md)
- **Task:** `docs/architect/tasks/c10-residual-overlap.md`

## Context

The c10 regression work closed the simultaneous-movement overlap and the
zero-distance pass-through, and added `EnemyOverlapRegressionTests`. One path
remains open: a stationary enemy can be invisible to every collision source.

Two deliberate rules combine to produce it:

1. The Domain omits an Enemy with a pending attack or one already within its
   attack range from the movement batch (`ArenaRun.RequestEnemyMovementBatch`),
   so the stop-to-attack rule belongs to the Domain and not to the adapter.
2. Presentation creates an Enemy view — and therefore the target collider the
   movement adapter casts from and the collider registry maps back — only when
   `ArenaBounds.Contains(enemy.Position, enemy.CollisionRadius)` holds. The
   outside-to-inside transition is the presentation entry moment.

Concrete condition: the Player stands at the Arena edge; an Enemy stops at
attack range with its centre within one collision radius of the boundary. Its
footprint is not fully inside the Bounds, so it has no view and no collider; it
is also not in the batch. A second Enemy approaching it has neither a batch peer
to cap against nor a collider to cast into, and can overlap it.

Forces: ADR-0011 (the adapter may only shorten or stop an intent), ADR-0007
(Domain and Application stay Unity-free), ADR-0035 / ADR-0037 (no per-tick
allocation after warm-up), ADR-0038 (five-query peak), and ADR-0039 (the
`CollisionRadius` is the single footprint, and spawn placement already receives
live occupancy built from a reusable snapshot).

## Decision

We will give `IEnemyMovementResolver` the live enemy occupancy alongside the
movement batch, because the collision source must come from published state
rather than from presentation, and because the Application already builds the
same occupancy for spawn placement.

Concretely:

- **Port input.** `IEnemyMovementResolver.Resolve` gains a
  `ReadOnlySpan<EnemyFootprint>` of every live Enemy, carrying its `EnemyId`,
  ground `Position3D`, and `CollisionRadius`. `EnemyFootprint` gains the
  `EnemyId` it does not carry today; the spawn resolver ignores it.
- **Application side.** `EnemyMovementStage` fills a reusable
  `ArenaRunStateSnapshot` and a preallocated `EnemyFootprint[]` sized to
  `run.EnemyCapacity`, exactly as `EnemySpawnStage` does for spawn placement,
  and passes the span to the resolver. The buffers are allocated once per
  capacity and reused; the path allocates nothing after warm-up.
- **Adapter side.** `UnityEnemyMovementResolver` caps every intent against every
  other live footprint: a batch peer is capped against its accepted position,
  a stationary Enemy against its snapshot position. The cap is the existing
  first-contact fraction of the relative motion. The mover excludes itself by
  id. A pair that already overlaps is left in place (fraction `0`); no
  separation push is added.
- **Unchanged.** The Domain's batch eligibility rule, the presentation bounds
  gate, the view and prefab, targeting, and the physics query sites all stay as
  they are. No new query site is introduced, so ADR-0038's three markers and
  five-query peak are unchanged.

Concrete consequences:

- **Modules affected:** `Application/Ports` (`EnemyFootprint`,
  `IEnemyMovementResolver`), `Application/Ticks/Stages/Enemy/EnemyMovementStage.cs`
  and `EnemySpawnStage.cs` (constructor call gains the id),
  `Infrastructure/Movement/UnityEnemyMovementResolver.cs`, the composition fake
  in `ArenaCompositionTests`, and the movement and overlap PlayMode fixtures.
- **New dependencies:** none. The Application still references no Unity type;
  the adapter gains no new collaborator.
- **Out of scope:** presentation or targeting changes, the Domain batch rule,
  spawn placement, separation or steering, new physics queries, and ADR-0038's
  budget.

## Options considered

### Option A — Collision-bearing view for every live enemy
- Pros: no Application/Domain contract change; collision follows physical
  presence.
- Cons: couples collision to Presentation; one extra view object per live
  enemy; targeting shares `IEnemyColliderRegistry`, so an invisible Enemy either
  becomes hittable or needs a targetability filter — a new seam in disguise.
- Why not chosen: it moves the collision source into the layer that deliberately
  withholds it, and the targeting question is unresolved.

### Option B — Application-aware movement occupancy (chosen)
- Pros: covers every live Enemy regardless of presentation or batch membership;
  mirrors ADR-0039's spawn occupancy; no new physics query; allocation-free
  after warm-up; keeps the Domain batch rule and ADR-0011; collision stays
  adapter-owned.
- Cons: public Application port change; a reusable buffer in the movement
  stage; `EnemyFootprint` gains identity; fixtures change.
- Why chosen: it is the smallest contract that expresses "every live Enemy is
  blocking geometry", and it uses the occupancy pattern already accepted for
  spawn placement.

### Option C — Domain includes stationary enemies as zero-travel intents
- Pros: no Application port change; the batch stays the single collision input.
- Cons: changes the Domain's batch eligibility rule; zero-travel intents need
  type and validation support; interacts with the attack phase and the
  contract's "stop-to-attack is not the adapter's" line; most Domain surface to
  test.
- Why not chosen: it weakens a Domain rule that is deliberately not the
  adapter's, and its interaction with the attack stage is not needed to solve
  the gap.

## Consequences

### Positive

- Every live Enemy is a collision source, whether or not it is presented or
  moving, so the stationary-edge overlap is closed at the adapter.
- The collision source is published state, so a missing or delayed view cannot
  silently remove an Enemy from collision.
- The occupancy pattern is shared with spawn placement: one snapshot, one
  preallocated buffer, one `EnemyFootprint` type.

### Negative / trade-offs

- The movement port changes and its call sites, the composition fake, and the
  fixtures change with it.
- The Application builds the live-enemy span on every movement tick and the
  adapter's pair pass is quadratic in live enemies. With the default ten
  enemies the cost is negligible, but it grows with `EnemyCapacity`.
- The pair cap remains conservative: it stops at contact and never separates an
  already-overlapping pair.

### Neutral / Risks

- No new physics queries; ADR-0038's peak is unchanged.
- The occupancy buffers are preallocated and reused; ADR-0035 / ADR-0037's
  allocation rules are preserved after warm-up.
- The Domain still decides who is eligible to move and still validates that an
  accepted position lies on its intent's path (ADR-0011).

## Compliance with paradigm

- **ADR-0007:** `EnemyFootprint` carries Domain value types only; Domain and
  Application reference no Unity type.
- **ADR-0011:** the adapter still only shortens or stops an intent; the new
  input adds obstacles, not authority.
- **ADR-0028:** the collider registry is unchanged; occupancy does not replace
  it, it complements it for Enemies that have no collider.
- **ADR-0038:** no new query site.
- **ADR-0035 / ADR-0037:** buffers are reused; no per-tick allocation after
  warm-up.
- **ADR-0039:** `CollisionRadius` remains the single footprint; the new input
  reuses its type and its buffer pattern.

No paradigm exception is needed.

## Migration plan

1. Add `EnemyId` to `EnemyFootprint` and update the two construction sites.
2. Change `IEnemyMovementResolver.Resolve` to take the occupancy span; update
   the composition fake and every call site.
3. Fill the reusable snapshot and footprint buffer in `EnemyMovementStage` and
   pass the span.
4. Cap each intent against every other live footprint in
   `UnityEnemyMovementResolver`, using the peer's accepted position or the
   stationary Enemy's snapshot position.
5. Add fixtures: a stationary Enemy omitted from the batch blocks a mover; a
   stationary Enemy with no view and no collider blocks; existing overlap,
   movement, and spawn fixtures stay green.
6. Run the scoped fixtures and `dotnet test Wild_Wild_West.Core.sln`; the tick
   path must stay allocation-free after warm-up.

Each step leaves the build green.

## References

- `docs/architect/tasks/c10-residual-overlap.md`
- `docs/architect/architecture/arena-combat/application-tick-contract.md` § External Resolution Boundary
- `docs/architect/coder-inbox/c10-residual-overlap.md`
- `Assets/_Project/Contexts/Arena/Application/Ports/EnemyFootprint.cs`
- `Assets/_Project/Contexts/Arena/Application/Ports/IEnemyMovementResolver.cs`
- `Assets/_Project/Contexts/Arena/Application/Ticks/Stages/Enemy/EnemyMovementStage.cs`
- `Assets/_Project/Contexts/Arena/Infrastructure/Movement/UnityEnemyMovementResolver.cs`
- `Assets/_Project/Contexts/Arena/Presentation/Enemies/EnemyViewRegistry.cs`
