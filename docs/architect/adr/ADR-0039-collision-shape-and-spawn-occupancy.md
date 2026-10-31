# ADR-0039: Make CollisionRadius the single collision-shape contract and space spawns by live occupancy

- **Status:** Accepted
- **Date:** 2026-10-31
- **Authors:** architect-role
- **Reviewers:** user (selected Package B — Application-aware spacing — on 2026-10-31)
- **Related ADRs:** [ADR-0007](ADR-0007-unity-isolation-and-layer-dependencies.md), [ADR-0011](ADR-0011-domain-authority-over-movement-resolution.md), [ADR-0023](ADR-0023-arena-bounds-sliding.md), [ADR-0028](ADR-0028-live-enemy-collider-registration.md), [ADR-0035](ADR-0035-reusable-frame-buffers.md), [ADR-0037](ADR-0037-metric-3-scope-and-baseline.md), [ADR-0038](ADR-0038-physics-query-budget.md)
- **Task:** `docs/architect/tasks/enemy-collision-fix.md`

## Context

In the Arena scene enemies appear to spawn and remain clumped at one point.
The user verified that the behaviour persists with the ADR-0038 query budget
disabled, so the cache is not the cause.

Three defects combine:

1. `UnityEnemySpawnPlacementResolver.ResolveSpawnPosition` walks a fixed
   candidate ring and returns the first point outside the camera frustum. It
   receives only the request (kind and radius) and keeps no memory of earlier
   spawns; `EnemySpawnStage` calls it once per spawn, and the Domain's
   `IsSpawnPosition` validates only the Spawn Band. A still camera/player
   therefore yields the same point for every spawn in a wave.
2. `UnityEnemyMovementResolver.GeometryLimitedDistance` skips hits with
   `hit.distance <= 0f`, which is what Unity reports when the cast sphere
   starts inside another collider. Co-located enemies are invisible to the
   query and move as one clump; the resolver only shortens forward travel.
3. The collision shape is defined three times: the Domain's planar
   `CollisionRadius` at ground `Position`; the prefab's `BoxCollider` on the
   child `Body` at local `(0, 0.5, 0)` with size `1×1×1`; and the adapter's
   sphere cast centred at `from.y + CenterHeightOffset`, where
   `CenterHeightOffset` looks for a collider on the root transform (which has
   none) and falls back to the radius. Regular radius `0.5` happens to match
   the prefab; boss radius `1.0` does not.

The Domain does not determine Unity collision (INV-081); the adapter owns it.
F-007 explicitly deferred enemy crowding: "Worth an architect decision if
enemy crowding matters." The user now reports that it matters. The forces are
ADR-0011 (the adapter may only shorten or stop an intent), ADR-0007 (Domain
and Application stay Unity-free), ADR-0038 (five-query peak), and ADR-0035 /
ADR-0037 (no per-tick allocation after warm-up).

## Decision

We will make `CollisionRadius` the single authoritative planar footprint and
give spawn placement the live enemy footprints, because the same-point spawn
is the root cause of the clump and the three collision origins make both the
spawn and the movement queries unreliable.

Concretely:

- **Collision shape.** Every enemy view's target collider must have a
  horizontal half-extent equal to the snapshot's `CollisionRadius` and a world
  centre at `Position + (0, CollisionRadius, 0)`. The prefab's collider stays
  `1×1×1` at local `(0, 0.5, 0)`; the view scales its root by
  `radius / 0.5` when presented, which also scales the visual body.
- **Movement cast origin.** `IEnemyTransformRegistry` gains
  `TryGetCollider(EnemyId, out Collider)`; `EnemyViewTransformRegistry`
  returns `EnemyView.TargetCollider`. `UnityEnemyMovementResolver` casts from
  `(from.x, collider.bounds.center.y, from.z)` and keeps the self-skip on the
  root transform. If no collider is registered, the current
  `from.y + radius` fallback stays.
- **Spawn occupancy.** `IEnemySpawnPlacementResolver.ResolveSpawnPosition`
  gains a `ReadOnlySpan<EnemyFootprint>` of live positions and radii, where
  `EnemyFootprint` is an Application-port value type carrying `Position3D` and
  `CollisionRadius`. The Application builds the span from a reusable
  `ArenaRunStateSnapshot` filled by `ArenaRun.CreateSnapshot(target)`, into a
  preallocated `EnemyFootprint[]`; the path allocates nothing after the first
  spawn. The adapter returns a Spawn Band point at ground height, outside the
  current view, and at least `radius + otherRadius` horizontally from every
  occupied footprint. If every unoccupied candidate is visible it returns the
  farthest-from-aim unoccupied candidate; if every candidate is occupied it
  returns the farthest-from-aim candidate and the overlap is a recorded
  limitation (128 candidates for 10 enemies in the default profile).
- **Enemy–enemy overlap.** Enemies remain blocking geometry. A cast that
  starts inside another enemy's collider (`hit.distance <= 0` and
  `IEnemyColliderRegistry.TryGetEnemyId` resolves it) is treated as blocked —
  travel `0` — instead of skipped. Overlaps with non-enemy colliders keep the
  existing skip. No separation push is added; spawn occupancy is the
  prevention and the zero-distance block is the safety net.

Concrete consequences:

- **Modules affected:** `Application/Ports` (new `EnemyFootprint`, changed
  `IEnemySpawnPlacementResolver`), `Application/Ticks/Stages/Enemy/EnemySpawnStage.cs`,
  `Infrastructure/Spawning/UnityEnemySpawnPlacementResolver.cs`,
  `Infrastructure/Movement/IEnemyTransformRegistry.cs` and
  `UnityEnemyMovementResolver.cs`, `CompositionRoot/EnemyViewTransformRegistry.cs`
  and `ArenaRegistrations.cs`, `Presentation/Enemies/EnemyView.cs` and
  `EnemyViewRegistry.cs`, `Resources/Prefabs/EnemyView.prefab` (unchanged size,
  now scaled at presentation), and their PlayMode fixtures.
- **New dependencies:** `UnityEnemyMovementResolver` additionally depends on
  the existing `IEnemyColliderRegistry` (Infrastructure-internal). No new
  assembly, package, or layer edge.
- **Out of scope:** separation or steering, layer-based enemy pass-through,
  targeting changes, ADR-0038's query budget, and any change to the Domain's
  bounds or admission rules.

## Options considered

### Option A — Adapter-local spawn spacing
- Pros: confined to the spawn resolver; no port change; no extra physics query.
- Cons: the adapter does not know live enemy positions, so spacing is only
  against its own recent choices; a moved enemy is not accounted for.
- Why not chosen: it fixes the first wave but not the general case, and it
  leaves the collision-shape ambiguity untouched.

### Option B — Application-aware spawn spacing (chosen)
- Pros: spacing against every live enemy, including one that moved; the
  Application already owns the run snapshot; the Domain and Application stay
  Unity-free.
- Cons: public Application port change; the tick must build the occupancy
  buffer allocation-free; fixtures and the tick contract change.
- Why chosen: correctness against live enemies at the smallest contract that
  expresses occupancy, with no new physics queries.

### Option C — Enemies pass through each other
- Pros: no queueing or clumping; the simplest crowd behaviour.
- Cons: enemies can stack visually on the player; changes F-007's accepted
  enemy-blocking semantics; needs a layer configuration and a contract note.
- Why not chosen: it removes physicality rather than fixing the shape and the
  spawn, and the user asked for correct collision, not its removal.

## Consequences

### Positive

- One explicit collision shape: `CollisionRadius` drives bounds, spawn
  clearance, the view collider, and the movement cast.
- Spawns are spaced against every live enemy, so the one-place clump cannot
  recur from placement.
- The boss's physical collider matches its domain radius instead of being half
  the size.
- The zero-distance overlap is blocked, so an enemy can no longer pass through
  another.

### Negative / trade-offs

- The placement port and its call sites change; the Application must carry a
  reusable snapshot buffer and footprint array.
- Scaling the view changes the boss's visual size (intended: it is twice the
  radius) and couples the prefab's base collider half-extent (`0.5`) to the
  scale rule. The view should derive the base half-extent from the collider at
  startup rather than hard-code it.
- No separation rule: enemies can still queue behind one another, and an exact
  simultaneous convergence can leave two enemies touching. The zero-distance
  block prevents pass-through but does not push them apart.

### Neutral / Risks

- No new physics queries are added; ADR-0038's five-query peak is unchanged.
- The occupancy buffer is preallocated and reused; ADR-0035 / ADR-0037's
  allocation rules are preserved after warm-up.
- The Domain still does not determine Unity collision (INV-081); it continues
  to validate only the band, the ground, and the wave phase.

## Compliance with paradigm

- **ADR-0007:** `EnemyFootprint` carries Domain value types only; Domain and
  Application reference no Unity type. The collider stays in Infrastructure,
  Presentation, and CompositionRoot.
- **ADR-0011:** the movement adapter still only shortens or stops an intent;
  it never re-aims, extends, adds, or drops one. Spawn placement remains a
  payload choice validated by the Domain.
- **ADR-0023:** Arena Bounds remain Domain-owned; the adapter keeps its
  bounds clamp.
- **ADR-0028:** one target collider per enemy view; `TryGetCollider` returns
  exactly that collider.
- **ADR-0038:** no new query sites; the same three `NonAlloc` markers remain
  the only counted calls.
- **ADR-0035 / ADR-0037:** the occupancy buffer is reused; no per-tick
  allocation after the first spawn.

No paradigm exception is needed.

## Migration plan

1. Add `EnemyFootprint` and the new `IEnemySpawnPlacementResolver` signature;
   update `EnemySpawnStage` to fill a reusable snapshot and footprint buffer
   and pass the span.
2. Update `UnityEnemySpawnPlacementResolver` to reject occupied candidates and
   keep the outside-view and farthest-from-aim rules.
3. Add `TryGetCollider` to `IEnemyTransformRegistry` and
   `EnemyViewTransformRegistry`; update `UnityEnemyMovementResolver` to cast
   from the collider centre and to block a distance-0 enemy overlap, injecting
   `IEnemyColliderRegistry`.
4. Scale the view from the snapshot radius in `EnemyView`/`EnemyViewRegistry`;
   derive the base half-extent from the target collider at startup.
5. Update the PlayMode fixtures: multiple spawns in one run are spaced; a
   moved enemy's new position is avoided; the boss collider matches radius
   `1.0`; a distance-0 enemy overlap yields travel `0`; existing fixtures stay
   green.
6. Run the scoped fixtures and `dotnet test Wild_Wild_West.Core.sln`; the tick
   path must stay allocation-free after warm-up.

Each step leaves the build green.

## References

- `docs/architect/tasks/enemy-collision-fix.md`
- `docs/architect/architecture/arena-combat/application-tick-contract.md` § External Resolution Boundary
- `docs/architect/coder-inbox/F-007.md` (enemy–enemy blocking deferred)
- `Assets/_Project/Contexts/Arena/Infrastructure/Spawning/UnityEnemySpawnPlacementResolver.cs`
- `Assets/_Project/Contexts/Arena/Infrastructure/Movement/UnityEnemyMovementResolver.cs`
- `Assets/_Project/Contexts/Arena/Application/Ticks/Stages/Enemy/EnemySpawnStage.cs`
- `Assets/_Project/Contexts/Arena/Presentation/Enemies/EnemyView.cs`
- `Assets/_Project/CompositionRoot/EnemyViewTransformRegistry.cs`
- `Assets/Resources/Prefabs/EnemyView.prefab`
