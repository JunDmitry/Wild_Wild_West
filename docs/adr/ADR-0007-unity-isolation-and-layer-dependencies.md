# ADR-0007: Isolate Unity from Arena Combat Domain and Application layers

- **Status:** Accepted
- **Date:** 2026-09-13
- **Authors:** Dmitry Rysev
- **Reviewers:** none
- **Related ADRs:** [ADR-0002](ADR-0002-adopt-iddd-arena-combat.md), [ADR-0006](ADR-0006-sequential-external-interaction-protocol.md)

## Context

The game uses Unity for rendering, physics, input, scenes, navigation, UI, audio, and object lifetime. These technologies must not become dependencies of gameplay rules or application use cases.

The project needs compile-time enforcement of dependency direction.

## Decision

The project will use separate assembly definitions for Game.Arena.Domain, Game.Arena.Application, Game.Arena.Infrastructure, Game.Arena.Presentation, and Game.CompositionRoot.

Game.Arena.Domain and Game.Arena.Application will not reference UnityEngine or UnityEditor.

Unity APIs will be used only in Infrastructure, Presentation, and CompositionRoot.

Presentation will not reference Infrastructure directly.

## Rationale

- Assembly definitions enforce dependency rules at compilation time.
- Domain tests can run without Unity scenes or PlayMode.
- Presentation remains replaceable and does not own gameplay state.
- Infrastructure adapters can change without changing aggregate rules.
- CompositionRoot remains the only place where concrete dependencies are assembled.

## Consequences

### Positive

- Unity coupling is contained.
- Domain and Application tests are fast and deterministic.
- Dependency violations are visible during compilation.
- Presentation cannot become a hidden application service.

### Negative

- More assembly definitions and references must be maintained.
- Mapping between Unity types and domain types is required.
- Developers cannot call Unity APIs from Application convenience code.

### Neutral / Risks

- CompositionRoot naturally has broad knowledge and must remain thin.
- Scene-bound dependencies require explicit lifecycle management.
- Infrastructure adapters need PlayMode integration tests.

## Alternatives

1. **Use one Unity assembly for all gameplay code** — makes dependency direction unenforceable.
   - Why not chosen: gameplay rules would drift into MonoBehaviour classes.
2. **Allow Presentation to call Infrastructure directly** — creates horizontal coupling.
   - Why not chosen: CompositionRoot and Application ports should own the connection.
3. **Reference UnityEngine from Application for convenience** — makes core use cases Unity-dependent.
   - Why not chosen: it prevents clean testing and technology replacement.

## References

- [Unity Assembly Definitions](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html)
- [Clean Architecture](https://www.oreilly.com/library/view/clean-architecture-a/9780134494272/)