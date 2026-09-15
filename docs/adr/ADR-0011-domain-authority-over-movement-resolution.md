# ADR-0011: Keep domain authority over movement resolution and require explicit interaction cancellation

- **Status:** Accepted
- **Date:** 2026-09-15
- **Authors:** project team
- **Reviewers:** none
- **Related ADRs:** [ADR-0006](ADR-0006-sequential-external-interaction-protocol.md), [ADR-0010](ADR-0010-correlate-domain-interactions-with-aggregate.md)

## Context

Player movement is resolved by external systems such as Unity physics or navigation. The first implementation accepted any resolved position inside the Arena, which allowed an adapter to relocate the Player arbitrarily, and closed the pending interaction when a resolution payload was invalid, which contradicted the rule that rejected resolutions have no side effects.

Unattended pending interactions also need a defined way to be closed when adapters fail or the application lifecycle supersedes them.

## Decision

A Player Movement Request expresses domain intent as source position, direction, and requested distance already limited by Arena Bounds along the direction. The requested position is derived and not stored.

The Arena Run accepts a movement resolution only when the accepted position lies on the requested movement path: not behind the origin, not beyond the requested distance, and not laterally displaced beyond geometric tolerance. The external world may shorten a movement but cannot extend, redirect, or relocate it.

Rejected resolutions, including those with an invalid payload, do not mutate aggregate state, do not advance Aggregate Revision, do not produce Domain Events, and do not close the pending interaction.

A pending interaction is closed only by successful application or by explicit cancellation through the Arena Run API with a stated reason. Cancellation does not advance Aggregate Revision and does not produce Domain Events.

Wall sliding is not part of this contract and requires a future decision.

## Rationale

- The Domain must remain the authority over how far and where the Player may move.
- Infrastructure resolves collisions; it does not define gameplay intent.
- Side-effect-free rejection keeps the interaction protocol predictable and retryable.
- Explicit cancellation makes interaction lifecycle observable and testable instead of implicit.
- Deriving the requested position removes duplicated data from the request.

## Consequences

### Positive

- Adapters cannot teleport the Player.
- Invalid adapter answers cannot destroy a valid pending interaction.
- Interaction lifecycle has three explicit exits: apply, cancel, or remain pending.
- Movement contracts are fully testable without Unity.

### Negative

- Sliding along obstacles cannot be expressed as a valid resolution yet.
- Application lifecycle must decide when a pending interaction is cancelled.
- Additional value objects and validation code exist in the Domain.

### Neutral / Risks

- A misbehaving adapter can leave an interaction pending until cancellation.
- Geometric tolerances are part of the contract and must be kept stable.
- Extending the contract to sliding will require a new ADR.

## Alternatives

1. **Accept any position inside the Arena** — allows adapters to relocate the Player.
   - Why not chosen: the Domain loses authority over movement.
2. **Close the pending interaction on invalid payload** — violates the side-effect-free rejection rule.
   - Why not chosen: a corrupted answer could cancel a legitimate interaction.
3. **Allow wall sliding immediately** — requires a richer path model before the first playable slice.
   - Why not chosen: it is deferred until real locomotion integration exists.

## References

- [Vaughn Vernon, Implementing Domain-Driven Design](https://www.domainlanguage.com/ddd/reference/)