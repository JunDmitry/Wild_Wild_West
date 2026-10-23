# ADR-0023: Allow sliding along Arena Bounds in the Domain movement contract

- **Status:** Accepted
- **Date:** 2026-10-23
- **Authors:** architect-role
- **Reviewers:** none
- **Related ADRs:** [ADR-0011](ADR-0011-domain-authority-over-movement-resolution.md), [ADR-0017](ADR-0017-view-relative-player-input.md)

## Context

Design F-001 describes Arena Bounds containment as sliding, not stopping: in
FL-002 the player who holds a direction that points outside the arena but also
has a component along the boundary slides along that boundary, and a diagonal
into a corner stays inside. This restates US-004 E2 of the accepted analysis
("the player moves along Arena Bounds while remaining inside the arena").

The Domain cannot express that today. `ArenaBounds.PermittedTravel` returns one
`Distance` along the *requested* direction: it truncates the whole move at the
first axis whose bound would be crossed. A diagonally moving player therefore
stops at a wall instead of sliding, and `ArenaRun.RequestPlayerMovement` builds
its `PlanarMovementIntent` from that truncated distance on the original
direction. ADR-0011 states this explicitly — "Wall sliding is not part of this
contract and requires a future decision" — and predicted that extending it would
require a new ADR.

ADR-0011's deferral conflated two different obstructions:

- **Arena Bounds** are a closed convex box owned by the Domain; sliding along a
  box face is deterministic geometry and can be decided in the Domain.
- **Scene geometry** (props, walls inside the arena) is resolved by a Unity
  adapter and is subject to ADR-0011's rule that an adapter may only shorten a
  requested movement, never redirect it.

F-001's design requires sliding along Arena Bounds only. Scene-geometry sliding
remains out of scope and unchanged.

## Decision

The Domain resolves movement against Arena Bounds with sliding, and returns the
direction the Player will actually travel.

- `ArenaBounds.PermittedMovement(Position3D from, Direction3D direction,
  CollisionRadius radius, Distance requested)` returns a `BoundedMovement`
  (a `Direction3D` plus a `Distance`) instead of a bare `Distance`.
- When the requested direction does not leave the bounds within the requested
  distance, the result is the requested direction and distance unchanged.
- When an axis would leave its bound, that axis's component is removed from the
  direction and the move continues along the remaining tangential direction.
  The travelled distance is the requested distance scaled by the surviving
  tangential component, so speed along the boundary never exceeds the input
  speed (BR-030, and the "diagonal is not faster" rule of F-001). The distance
  is then clamped to the remaining travel along that tangential direction.
- A direction that points outside with no tangential component produces zero
  movement; a diagonal into a corner, where both axes are limited, also
  produces zero movement.
- `ArenaRun.RequestPlayerMovement` builds the `PlanarMovementIntent` from the
  returned direction and distance. A zero result still ends in
  `PlayerMovementRequestOutcome.PositionUnchanged`.
- `ArenaBounds.PermittedTravel` is removed; the new method is its replacement at
  its only call site.
- External resolvers are unaffected in contract: they still receive a planar
  request and may only shorten it along its path. Because the path is already
  tangential at Arena Bounds, a resolver that stops at the boundary wall does
  not defeat the slide.

## Rationale

- Sliding along Arena Bounds is a rule about where the Player may move, so it
  belongs to the Domain's authority over movement resolution (ADR-0011) rather
  than to an adapter or to the Application.
- Returning direction *and* distance keeps the existing intent model honest: a
  `PlanarMovementIntent` describes the path the adapter must resolve, and after
  sliding that path no longer has the input's direction.
- The projected-distance rule preserves BR-030: pressing into a wall at an angle
  slides more slowly than pressing along it, and no input can make the Player
  travel faster than the configured movement speed.
- The change is deterministic and Unity-free, so it is covered by the standalone
  Domain suite.
- Keeping scene-geometry resolution as shorten-only preserves ADR-0011's reason
  for existing: an adapter must not redirect gameplay intent.

## Consequences

### Positive

- F-001 FL-002 and US-004 E2 become expressible and testable in the Domain.
- Movement resolution stays deterministic and independent of Unity physics.
- The Application and the tick contract do not change shape; only the values
  handed to `PlanarMovementIntent` change.

### Negative

- `ArenaBounds.PermittedTravel` disappears from the public Domain surface; every
  test and caller of it changes.
- The Domain now owns a small amount of movement semantics that a naive reader
  might expect in an adapter.
- A tangential move that is itself blocked by scene geometry still stops; the
  player sees a slide at Arena Bounds but not along props. That asymmetry is
  deliberate and recorded here.

### Neutral / Risks

- Movement near a boundary can alternate between slide and stop as the input
  angle changes; with tolerance-zero comparisons this may need a small
  hysteresis later. The current geometric tolerance is used only to detect a
  vanishing tangential component.
- Resolvers that depended on the request direction being exactly the input
  direction must not assume it; the tick contract now says so.

## Alternatives

1. **Project the movement onto the boundary tangent in the Application.**
   `ArenaRunSnapshot` already carries the bounds, so Application could rewrite
   the input before calling the aggregate.
   - Why not chosen: geometry semantics would live in the Application while the
     Domain keeps its own clamping, duplicating the same rule in two layers.
2. **Keep stopping and rework the design (FL-002 becomes "stops").**
   - Why not chosen: it contradicts an accepted analyst story (US-004 E2) and a
     P0 requirement; the design follows the analysis, not the other way round.
3. **Let the Unity movement resolver slide along colliders.**
   - Why not chosen: violates ADR-0011 — an adapter may shorten a movement but
     must not redirect it.

## Compliance with paradigm

- ADR-0011: Domain authority over movement resolution is strengthened, not
  weakened. The adapter contract (shorten only) is unchanged; only Arena Bounds
  gain the redirected path, and they are Domain-owned.
- ADR-0017: view-relative input is unchanged; the slide happens after the
  Application has produced a planar movement intent.
- ADR-0007: the change is pure Domain code with no Unity references.
- `docs/architect/paradigm.md`: no deviation; no exception is added.

## Migration plan

1. Add `Domain/Geometry/BoundedMovement.cs`.
2. Replace `ArenaBounds.PermittedTravel` with `PermittedMovement`; drop the
   now-unused private helper if it no longer applies.
3. Update `ArenaRun.RequestPlayerMovement` to use the returned direction and
   distance.
4. Update `ArenaBoundsTests`: replace the four `PermittedTravel` tests and add
   sliding, pure-outward, and corner cases.
5. Add an aggregate-level test proving the movement intent handed to the adapter
   is tangential at the boundary.
6. Update `docs/architect/architecture/arena-combat/application-tick-contract.md`
   § Input.
7. Build and run the standalone Domain and Application suites; hand the Unity
   PlayMode consequence to `coder-role`.

## References

- `docs/architect/design-inbox/F-001.md` — FL-002, S-001
- `docs/design/features/F-001/ux-flows.md` — FL-002
- `docs/analyst/user-stories/US-004-arena-bounds-containment.md` — E2
- `docs/analyst/business-rules.md` — BR-030, BR-031, BR-033
- `Assets/_Project/Contexts/Arena/Domain/Geometry/ArenaBounds.cs`
- `Assets/_Project/Contexts/Arena/Domain/Aggregates/ArenaRun/ArenaRun.cs`
- `docs/architect/architecture/arena-combat/application-tick-contract.md`
