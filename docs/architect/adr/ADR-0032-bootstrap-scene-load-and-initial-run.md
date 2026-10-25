# ADR-0032: Load the Arena scene from bootstrap and create the initial run on scene entry

- **Status:** Accepted
- **Date:** 2026-10-25
- **Authors:** architect-role
- **Reviewers:** none
- **Related ADRs:** [ADR-0014](ADR-0014-use-vcontainer-for-dependency-injection.md), [ADR-0029](ADR-0029-scene-reload-through-application-port.md), [ADR-0030](ADR-0030-composition-root-scopes.md), [ADR-0031](ADR-0031-scene-binder-and-frame-driver.md), [ADR-0007](ADR-0007-unity-isolation-and-layer-dependencies.md)

## Context

T-8.3's exit criterion is that entering the arena from bootstrap creates exactly one active run with fresh identities, while entering it after a defeat reload presents the run the tick already restarted and creates nothing (ADR-0029). Neither half exists today.

`ArenaRunSession.StartInitialRun()` already allocates fresh identities and returns `ActiveRunAlreadyExists` when a run is live — but it has no production call site; only tests and `PlayModeTickHarness` call it. `ArenaTickDriver` (ADR-0031) deliberately skips while `HasActiveRun` is false, so an unstarted arena renders nothing. On the other side, `BootstrapScene` carries only `BootstrapLifetimeScope`, nothing loads `ArenaScene`, and `ArenaSceneBinder` throws when no live bootstrap parent exists. The only `SceneManager` caller is the defeat-reload adapter in the scene scope (ADR-0029).

## Decision

We will create the initial run from a scene-scope startup entry point, and load the Arena scene once from a root-scope bootstrap entry point through the existing scene-load port.

- `ArenaRunStartupDriver` is an `IStartable` entry point in `ArenaSceneLifetimeScope`, registered in the catalogue before `ArenaTickDriver`. It resolves `ArenaRunSession` and calls `StartInitialRun()`. `InitialRunStartStatus.Started` is the fresh-entry path; `ActiveRunAlreadyExists` is the defeat-reload path and is stated explicitly: the driver creates nothing and the tick's first dispatch presents the restarted run. Any other status throws.
- `ArenaBootstrapEntryPoint` is an `IStartable` entry point in `BootstrapLifetimeScope`. It resolves `IArenaSceneReloader` and calls `ReloadArenaScene()` once, loading the configured `ArenaSceneName`. It is registered by `BootstrapLifetimeScope.Configure` behind a serialized `LoadArenaOnStart` flag (on by default), not by the shared catalogue: VContainer dispatches entry points on every `Build()`, so a catalogue-level entry point would fire inside the composition fixtures that build the process graph. The catalogue stays side-effect free, and an in-memory scope rig turns the flag off so no test performs a destructive scene load.
- `IArenaSceneReloader` moves from the scene catalogue to the process catalogue. The adapter has no scene dependencies (it calls `SceneManager` with a configuration value), and both callers — the bootstrap and `DefeatRestartStage` — need it; the scene scope resolves the same singleton from the parent scope (ADR-0030).
- Both entry points are ordinary catalogue registrations. No scene object, serialized field, or object search is added; the scene assets are unchanged.
- Out of scope: a menu or return path, identity reset, and a rename of the port's method. A later non-arena scene that must clear a run is a new decision.

## Rationale

- **Why scene entry, not process start.** The session is process-lifetime, so a run created at bootstrap would exist for every later scene too. Tying creation to arena entry keeps the run's lifetime equal to the arena's use and leaves room for a menu without inheriting a live run.
- **Why the existing port.** `IArenaSceneReloader` already means "load the configured arena scene"; the bootstrap's first load and the defeat reload are the same operation with different triggers. Reusing it keeps the only `SceneManager` call in Infrastructure, as ADR-0029 requires, and makes the load testable with a recording fake.
- **Why it is exactly once.** `StartInitialRun` is idempotent by contract: it reports `ActiveRunAlreadyExists` rather than creating a second run. The defeat reload therefore cannot create one because the tick has already restarted it. The bootstrap scope builds once, so the scene load fires once.
- **Why the reloader becomes process-lifetime.** A registration scope should follow the dependency, not the caller: the adapter depends only on configuration, and both scopes need the same instance. Duplicating the registration in both scopes would create two adapters with the same behaviour for no gain.

## Options considered

### Option A — scene-side startup + shared reload port (chosen)
`ArenaRunStartupDriver` in the scene scope; `ArenaBootstrapEntryPoint` in the root scope; the reloader moves to the process graph.
- Pros: run creation follows arena entry; both paths provable with a recording port; Application, Domain, Presentation, and assets untouched; one port, one adapter.
- Cons: two new types; a registration moves between scopes; the bootstrap entry point needs a scene-side opt-out so scope fixtures do not load a scene.

### Option B — bootstrap owns run creation
One root-scope entry point creates the run and loads the scene.
- Pros: one entry point, smallest type count.
- Cons: a live run exists for the whole process regardless of scene; the delivery-plan criterion is phrased per scene entry; a future menu inherits a run it did not ask for.

### Option C — the scene binder creates the run
`ArenaSceneBinder.BuildScope` calls `StartInitialRun()` after building; only the bootstrap loader is new.
- Pros: smallest diff.
- Cons: ADR-0031 defines the binder as a wiring object with serialized references; creation ordering becomes implicit inside it and is harder to prove in isolation.

## Consequences

### Positive

- Pressing Play on the bootstrap scene reaches a playable arena with no manual wiring.
- A fresh process creates exactly one run, with identities from the process-lifetime sources.
- A defeat reload creates nothing and presents the run the tick restarted.
- Both behaviours are testable without performing a destructive scene load.

### Negative

- `IArenaSceneReloader` is now registered process-lifetime even though the tick consumes it from the scene scope; readers must follow the parent resolution.
- The port's name says "reload" while the bootstrap uses it to load the first scene.
- `LoadArenaOnStart` exists so in-memory scopes stay inert; a future rig that forgets to turn it off will load the arena scene during a test, which fails loudly rather than silently.
- A wrong `ArenaSceneName` fails at load time in Unity, not at composition.

### Neutral / Risks

- `IStartable` runs when the scope builds: the startup driver runs inside `ArenaSceneBinder.Start`'s `Build()`, before the binder registers dispatch handlers. Run creation dispatches nothing, and the first tick is next frame, so the order is safe but must stay true.
- A second ArenaScene load in the same process that is not a defeat reload presents the existing run instead of starting a new one. That is deliberate; an explicit "new game" flow must clear the session first.

## Compliance with paradigm

- ADR-0007: the scene load stays in Infrastructure behind an Application port; no Unity type enters Application or Domain.
- ADR-0014: the persistent root scope starts the arena scene; the scene scope builds behind the binder.
- ADR-0029: the port keeps its meaning and its only `SceneManager` implementation; the defeat reload is unchanged.
- ADR-0030: one catalogue; the reloader registration moves to the scope that matches its dependencies.
- ADR-0031: the binder is untouched; entry points remain catalogue registrations.
- `docs/architect/paradigm.md`: no deviation.

## Migration plan

1. Record this ADR and index it.
2. Move the `IArenaSceneReloader` registration to `RegisterProcessLifetime`.
3. Add `ArenaBootstrapEntryPoint` and register it in `BootstrapLifetimeScope.Configure` behind `LoadArenaOnStart`.
4. Add `ArenaRunStartupDriver` and register it in the scene graph before `ArenaTickDriver`.
5. Compile in Unity batchmode and run `ArenaCompositionTests`.
6. Hand the PlayMode coverage to `coder-role`.

## References

- `docs/coder/delivery-plan.md` § T-8.3
- `docs/architect/adr/ADR-0029-scene-reload-through-application-port.md`
- `docs/architect/adr/ADR-0031-scene-binder-and-frame-driver.md`
- `Assets/_Project/Contexts/Arena/Application/Sessions/ArenaRunSession.cs`
- `Assets/_Project/CompositionRoot/ArenaRegistrations.cs`
- `Assets/_Project/CompositionRoot/BootstrapLifetimeScope.cs`
