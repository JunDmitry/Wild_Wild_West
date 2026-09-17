# ADR-0004: Use typed process-wide monotonic identity sources

- **Status:** Superseded by ADR-0001
- **Date:** 2026-09-12
- **Authors:** project team
- **Reviewers:** none
- **Related ADRs:** [ADR-0002](ADR-0002-adopt-iddd-arena-combat.md), [ADR-0003](ADR-0003-arena-run-aggregate-and-repository.md)

> **Superseded.** This record duplicated a decision already made in ADR-0001 and resolved it differently by proposing a single process-wide sequence shared by all identity types. The project uses an independent monotonic sequence per identity type as decided in ADR-0001. The typed identity value objects and the separate identity source contracts described here remain in force through ADR-0001.

## Context

The game creates ArenaRun, Player, and Enemy entities across multiple arena scene reloads. Unity instance identifiers and pooled GameObject identities cannot be used as domain identities.

A generic EntityId does not express domain intent and allows accidental misuse between aggregate and entity types.

## Decision

The system will use ArenaRunId, PlayerId, and EnemyId value objects.

The Application layer will expose separate contracts: IArenaRunIdSource, IPlayerIdSource, and IEnemyIdSource.

All identity sources will use one process-wide monotonic sequence. Identifiers will not be reset, reused, released, or derived from Unity objects.

## Rationale

- Typed identities prevent category mistakes in domain APIs.
- A shared monotonic sequence guarantees uniqueness during one process lifetime.
- Scene reloads do not affect domain identity allocation.
- Application remains the owner of identity allocation.
- Domain entities receive identities but do not generate them.

## Consequences

### Positive

- Domain APIs become self-documenting.
- ArenaRunId, PlayerId, and EnemyId cannot be confused accidentally.
- Entity identities remain stable independently of Unity object pooling.
- Future replay and network synchronization have an explicit identity policy.

### Negative

- More value object types and conversion points are required.
- Infrastructure registries must map Unity objects to typed domain identifiers.
- Existing EntityId-based tests and legacy code must be migrated.

### Neutral / Risks

- The internal monotonic sequence must handle exhaustion explicitly.
- Domain reload in Unity Editor is not treated as a process-lifetime guarantee.
- Persistence identifiers may require a separate policy in the future.

## Alternatives

1. **Keep one shared EntityId everywhere** — technically simpler but weak in domain expressiveness.
   - Why not chosen: it permits accidental cross-category identifier usage.
2. **Generate identities inside Domain entities** — violates Application ownership of identity allocation.
   - Why not chosen: identity generation is not an aggregate business rule.
3. **Use Unity InstanceID values** — Unity object lifetime does not match domain entity lifetime.
   - Why not chosen: pooled and recreated GameObjects would make identity unstable.

## References

- [Vaughn Vernon, Implementing Domain-Driven Design](https://www.domainlanguage.com/ddd/reference/)