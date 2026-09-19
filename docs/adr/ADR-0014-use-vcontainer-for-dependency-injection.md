# ADR-0014: Use VContainer for dependency injection in CompositionRoot

- **Status:** Accepted
- **Date:** 2026-09-19
- **Authors:** Dmitry Rysev
- **Reviewers:** none
- **Related ADRs:** [ADR-0007](ADR-0007-unity-isolation-and-layer-dependencies.md), [ADR-0008](ADR-0008-code-style-and-static-usage-policy.md)

## Context

The game uses a two-scene architecture: a persistent BootstrapScene that lives in DontDestroyOnLoad and a reloadable ArenaScene. The application requires a dependency injection container for the Composition Root to wire Domain repositories, Application identity sources, Application sessions, Infrastructure adapters, and Presentation views.

Extenject (Zenject) was previously considered. It relies heavily on reflection-based injection, attribute decoration, and MonoInstaller hierarchies, which can blur the strict boundary between Domain, Application, and Presentation layers.

## Decision

The project will use VContainer as the dependency injection library in Game.CompositionRoot.

Registrations will use code-first configuration via LifetimeScope without attribute injection or container access in Domain or Application layers.

The persistent Bootstrap root scope will hold process-lifetime singletons: IArenaRunRepository, IArenaRunIdSource, IPlayerIdSource, IEnemyIdSource, and ArenaRunSession.

The ArenaScene scope will register per-scene adapters and views and will be re-created on each scene reload.

Neither Domain nor Application will reference VContainer.

## Rationale

- VContainer uses pure C# code-first registration and minimal reflection overhead.
- Explicit lifetime scopes match the two-stage lifecycle: root scope survives reload; scene scope is re-created.
- VContainer does not require decorating business classes with injection attributes.
- Eliminates MonoInstaller boilerplate across views and presenters.
- Compatible with Unity isolation (ADR-0007) and explicit dependencies policy (ADR-0008).

## Consequences

### Positive

- Clean separation between process-lifetime and scene-lifetime dependencies.
- Zero container coupling inside Domain and Application layers.
- Fast, AOT-friendly dependency resolution.
- Explicit, centralized registration in Game.CompositionRoot.

### Negative

- Developers familiar only with Zenject must learn VContainer LifetimeScope conventions.
- Certain Zenject-specific convenience patterns (e.g. contextual binding, convention-based binding) are not available.

### Neutral / Risks

- Unity package dependency on VContainer must be added to the project manifest before T-8.
- Scene transition wiring requires explicit LifetimeScope parenting.

## Alternatives

1. **Use Extenject (Zenject)** — mature and widely used, but introduces reflection overhead, attribute coupling, and complex installer hierarchies.
   - Why not chosen: VContainer provides a cleaner, lighter fit for code-first layer isolation.
2. **Pure manual dependency injection without a container** — feasible, but creates high maintenance cost as view factories, adapters, and handlers increase.
   - Why not chosen: VContainer handles scope disposal and lifetime management reliably.

## References

- [VContainer Documentation](https://vcontainer.hadashikick.jp/)
- [ADR-0007: Isolate Unity from Arena Combat Domain and Application layers](ADR-0007-unity-isolation-and-layer-dependencies.md)