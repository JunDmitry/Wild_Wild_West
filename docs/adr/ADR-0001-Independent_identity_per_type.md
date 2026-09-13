# ADR-0001: Independent Monotonic Identity Sources Per Domain Identity Type

- **Status**: Accepted
- **Date**: 2026-09-13
- **Authors**: Dmitry Rysev
- **Reviewers**: none
- **Related ADRs**: none

## Context

The Arena Combat bounded context introduces typed domain identities:

- ArenaRunId
- PlayerId
- EnemyId

A previous decision proposed a shared process-wide monotonic numeric sequence for all identity types. This would create values such as ArenaRunId(1), PlayerId(2), EnemyId(3).

The project requires independent identifier sources for each identity type. The sources must not share counters, must not be reset during ArenaScene reload, and must not reuse allocated values.

## Decision

The system will use independent process-lifetime monotonic identity sources:

- IArenaRunIdSource allocates ArenaRunId values.
- IPlayerIdSource allocates PlayerId values.
- IEnemyIdSource allocates EnemyId values.

Each source owns an independent sequence starting from its own initial value.

The sources are registered in the Bootstrap root scope and survive ArenaScene reloads.

The Domain layer receives already allocated typed identities and never generates identities itself.

Numeric equality between different identity types has no semantic meaning. ArenaRunId(1), PlayerId(1), and EnemyId(1) are distinct domain values.

## Consequences

- Identity allocation remains outside the Domain layer.
- The Application layer owns identity allocation orchestration.
- Each aggregate and entity receives a strongly typed identity.
- Unity registries must use typed identity keys.
- Physics hit resolution for enemy targeting returns EnemyId values only.
- Reloading ArenaScene creates a new ArenaRunId and PlayerId while preserving all source counters.
- Previously implemented IEntityIdSource and MonotonicEntityIdSource are legacy migration components and will be retired during T-2.
- No identifier source supports reset, release, reuse, peek, or commit operations.

## Alternatives Considered

### Shared process-wide identity sequence

Rejected because independent identity source ownership is required and numeric ordering across different domain identity types has no business value.

### Generic IIdentitySource<TIdentity>

Not selected by default because domain-specific source contracts better express ubiquitous language and produce clearer dependency graphs for the current number of identity types.

### Resettable identity sources per ArenaScene

Rejected because identifiers must remain monotonic across ArenaScene reloads and must never be reused during one application process lifetime.