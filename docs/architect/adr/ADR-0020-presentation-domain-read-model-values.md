# ADR-0020: Let Presentation consume Domain read-model value types

- **Status:** Accepted
- **Date:** 2026-10-22
- **Authors:** architect-role
- **Reviewers:** none
- **Related ADRs:** [ADR-0007](ADR-0007-unity-isolation-and-layer-dependencies.md), [ADR-0003](ADR-0003-arena-run-aggregate-and-repository.md), [ADR-0005](ADR-0005-domain-events-and-application-notifications.md)

## Context

F-001 requires `PlayerPresenter` to place the player object at the position
published in the tick snapshot. The Application read models are typed with
Domain concepts end to end: `PlayerSnapshot` exposes `PlayerId`, `Position3D`,
`Health`, `WeaponKind`, `GameTimePoint`, and `PendingPlayerAttack`;
`EnemySnapshot` and `ArenaRunSnapshot` do the same.

`Game.Arena.Presentation` was allowed to reference only
`Game.Arena.Application`, enforced by `AssemblyBoundaryTests` and the asmdef.
C# requires the assembly that defines a type whenever that type is named
(CS0012), even when it arrives through a public Application API. The presenter
therefore could not read `PlayerSnapshot.Position` at all.

The coder raised this as a module-boundary blocker (c1b). Three ways out were
considered: a primitive position contract in Application, moving the presenter
to Infrastructure, or allowing Presentation to reference Domain.

## Decision

`Game.Arena.Presentation` may reference `Game.Arena.Domain`, permanently, for
one purpose: **consuming read models and immutable Domain value types.**

Allowed in Presentation:

- reading snapshot properties: `Position3D`, `Health`, `WeaponKind`,
  `GameTimePoint`, `PlayerId`, `EnemyId`, wave and boss enums;
- reading Domain value types passed through Application APIs.

Forbidden in Presentation:

- constructing or holding aggregates (`ArenaRun`), Domain factories,
  repositories, or Domain interaction requests/resolutions;
- opening or applying Domain interactions;
- containing gameplay rules (damage, range, cooldown, spawn, victory);
- referencing `Game.Arena.Infrastructure` or `Game.CompositionRoot`.

The reference set becomes:

| Assembly | May reference |
|----------|---------------|
| `Game.Arena.Presentation` | `Game.Arena.Application`, `Game.Arena.Domain` |

`AssemblyBoundaryTests` is updated to allow exactly that set, and the
Presentation asmdef adds `Game.Arena.Domain`. ADR-0007's rule that
Presentation never references Infrastructure is unchanged, and Domain and
Application still reference no Unity assembly.

## Rationale

- The snapshot is a read model of the aggregate; duplicating its value types
  in Application would double the geometry (and every health, time, and
  weapon type) for no isolation benefit.
- A primitive position contract would be a one-off: the next presenter
  concern (health bar, attack wind-up, weapon icon) re-opens the same
  question.
- Reading a value type is not a dependency on gameplay behaviour; the rule
  above keeps presenters as projections of the snapshot.
- The alternative that truly avoided the edge — moving presenters to
  Infrastructure — contradicts the role of the Presentation layer and the F-001
  design.

## Consequences

### Positive

- Presenters can render any snapshot value without conversion types.
- No new public Application contract; the tick contract stays as designed.
- The dependency direction stays acyclic: Presentation → Application → Domain,
  plus Presentation → Domain for values.

### Negative

- The boundary test no longer proves Presentation is Domain-free; the
  restriction above is enforced by review, not the compiler.
- A contributor could call Domain behaviour from a presenter without noticing
  a compile error.

### Neutral / Risks

- If Presentation later needs Domain behaviour, that is a new ADR — the
  allowed use stays read-only.
- PlayMode tests in `Game.Arena.PlayMode.Tests` reference the same assemblies
  (ADR-0018), so presentation tests can assert Domain-typed values directly.

## Alternatives

1. **Primitive read model in Application** — an Application type with X/Y/Z
   floats, or float properties on the snapshot.
   - Why not chosen: new public contract, conversion code, and it grows per
     presenter concern.
2. **Presenter in Infrastructure** —
   - Why not chosen: wrong layer, contradicts F-001, blurs adapter roles.

## Compliance with paradigm

- ADR-0007: Unity isolation is untouched — Domain and Application still have
  no Unity reference; the new edge points from a Unity assembly to a pure one.
- ADR-0003: Presentation still reads only snapshots, never the aggregate.
- `docs/architect/paradigm.md`: no deviation; the layering table in this ADR
  refines it.

## Migration plan

1. Add `Game.Arena.Domain` to `Game.Arena.Presentation.asmdef` references.
2. Allow it in the `Game.Arena.Presentation` row of
   `AssemblyBoundaryTests`.
3. Implement `PlayerView`/`PlayerPresenter` and the camera under F-001.
4. Keep `dotnet`-side builds unaffected: Presentation is Unity-only.

## References

- `docs/architect/coder-inbox/c1b.md`
- `docs/architect/tasks/c1b.md`
- `Assets/_Project/Contexts/Arena/Application/ReadModels/PlayerSnapshot.cs`
- `Assets/_Project/Tests/Architecture/AssemblyBoundaryTests.cs`
- `docs/coder/architect-inbox/F-001.md`
