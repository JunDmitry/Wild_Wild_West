# Migration Boundary

## Dependency Direction During Migration

```text
Legacy production
    does not reference Game.Arena.*

Game.Arena.Domain
    does not reference legacy assemblies

Game.Arena.Application
    does not reference legacy assemblies

Game.Arena.Infrastructure
    does not reference legacy assemblies

Game.Arena.Presentation
    does not reference legacy assemblies

Game.Legacy.Characterization.Tests
    may reference legacy assemblies

Game.Architecture.Tests
    validates the assembly graph
```

## Production Cut-Over Boundary

The legacy and target gameplay models do not exchange runtime state.

No adapter translates legacy GameState into ArenaRun.

No adapter translates ArenaRun into legacy GameState.

The main gameplay scene remains legacy-driven until T-9.

The new Arena Combat implementation is validated through Domain tests, Application tests, Infrastructure tests, Presentation tests, and ArenaSandbox.

## CompositionRoot Rule

Before T-9:

- Game.CompositionRoot does not reference legacy gameplay assemblies;
- ArenaSandbox may use a dedicated new CompositionRoot;
- main gameplay continues to use the legacy bootstrap path.

During T-9:

- main gameplay is switched to the new CompositionRoot;
- legacy gameplay remains in the repository but is not part of the active production path.

During T-10:

- legacy references are removed;
- legacy assemblies and tests are retired.

## Forbidden Migration Shortcuts

- sharing a common assembly between legacy and Game.Arena;
- importing legacy Domain types into Game.Arena.Domain;
- exposing legacy state through new Presentation;
- exposing ArenaRun to legacy Presentation;
- synchronizing both gameplay models during one frame;
- maintaining two sources of gameplay truth.
