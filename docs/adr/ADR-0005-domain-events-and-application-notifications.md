# ADR-0005: Use domain events and application notifications instead of simulation result flags

- **Status:** Accepted
- **Date:** 2026-09-12
- **Authors:** project team
- **Reviewers:** none
- **Related ADRs:** ADR-0002, ADR-0003

## Context

The legacy simulation result uses boolean flags and collections of facts such as PlayerDefeated, EnemySpawned, WaveStarted, and GamePhaseChanged. This primitive state encoding does not express event semantics clearly and is difficult to extend safely.

The project requires typed result handling for presentation, audio, effects, UI, analytics, and scene lifecycle without introducing a universal EventBus into the Domain.

## Decision

ArenaRun will record typed domain events for meaningful gameplay facts.

The Application layer will convert committed domain events into application notifications.

SimulationResult will be removed after the migration of all consumers.

Presentation, audio, VFX, analytics, and debug systems will receive application notifications through typed handler interfaces and an Application-owned dispatcher.

## Rationale

- Domain events represent facts that happened in the gameplay model.
- Typed events remove boolean-oriented result encoding.
- Application notifications avoid direct Domain dependencies from Unity presentation.
- A typed dispatcher is clearer and safer than a reflection-based EventBus.
- GameState-like snapshots remain the source for continuous display state.

## Consequences

### Positive

- Significant gameplay facts receive explicit domain names.
- Presentation does not access mutable aggregates.
- Result handling becomes extensible without modifying a giant result DTO.
- Events can support replay, analytics, and debugging later.
- State synchronization and one-shot notifications are clearly separated.

### Negative

- More event and notification types are required.
- Event ordering must be explicitly defined and tested.
- Mapping domain events to application notifications adds a translation layer.

### Neutral / Risks

- Continuous movement updates should use snapshots, not per-frame domain events.
- Notification handlers must not mutate ArenaRun.
- Handler failures must not cause domain operations to execute again.

## Alternatives

1. **Keep SimulationResult boolean flags** — simple initially but weakly typed and difficult to evolve.
   - Why not chosen: the current result model already shows primitive state encoding problems.
2. **Use a universal EventBus in Domain** — couples the Domain to a delivery mechanism.
   - Why not chosen: event transport is an Application and Infrastructure concern.
3. **Let Views poll and infer every event from snapshots** — one-shot events can be missed or reconstructed ambiguously.
   - Why not chosen: defeat, attack, and victory need explicit occurrence semantics.

## References

- [Domain Events](https://martinfowler.com/eaaDev/DomainEvent.html)
- [Vaughn Vernon, Implementing Domain-Driven Design](https://www.domainlanguage.com/ddd/reference/)