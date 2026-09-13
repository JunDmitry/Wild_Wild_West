# ADR-0002: Adopt IDDD for the Arena Combat bounded context

- **Status:** Accepted
- **Date:** 2026-09-12
- **Authors:** Dmitry Rysev
- **Reviewers:** none
- **Related ADRs:** [ADR-0001](ADR-0001-Independent_identity_per_type.md)

## Context

The project is a Unity 3D arena game where one player fights waves of enemies and bosses. The original implementation used data-only state objects, static rule classes, a central simulation procedure, and boolean-oriented simulation results. This structure does not provide explicit aggregate boundaries, domain invariants, domain events, or a sufficiently expressive ubiquitous language.

The project must preserve Unity as an external presentation and infrastructure technology while making the gameplay model independently testable and maintainable.

## Decision

The Arena Combat gameplay will be implemented using an IDDD-oriented architecture.

The Arena Combat bounded context will contain domain aggregates, entities, value objects, domain policies, domain services, repositories, domain events, application services, application notifications, and infrastructure adapters.

Unity APIs will not be referenced from the Domain or Application layers.

## Rationale

- The game contains meaningful business rules around waves, bosses, defeat, victory, combat, and entity lifecycle.
- Aggregate boundaries are needed to protect consistency rules.
- Domain events express meaningful gameplay facts without coupling the Domain to Unity views.
- The approach allows Unity rendering and physics technologies to change without changing gameplay rules.
- The architecture supports future save/load, replay, analytics, multiplayer, and ECS integration.

## Consequences

### Positive

- Gameplay rules are expressed using domain language.
- Aggregate invariants become explicit and testable.
- Unity remains outside the gameplay domain.
- Application orchestration is separated from domain state changes.
- The model can evolve without introducing a procedural god service.

### Negative

- The codebase will contain more domain types and explicit boundaries.
- The current Core.Model and Core.Rules implementation must be migrated gradually.
- Developers must maintain ubiquitous language and aggregate discipline.

### Neutral / Risks

- Not every gameplay calculation should become an entity method.
- High-frequency operations require profiling after the architecture is established.
- Aggregate boundaries may need revision if multiplayer or hundreds of enemies are introduced.

## Alternatives

1. **Keep the current Functional Core and Imperative Shell implementation** — it does not sufficiently express aggregate invariants, domain events, or bounded context language.
   - Why not chosen: it would preserve an anemic model and a central procedural simulation service.
2. **Put all gameplay logic into MonoBehaviour classes** — it couples rules to Unity and reduces testability.
   - Why not chosen: Unity components are presentation and infrastructure details.
3. **Adopt ECS as the core gameplay architecture immediately** — ECS is not a replacement for strategic domain modeling.
   - Why not chosen: it would solve performance concerns prematurely while leaving domain boundaries unclear.

## References

- [Vaughn Vernon, Implementing Domain-Driven Design](https://www.domainlanguage.com/ddd/reference/)
- [Domain-Driven Design Reference](https://www.domainlanguage.com/ddd/reference/)