# ADR-0006: Use a sequential external interaction protocol for gameplay simulation

- **Status:** Accepted
- **Date:** 2026-09-13
- **Authors:** project team
- **Reviewers:** none
- **Related ADRs:** ADR-0002, ADR-0005

## Context

Unity physics, navigation, spawn positions, scene lifecycle, and input are external concerns. The legacy flat SimulationPlan attempts to request movement, attack, and spawn resolutions in one phase.

Some external queries depend on previous domain decisions. For example, a melee overlap must use the position after movement resolution, and a spawn identifier should not be allocated until the Domain has requested a spawn.

## Decision

Gameplay processing will use a sequential interaction protocol.

The Domain will produce typed interaction requests. The Application layer will resolve them through narrow ports. The Domain will consume typed resolutions and continue the aggregate operation.

The protocol will support correlation between a request, the aggregate revision that produced it, and the matching resolution.

## Rationale

- The Application layer must not duplicate combat, cooldown, or wave rules.
- External queries must be requested only after the Domain determines they are necessary.
- Dependent operations require prior resolutions.
- Typed requests and resolutions allow deterministic tests without Unity.
- Correlation protects the aggregate from stale or mismatched external results.

## Consequences

### Positive

- Q plus LMB can be processed correctly.
- Melee overlap can use the actual resolved player position.
- Spawn identity allocation occurs only when a spawn is requested.
- Unity physics remains external to the Domain.
- The protocol is suitable for replay and debug tracing.

### Negative

- One frame may involve several interaction stages.
- Application orchestration becomes more explicit.
- Batch query models are required for multiple enemy movement requests.

### Neutral / Risks

- Unity physics results are not inherently deterministic across platforms.
- The protocol must avoid asynchronous re-entry into an outdated ArenaRun revision.
- Performance must be profiled once enemy count grows.

## Alternatives

1. **Resolve all external queries before Domain processing** — forces Application to guess Domain requirements.
   - Why not chosen: it duplicates gameplay rules and produces incorrect same-frame behavior.
2. **Call Unity Physics directly from Domain** — couples the Domain to Unity.
   - Why not chosen: it breaks testability and dependency direction.
3. **Keep a single FrameContext** — cannot correctly express dependent interaction stages.
   - Why not chosen: the context becomes speculative and stale.

## References

- [Functional Core, Imperative Shell](https://www.destroyallsoftware.com/screencasts/catalog/functional-core-imperative-shell)
- [Vaughn Vernon, Implementing Domain-Driven Design](https://www.domainlanguage.com/ddd/reference/)