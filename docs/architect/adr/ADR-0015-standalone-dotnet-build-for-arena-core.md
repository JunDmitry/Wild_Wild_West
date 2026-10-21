# ADR-0015: Build the Arena core as a standalone .NET project while keeping its sources in Assets

- **Status:** Accepted
- **Date:** 2026-10-21
- **Deciders:** Dmitry Rysev
- **Related:** [ADR-0002](ADR-0002-adopt-iddd-arena-combat.md), [ADR-0007](ADR-0007-unity-isolation-and-layer-dependencies.md), [ADR-0016](ADR-0016-bounded-context-and-documentation-layout.md), task `arch-init-1`

## Context

ADR-0007 forbids `Game.Arena.Domain` and `Game.Arena.Application` from referencing `UnityEngine` or `UnityEditor`, and the assembly definition graph enforces that rule at compile time. The enforcement is real, but it only runs inside the Unity Editor: every `.csproj` in the repository is Unity-generated, and the Domain and Application projects therefore cannot be built or tested without Unity. The architecture tests that should prove isolation are themselves EditMode tests running inside the Editor.

The project is a training project on architectural design. Demonstrating that the core is independent of Unity — not only by a dependency rule but by an actual build and test run outside the Editor — is a primary goal. The core uses only the BCL, C# 9, and no external packages; its tests use only NUnit. Unity 2022.3.46f1 remains the integration and PlayMode environment.

## Decision

> We will add a standalone .NET build for `Game.Arena.Domain` and `Game.Arena.Application` under `src/`, and standalone NUnit test projects under `tests/`, while keeping the source files in `Assets/_Project/Contexts/Arena/` and letting Unity compile the same files through the existing asmdefs.

Consequences:

- `src/Game.Arena.Domain/Game.Arena.Domain.csproj` and `src/Game.Arena.Application/Game.Arena.Application.csproj` target `netstandard2.1`, disable default compile items, and include the Assets sources through linked globs.
- `tests/Game.Arena.Domain.Tests` and `tests/Game.Arena.Application.Tests` target `net9.0`, use NUnit plus the standard test SDK, link the existing test sources, and keep assembly names that satisfy `InternalsVisibleTo`.
- A separate `Wild_Wild_West.Core.sln` contains the four projects; the Unity-generated `Wild_Wild_West.sln` and root `.csproj` files are left to Unity.
- No source is duplicated and no source moves out of `Assets/`.
- The standalone build is the fast proof of Unity independence; Unity remains the environment for Infrastructure, Presentation, PlayMode, and architecture tests.
- Explicitly out of scope: moving the core outside `Assets/`, NuGet packaging or versioned consumption, and standalone builds for Infrastructure or Presentation.

## Options considered

### Option A — Linked-source standalone build (chosen)
- Pros: proves the core builds and tests without Unity; no duplication; Unity iteration is unchanged; one source of truth.
- Cons: a second solution and build configuration to maintain; the test sources must stay Unity-compatible.
- Why chosen: it delivers the training goal at the lowest structural cost and keeps the migration path intact.

### Option B — Move the core sources outside Assets
- Pros: strongest physical boundary; cleanest repository layout.
- Cons: every core change needs a build-and-copy step; PDBs are needed for debugging; the T-4 migration pauses.
- Why not chosen: the architectural boundary is already achieved by dependency direction; the physical move adds friction without changing the dependency graph.

### Option C — Keep asmdef isolation only
- Pros: zero cost.
- Cons: the core still cannot build or test without the Editor; the isolation claim stays unverifiable outside Unity.
- Why not chosen: it fails the goal of proving Unity independence.

### Option D — Full separate solution and NuGet packages
- Pros: maximum isolation and reuse.
- Cons: versioning overhead, a second repository or release process, slow iteration.
- Why not chosen: disproportionate for a single training project.

## Consequences

### Positive

- The core can be built and tested by `dotnet build` and `dotnet test` in seconds.
- Unity independence is demonstrated by execution, not only by asmdef configuration.
- CI can run the core tests without a Unity licence or Editor.

### Negative / trade-offs

- Two build systems describe the same sources; a change that compiles in one may fail in the other.
- The linked-glob paths must be updated if the context folder moves again.
- The standalone tests do not cover Unity adapters.

### Neutral

- Test sources must avoid Unity-only APIs; the one unused `UnityEngine.Profiling` import is removed by this task.

## Compliance with paradigm

- Enforces ADR-0007 and the Clean Architecture layer rules in `docs/architect/paradigm.md`.
- No deviation; no exception entry is needed.

## Migration plan

1. Move the Arena context under `Assets/_Project/Contexts/Arena/` (ADR-0016).
2. Add `src/Game.Arena.Domain` and `src/Game.Arena.Application` with linked compile items.
3. Add `tests/Game.Arena.Domain.Tests` and `tests/Game.Arena.Application.Tests`.
4. Add `Wild_Wild_West.Core.sln`.
5. Run `dotnet build` and `dotnet test` until green.
6. Open Unity once and run the Architecture plus Arena test suites to confirm the Editor path still compiles.

Each step leaves both build paths green.

## References

- Related code: `Assets/_Project/Contexts/Arena/Domain`, `Assets/_Project/Contexts/Arena/Application`, `Assets/_Project/Contexts/Arena/Tests`
- Related docs: `docs/architect/paradigm.md`, `docs/architect/adr/ADR-0007-unity-isolation-and-layer-dependencies.md`
- External sources: [.NET Standard](https://learn.microsoft.com/dotnet/standard/net-standard), [Unity Assembly Definitions](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html)
