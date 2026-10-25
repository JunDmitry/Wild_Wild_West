# ADR-0031: Bind the Arena scene explicitly and drive the tick from a scene entry point

- **Status:** Accepted
- **Date:** 2026-10-25
- **Authors:** architect-role
- **Reviewers:** none
- **Related ADRs:** [ADR-0014](ADR-0014-use-vcontainer-for-dependency-injection.md), [ADR-0027](ADR-0027-typed-result-dispatcher.md), [ADR-0028](ADR-0028-live-enemy-collider-registration.md), [ADR-0030](ADR-0030-composition-root-scopes.md), [ADR-0007](ADR-0007-unity-isolation-and-layer-dependencies.md), [ADR-0008](ADR-0008-code-style-and-static-usage-policy.md)

## Context

ADR-0030 built the two VContainer scopes but deliberately left the Arena scene seam unfilled: `ArenaSceneLifetimeScope` refuses to build until someone supplies `ArenaSceneObjects`, and no scene asset exists. Nothing in production drives `ArenaRunTickService`; only the PlayMode harness does. The presenters are scene components with `Present` methods that do not implement any dispatcher handler interface, and `IEnemyTransformRegistry` has no production implementation at all.

T-8.2's exit criterion is that the scene binder resolves the tick service and the presenters, and that a missing dependency fails loudly at startup. Two contract gaps surfaced while designing it. First, the dispatcher accepted snapshot handlers only at stages 1–2, so the snapshot-driven gameplay-visual presenters had no stage to register at. Second, no handler ever received the tick's HUD model, so the HUD presenters had no route to their input. ADR-0027 is amended for both.

## Decision

We will bind the Arena scene through one explicit binder in `Game.CompositionRoot`, adapt presentation to the dispatcher in composition, and drive ticks from one scene-scope entry point.

- `ArenaSceneBinder` is a `MonoBehaviour` on the Arena scene with serialized references only — the scene scope, the camera, the player transform, the input source, the enemy view registry, and every presenter. `Awake` validates all of them and throws naming the missing field; `Start` builds. No `Find`, no singleton, no service lookup.
- The binder sets `ParentReference.Create<BootstrapLifetimeScope>()` on the scene scope. That is VContainer's own typed cross-scene link (ADR-0014); the persistent bootstrap scope is resolved by type and must already be built, otherwise `Build` throws. Nothing resolves a service through a global search.
- `ArenaSceneObjects` carries Unity scene objects only: camera, player transform, input source, enemy transform registry. Configuration leaves the seam — `ArenaGameConfiguration` is a process-lifetime registration in the root scope, and the scene registrations resolve it from the parent container when an adapter is constructed. There is exactly one configuration instance.
- Presentation is not changed. `ArenaSnapshotHandler` and `ArenaHudHandler` give a presenter's `Present` method the `IArenaRunSnapshotHandler` and `IArenaRunHudHandler` shapes. The binder registers camera, player, and enemies at `EntityViewSynchronization`; the aim, melee, attack-cue, boss-entry, and notification presenters at `GameplayVisual`; the health, equipped-weapon, wave, boss, and defeat-window presenters at `Hud`.
- The binder wires `EnemyViewRegistry.ViewCreated`/`ViewDestroyed` to the scene scope's `IEnemyColliderRegistration` (ADR-0028), and resolves `ArenaRunTickService` eagerly so a missing dependency throws before the first frame.
- `EnemyViewTransformRegistry` implements the Infrastructure `IEnemyTransformRegistry` over `EnemyViewRegistry.TryGetView` and `EnemyView.Body`. Composition is the only layer that may see both sides: Infrastructure must not reference Presentation (ADR-0007), and Presentation must not reference Infrastructure.
- `ArenaTickDriver` is a scene-scope entry point (`RegisterEntryPoint`, VContainer's `ITickable`). It ticks once per frame and does nothing while `ArenaRunSession.HasActiveRun` is false. Starting the run is T-8.3, not the driver; the driver never creates or restarts a run.
- Registration order is fixed: the binder registers every handler during `Start`, before the first `Update`, and the dispatcher closes registration at its first dispatch.

Out of scope: the BootstrapScene and ArenaScene assets, object placement, prefab wiring, and the PlayMode binder fixture. Those are handed to `coder-role`.

## Options considered

### Option A — Binder + entry-point driver + handler adapters + minimal scenes
- Pros: no Presentation, Application, or Domain edit beyond the dispatcher; ADR-0027 stages preserved; every dependency resolved eagerly; the production enemy-transform seam finally exists.
- Cons: four new CompositionRoot types and a thin adapter layer.
- Chosen.

### Option B — Presenters implement the handler contract themselves
- Pros: no adapter layer.
- Cons: about twelve Presentation files and their PlayMode fixtures change; Presentation takes on an orchestration role it does not have; large T-7 regression surface.

### Option C — Binder only, proven in a PlayMode-built scene
- Pros: smallest surface.
- Cons: "fails loudly at startup" is never exercised in a real scene; scenes, driver, and registry move to T-8.3.

## Consequences

### Positive

- One object wires the Arena scene, and every one of its dependencies is visible on it.
- Presentation keeps its `Present` shape and stays free of dispatcher and container knowledge.
- The HUD model reaches its handlers, and gameplay visuals have a stage they can register at.
- The frame loop has one owner, and it cannot start a run by itself.
- A missing scene reference, an absent bootstrap scope, or a missing registration throws before gameplay.

### Negative

- CompositionRoot now references Presentation types (view registry, presenters). The assembly already referenced Presentation; the new coupling is concrete types, not new assemblies.
- The binder's serialized field list is long and must track presenter changes.
- `ArenaSceneObjects` lost its configuration member, so the composition fixture constructs it differently.

### Neutral / Risks

- The parent scope is located by type through VContainer's `FindAnyObjectByType`. That is a typed parent link, not a service search, but it does rely on exactly one `BootstrapLifetimeScope` being alive.
- Handler registration happens in `Start`; a scene object that ticks before `Start` would race it. Nothing in the scene ticks before `Start` today.
- A future presenter needs a line in the binder; the compiler will not demand it.

## Compliance with paradigm

- ADR-0007: no Unity type enters Domain or Application; CompositionRoot remains the only assembly that bridges Presentation and Infrastructure.
- ADR-0008: no new static state; the parent link is a typed serialized reference resolved by the container.
- ADR-0014: the two-scope lifecycle and code-first registration are unchanged.
- ADR-0027: stages, ordering, registration-closing, and failure isolation are unchanged; the payload kinds widen per the amendment.
- ADR-0030: the catalogue remains the single registration place; the seam loses configuration to the root scope.
- `docs/architect/paradigm.md`: no deviation.

## Migration steps

1. Amend ADR-0027 for snapshot and HUD handlers at every stage.
2. Add `IArenaRunHudHandler` and widen the dispatcher and its tests.
3. Remove configuration from `ArenaSceneObjects`; resolve `ArenaGameConfiguration` from the parent in the scene registrations.
4. Add `EnemyViewTransformRegistry`, `ArenaSnapshotHandler`, `ArenaHudHandler`, `ArenaTickDriver`, and `ArenaSceneBinder`.
5. Register the driver as a scene-scope entry point.
6. Compile and run the composition fixture; hand the scenes and the binder fixture to `coder-role`.

## References

- `docs/coder/delivery-plan.md` § T-8.2
- `docs/architect/adr/ADR-0030-composition-root-scopes.md`
- `docs/architect/adr/ADR-0027-typed-result-dispatcher.md`
- `Assets/_Project/CompositionRoot/ArenaSceneBinder.cs`
- `Assets/_Project/CompositionRoot/ArenaTickDriver.cs`
- `Assets/_Project/Contexts/Arena/Application/Dispatch/ArenaRunResultDispatcher.cs`
