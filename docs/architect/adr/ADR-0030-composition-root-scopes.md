# ADR-0030: Compose the Arena graph in a persistent root scope and a per-scene child scope

- **Status:** Accepted
- **Date:** 2026-10-25
- **Authors:** architect-role
- **Reviewers:** none
- **Related ADRs:** [ADR-0007](ADR-0007-unity-isolation-and-layer-dependencies.md), [ADR-0008](ADR-0008-code-style-and-static-usage-policy.md), [ADR-0014](ADR-0014-use-vcontainer-for-dependency-injection.md), [ADR-0027](ADR-0027-typed-result-dispatcher.md), [ADR-0028](ADR-0028-live-enemy-collider-registration.md), [ADR-0029](ADR-0029-scene-reload-through-application-port.md)

## Context

ADR-0014 chose VContainer and a two-scope lifecycle: a persistent Bootstrap
root scope for process-lifetime singletons and a per-scene Arena scope that is
re-created on every reload. Until now nothing implements that decision —
`Game.CompositionRoot` contains only an `AssemblyMarker`, and the tick graph is
assembled by hand in the PlayMode harness.

T-8.1 requires "every Arena dependency registered in one composition root; no
scene object resolves a service through a global object search". The graph is
larger than the harness suggests: `ArenaRunTickServiceDependencies` takes twelve
collaborators, some process-lifetime (repository, identity sources, session) and
some scene-bound (input, camera, player transform, enemy transforms, colliders).
No configuration assets exist — every `ArenaDefinition`, `PlayerDefinition`,
weapon, enemy, and wave value is constructed in code today. Finally,
`IEnemyTransformRegistry` has no production implementation; only test fakes
exist.

The material questions are where the lifetime boundary falls, how a scene scope
receives Unity objects without a locator, and where configuration lives until a
settings asset exists.

## Decision

We will implement the two scopes ADR-0014 describes in `Game.CompositionRoot`,
with a single registration catalogue shared by both:

- `BootstrapLifetimeScope` is the persistent root. It marks its GameObject
  `DontDestroyOnLoad` and registers the process-lifetime graph: the
  code-defined `ArenaGameConfiguration`, `IArenaRunRepository`,
  `IArenaRunIdSource`, `IPlayerIdSource`, `IEnemyIdSource`, `MovementPathPolicy`,
  `ArenaRunFactory`, `ArenaRunSnapshotMapper`, `ArenaRunSessionDependencies`,
  `ArenaRunSession`, and `IRunRestartDelayPolicy`.
- `ArenaSceneLifetimeScope` is the child scope. It registers the scene graph:
  frame time source and game clock, input source, movement and targeting
  resolvers, collider registry (as both `IEnemyColliderRegistry` and
  `IEnemyColliderRegistration`), enemy movement and spawn placement, scene
  reloader, spawn pacing policy, `ArenaRunResultDispatcher` (as
  `IArenaRunDispatch` and as itself), `ArenaRunTickServiceDependencies`, and
  `ArenaRunTickService`.
- The scene scope receives an explicit `ArenaSceneObjects` seam — camera, player
  `Transform`, `IPlayerInputSource`, `IEnemyTransformRegistry`, and the
  configuration — through `Bind` before `Build`. Its `autoRun` is false, and a
  missing seam throws at scope construction. No `FindObjectOfType`, singleton,
  or static locator is used.
- Configuration is code-defined in `ArenaGameConfiguration.CreateDefault()`.
  The type is the only place the default arena, player, weapons, enemies, waves,
  restart delay, spawn interval, and arena scene name are constructed; a later
  settings asset replaces its body without touching registrations.
- `IEnemyTransformRegistry` is supplied by the seam until a production adapter
  exists; the registry's ownership belongs with the scene binder (T-8.2).

Explicitly out of scope for this decision: scene assets, view placement,
presenter registration on the dispatcher, and starting the initial run (T-8.2
and T-8.3).

## Options considered

### Option A — Root scope + registration catalogue (chosen)
- Pros: matches the T-8.1 substage boundary; no scene assets are needed; the
  root graph is testable without a scene; T-8.2 only adds the binder.
- Cons: scene-scoped resolution waits for T-8.2; configuration lives in code
  until a settings asset exists.
- Why chosen: it delivers the composition mechanism now without inventing scene
  content, and keeps the scene binder a separate, reviewable step.

### Option B — Full two-scope graph + scene binder
- Pros: end-to-end proof of lifetimes, reload, and dispatch in one task.
- Cons: needs BootstrapScene and ArenaScene assets and view placement that do
  not exist; mixes T-8.1–T-8.3; large review and rollback surface.
- Why not chosen: it front-loads scene authoring into an architecture task.

### Option C — Registration library only
- Pros: smallest code surface; purely code-first.
- Cons: no root scope exists; `DontDestroyOnLoad` semantics are untested;
  T-8.1's exit criterion is met only on paper.
- Why not chosen: it does not actually compose anything.

## Consequences

### Positive

- One place names every Arena dependency, and lifetime mistakes surface at
  container build rather than at the first tick.
- Domain and Application still reference neither Unity nor VContainer.
- The scene binder becomes a small adapter over an explicit seam, not a search.

### Negative / trade-offs

- The dispatcher is registered as itself as well as `IArenaRunDispatch`, because
  presenters must register handlers before registration closes at the first
  tick.
- Scene-scoped registrations cannot be resolved until T-8.2 supplies
  `ArenaSceneObjects`.
- Default configuration values (arena size, damage, restart delay, spawn
  interval) are duplicated from the PlayMode harness until a settings asset
  exists; the harness stays independent and unchanged.

### Neutral

- `ArenaSceneLifetimeScope` sets `autoRun` false; T-8.2 must call `Bind` then
  `Build`. A missing binding fails at scope construction.
- `IEnemyTransformRegistry` remains without a production adapter; the seam
  carries the gap explicitly instead of hiding it behind a locator.

## Compliance with paradigm

- ADR-0007 and `docs/architect/paradigm.md`: CompositionRoot is the only assembly
  that references all four layers; no other layer references it, and Domain and
  Application remain Unity-free.
- ADR-0014: two VContainer scopes, code-first registration, no attribute
  injection, no container access in Domain or Application.
- ADR-0027: the dispatcher is composed once in the scene scope; handler
  registration stays with the binder that owns the presenters.
- ADR-0029: the reloader receives the arena scene name from configuration.

## Migration plan

1. Record this ADR and index it.
2. Add `ArenaGameConfiguration`, `ArenaSceneObjects`, and `ArenaRegistrations`.
3. Add `BootstrapLifetimeScope` and `ArenaSceneLifetimeScope`.
4. Compile in Unity batchmode and commit the architect part.
5. Hand the PlayMode composition fixture to `coder-role`.
6. T-8.2 binds scene objects and registers presenters; T-8.3 starts the run.

## References

- `docs/coder/delivery-plan.md` § T-8.1
- `docs/architect/adr/ADR-0014-use-vcontainer-for-dependency-injection.md`
- `Assets/_Project/CompositionRoot/Game.CompositionRoot.asmdef`
- `Assets/_Project/Contexts/Arena/Application/Ticks/ArenaRunTickServiceDependencies.cs`
- `Assets/_Project/Contexts/Arena/Tests/GameArenaPlayMode/PlayModeTickHarness.cs`
- `Assets/_Project/Contexts/Arena/Infrastructure/`
- `Library/PackageCache/jp.hadashikick.vcontainer@5401e5a7eb/`
