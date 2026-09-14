# ADR-0010: Correlate domain interactions with aggregate revision

- **Status:** Accepted
- **Date:** 2026-09-14
- **Authors:** project team
- **Reviewers:** none
- **Related ADRs:** [ADR-0003](ADR-0003-arena-run-aggregate-and-repository.md), [ADR-0006](ADR-0006-sequential-external-interaction-protocol.md)

## Context

Arena Combat requires external systems to resolve movement, attacks, navigation, and spawning. Some resolutions may arrive after the Arena Run has already changed.

Applying a stale, duplicated, mismatched, or cross-run resolution can corrupt aggregate state.

Application must not own the identity of interactions initiated by the Domain because interaction lifecycle belongs to the Arena Run protocol.

## Decision

ArenaRun will maintain a monotonically increasing AggregateRevision and a local monotonically increasing InteractionId sequence.

Every external Interaction Request will contain ArenaRunId, InteractionId, and the AggregateRevision at which the request was created.

InteractionId will be allocated by ArenaRun and will not use the process-wide entity identity sequence.

Every Interaction Resolution will be accepted only through ArenaRun API.

ArenaRun will validate ArenaRunId, InteractionId, AggregateRevision, expected interaction type, expected stage, and pending interaction state before applying a resolution.

Every accepted state-changing aggregate operation advances AggregateRevision exactly once.

Read-only operations, rejected operations, and creation of an interaction request without state change do not advance AggregateRevision.

Rejected, stale, mismatched, and duplicate resolutions do not mutate aggregate state, advance AggregateRevision, or produce Domain Events.

One enemy movement batch is treated as one interaction and therefore uses one InteractionId. Individual entries in the batch are correlated by EnemyId.

## Rationale

- ArenaRun is the consistency boundary and must validate resolutions that can mutate it.
- AggregateRevision provides optimistic concurrency protection against stale resolutions.
- InteractionId distinguishes multiple interactions created at the same aggregate revision.
- ArenaRunId prevents resolutions from crossing aggregate instances.
- Domain-owned InteractionId keeps protocol identity with the component that owns interaction lifecycle.
- Explicit validation supports asynchronous adapters, replay, diagnostics, and deterministic tests.

## Consequences

### Positive

- Stale resolutions cannot mutate newer aggregate state.
- Duplicate resolutions cannot repeat domain transitions.
- Cross-run resolutions are rejected.
- Application does not duplicate domain protocol state.
- Interaction failures can be represented explicitly and tested.

### Negative

- ArenaRun must maintain pending interaction metadata.
- Interaction request and resolution types require correlation fields.
- Sequential gameplay processing becomes more explicit.

### Neutral / Risks

- Long-running asynchronous interactions may become stale frequently if unrelated state-changing operations advance the aggregate revision.
- Batch interactions require strict validation of contained entity identities.
- A future relaxation of strict aggregate revision matching would require a new ADR.

## Alternatives

1. **Validate only InteractionId** — does not prevent a valid but stale interaction from mutating a newer aggregate revision.
   - Why not chosen: interaction identity and aggregate version protect different consistency concerns.
2. **Generate InteractionId in Application** — moves domain protocol ownership outside the aggregate.
   - Why not chosen: ArenaRun initiates and validates interaction lifecycle.
3. **Accept resolutions without correlation** — relies on ordering assumptions of external systems.
   - Why not chosen: Unity and future asynchronous adapters do not provide the required aggregate consistency guarantee.

## References

- [Vaughn Vernon, Implementing Domain-Driven Design](https://www.domainlanguage.com/ddd/reference/)
- [Optimistic Offline Lock](https://martinfowler.com/eaaCatalog/optimisticOfflineLock.html)