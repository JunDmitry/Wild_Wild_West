# ADR-0003: Use ArenaRun as the aggregate root and repository target

- **Status:** Accepted
- **Date:** 2026-09-12
- **Authors:** project team
- **Reviewers:** none
- **Related ADRs:** [ADR-0002](ADR-0002-adopt-iddd-arena-combat.md)

## Context

A gameplay attempt contains one player, one current wave, active enemies, boss progression, defeat state, and victory state. These concepts participate in the same consistency rules.

The project requires a repository contract even before durable persistence is introduced. The repository must represent a collection of aggregate roots rather than an ORM abstraction.

## Decision

ArenaRun will be the aggregate root of the Arena Combat bounded context.

Player, Enemy, and Wave will be entities or internal domain concepts within the ArenaRun aggregate and will not have independent repositories.

The Domain layer will define IArenaRunRepository as a collection-like repository contract for ArenaRun aggregate roots.

The first implementation will be InMemoryArenaRunRepository in Infrastructure.

## Rationale

- Player defeat, enemy defeat, boss progression, wave completion, and victory must remain consistent within one transaction boundary.
- ArenaRun has a natural lifecycle corresponding to a single gameplay attempt.
- A repository for the aggregate root matches IDDD repository semantics.
- A collection-like interface avoids leaking ORM concepts into the Domain.
- In-memory storage supports the bootstrap scene lifecycle without premature persistence infrastructure.

## Consequences

### Positive

- Aggregate invariants have a single owner.
- Application services operate on ArenaRun rather than raw GameState data.
- Repository responsibilities remain narrow and domain-specific.
- Future persistence can replace the in-memory implementation.
- Presentation never receives mutable aggregate references.

### Negative

- ArenaRun may become large if the number of active enemies grows significantly.
- Repository-backed application services require explicit aggregate retrieval.
- Durable persistence will require a future transaction and persistence design.

### Neutral / Risks

- The in-memory repository returns managed aggregate references.
- Completed runs must be removed or archived according to an explicit lifecycle policy.
- Multiplayer may require revisiting aggregate boundaries.

## Alternatives

1. **Create separate repositories for Player, Enemy, and Wave** — breaks the aggregate consistency boundary.
   - Why not chosen: these entities do not have independent lifecycle requirements in the current game.
2. **Use a generic IRepository<TEntity>** — hides domain intent and encourages CRUD-oriented design.
   - Why not chosen: the repository must be specific to ArenaRun.
3. **Do not introduce a repository before persistence exists** — removes a useful aggregate collection boundary.
   - Why not chosen: the active run lifecycle already benefits from explicit repository semantics.

## References

- [Vaughn Vernon, Implementing Domain-Driven Design](https://www.domainlanguage.com/ddd/reference/)
- [Eric Evans, Domain-Driven Design](https://www.domainlanguage.com/ddd/)