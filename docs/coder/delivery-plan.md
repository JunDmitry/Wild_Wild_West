# Arena Combat Delivery Plan

## Purpose

This document is the authoritative implementation plan for the Arena Combat bounded context.

The plan is updated only at explicit planning or refactoring milestones.

Completed work remains recorded for traceability. Future work must follow the order defined here unless a new ADR or an explicit refactoring stage changes the plan.

Status vocabulary:

- **Completed** — delivered and covered by tests.
- **In Progress** — partly delivered; substage status says what remains.
- **Planned** — not started.

Substage status is `Done` or `Open`. An `Open` substage carries its own exit criteria.

## Current Architecture

The Arena Combat stack is the active gameplay stack; legacy gameplay assemblies are frozen and still power the old scene until cut-over (T-9).

The primary aggregate root is ArenaRun.

ArenaRun owns:

- one Player;
- the current Wave;
- active Enemy entities;
- aggregate lifecycle state;
- aggregate revision;
- pending external interaction protocol state.

The Domain layer owns gameplay consistency rules.

The Application layer owns use-case orchestration, identity allocation, repository usage, external interaction resolution, session lifecycle, read models, HUD copy, and notification dispatch.

Infrastructure adapts Unity systems: input, player and enemy movement, attack targeting, and spawn placement.

Presentation renders snapshots and consumes Application notifications through typed handlers registered on the result dispatcher (ADR-0027).

CompositionRoot owns the VContainer composition: the persistent root scope, the arena scene scope, one registration catalogue, and the scene binder that wires the Arena scene (ADR-0030, ADR-0031).

## Completed Stages

### T-0. Migration Foundation

Status: Completed

Delivered:

- ADR baseline;
- assembly dependency boundaries;
- architecture tests;
- StyleCop guardrails;
- legacy characterization tests;
- legacy defect register;
- legacy freeze policy;
- migration boundary;
- cut-over strategy.

### T-1. Arena Combat Domain Model

Status: Completed

Delivered:

- ArenaRun aggregate root;
- Player, Enemy, and Wave aggregate entities;
- typed identities;
- AggregateRevision;
- InteractionId;
- sequential interaction protocol;
- movement requests and resolutions;
- enemy spawn requests and resolutions;
- player attack start and impact;
- enemy attack start and impact;
- enemy movement batch;
- Health, DamageAmount, DamageApplication;
- wave progression;
- Victory;
- Defeat;
- typed Domain Events;
- deterministic EnemyId processing;
- Domain unit tests.

### T-2. Repository and Application Identity Integration

Status: Completed

Delivered:

- T-2.1: `IArenaRunRepository` in Domain. — Done
- T-2.2: `IArenaRunIdSource`, `IPlayerIdSource`, and `IEnemyIdSource` in Application. — Done
- T-2.3: `InMemoryArenaRunRepository` in Infrastructure. — Done
- T-2.4: `ArenaRunSession` in Application. — Done
- T-2.5: Repository and session tests. — Done

Exit criteria:

- Application stores ActiveArenaRunId, not ArenaRun state fragments. — Met
- ArenaRun is retrieved through IArenaRunRepository. — Met
- InMemoryArenaRunRepository behaves as a collection of aggregate roots. — Met
- Identity sources are independent, monotonic, process-lifetime services. — Met
- No Unity dependency appears in Domain or Application. — Met

### T-3. Application Interaction Orchestration

Status: Completed

Goal:

Coordinate existing ArenaRun operations and resolve external interactions without duplicating Domain rules.

Delivered:

- T-3.1: PlayerFrameInput, granular ports, spawn pacing contract, tick result contract, and stage order.
- T-3.2: Player movement and Player attack stages.
- T-3.3: Enemy movement and spawn stages; FixedIntervalSpawnPacingPolicy.
- T-3.4: Interrupted-step recovery, interaction cancellation, and error handling.
- T-3.5: Complete ArenaRunTickCoordinator and ordered-step tests.

Gameplay order:

1. Pending interaction recovery.
2. Advance Game Time.
3. Apply Player weapon switching.
4. Resolve and apply Player movement.
5. Start a requested Player attack.
6. Resolve a due Player attack impact.
7. Resolve and apply Enemy movement.
8. Start eligible Enemy attacks.
9. Resolve due Enemy attack impacts.
10. Attempt Enemy spawn according to Application pacing.

Interrupted-interaction recovery precedes normal gameplay stages.

Terminal ArenaRun status stops further combat stages.

The coordinator returns Domain Events and the final revision; it does not dispatch notifications.

A tick is not a transaction over all executed aggregate operations.

Detailed contract:

../architect/architecture/arena-combat/application-tick-contract.md

Exit criteria:

- Application does not duplicate ArenaRun gameplay rules. — Met
- Resolutions are constructed from matching Domain request correlations. — Met
- Rejected interactions stop the corresponding phase. — Met
- Pending interactions cannot leak into the next gameplay step. — Met
- Adapter failures preserve previously produced Domain Events. — Met
- Terminal ArenaRun stops remaining gameplay stages. — Met
- Tick results preserve event occurrence order and event revisions. — Met
- Spawn pacing resets when the active ArenaRun changes. — Met
- Identity sources remain process-lifetime services and are never reset. — Met
- Domain, Application, and architecture tests are green. — Met

### T-4. Read Models and Application Notifications

Status: Completed

Goal:

Provide immutable read models for Presentation and transform Domain Events into Application notifications.

Delivered:

- T-4.1: Define ArenaRunSnapshot. — Done
- T-4.2: Define PlayerSnapshot, EnemySnapshot, WaveSnapshot, and combat snapshots. — Done. Combat state is carried on the entity snapshots as `HasPendingAttack` and `PendingAttack`; there are no separate combat snapshot types.
- T-4.3: Define Application notifications. — Done: victory, enemy damaged, and enemy attack resolved, plus the attack-outcome value type.
- T-4.4: Implement Domain Event to Application Notification mapping. — Done.
- T-4.5: Add snapshot and notification tests. — Done.

Exit criteria:

- Presentation never receives ArenaRun directly. — Met
- Presentation never receives Player or Enemy entities directly. — Met
- Continuous state is rendered from snapshots. — Met
- One-shot facts are delivered through notifications. — Met

### T-5. Typed Result Dispatcher

Status: Completed

Goal:

Dispatch the tick result through explicit typed handlers in six ordered pipeline stages (ADR-0027).

Delivered:

- T-5.1: Define dispatcher pipeline stages. — Done (`ArenaRunDispatchStage`, six ordered values).
- T-5.2: Define typed notification handler contracts. — Done (`IArenaRunSnapshotHandler`, `IArenaRunNotificationHandler<TNotification>`).
- T-5.3: Implement deterministic handler ordering between stages. — Done (`ArenaRunResultDispatcher`: stage order, publication order, registration order).
- T-5.4: Define handler error policy. — Done (isolate and report through `IArenaRunDispatchDiagnostics`; the tick survives).
- T-5.5: Add dispatcher tests. — Done (Application routing/ordering/error fixtures plus the PlayMode `DispatchPipelineOrderTests`).
- T-5.6: Migrate Presentation off the notification sink. — Done (`ArenaRunNotificationPresenter` is a handler set; `PlayModeTickHarness` takes an `ArenaRunResultDispatcher`; the sink and publisher are deleted).

Pipeline stages:

1. Snapshot publication.
2. Entity view synchronization.
3. Gameplay visual notifications.
4. HUD notifications.
5. Audio and VFX notifications.
6. Analytics and diagnostics notifications.

Exit criteria:

- Snapshot publication precedes entity view synchronization, which precedes gameplay, HUD, audio/VFX, and analytics delivery. — Met
- Each one-shot fact reaches every registered handler exactly once per tick. — Met
- A handler failure follows the documented policy without losing the tick result or the Domain Events. — Met
- Application references no Presentation type. — Met
- Existing notification-sink call sites are either migrated or recorded as an adapter with a removal follow-up. — Met (migrated; the sink is gone)

### T-6. Unity Infrastructure

Status: Completed

Goal:

Implement Unity adapters for Application ports.

Substages:

- T-6.1: Unity clock adapter. — Done (`FrameDeltaGameClock` over `IFrameTimeSource`; `UnityFrameTimeSource` reads `Time.unscaledDeltaTime`, game-owned scale and pause).
- T-6.2: Unity input adapter. — Done (`UnityPlayerInputSource` over `IUnityInputDevice`).
- T-6.3: Unity player movement resolver. — Done (`UnityPlayerMovementResolver`).
- T-6.4: Unity ranged attack resolver. — Done (`UnityPlayerAttackTargetingResolver.ResolveRanged`).
- T-6.5: Unity melee attack resolver. — Done (`UnityPlayerAttackTargetingResolver.ResolveMelee`).
- T-6.6: Unity enemy movement resolver. — Done (`UnityEnemyMovementResolver` over `IEnemyTransformRegistry`).
- T-6.7: Unity spawn point provider. — Done (`UnityEnemySpawnPlacementResolver`).
- T-6.8: Collider identity registry. — Done (`EnemyColliderRegistry` implements `IEnemyColliderRegistry` and `IEnemyColliderRegistration` over a two-direction collider↔`EnemyId` map, populated from `EnemyViewRegistry` lifecycle callbacks (ADR-0028); unknown, null, and destroyed colliders resolve to `EnemyId.None`; 9-test PlayMode fixture including the live seam).
- T-6.9: Unity scene lifecycle adapter. — Done (`UnityArenaSceneReloader` over `SceneManager.LoadScene` in single mode; `DefeatRestartStage` requests the reload once through `IArenaSceneReloader` after a restart; 4-test PlayMode fixture proves exactly one reload; ADR-0029).
- T-6.10: Infrastructure PlayMode tests. — Done for the delivered adapters; each new adapter carries its own PlayMode coverage.

Substage exit criteria:

- T-6.1: an `IGameClock` implementation advances from Unity frame time, never wall-clock time; a PlayMode test proves monotonic advance across frames. — Met
- T-6.8: an `IEnemyColliderRegistry` implementation populated from live enemy views; an unknown collider resolves to `EnemyId.None`; a PlayMode test covers registration, unregistration, and lookup. — Met
- T-6.9: an Application port for scene reload (defeat delay and restart) implemented with `SceneManager`; a PlayMode test proves a defeated run triggers the configured reload exactly once. — Met

Exit criteria:

- Every Application port consumed by the tick service has a Unity adapter. — Met
- No adapter decides gameplay rules; it supplies data or applies an already-made decision. — Met
- Adapters are constructible without a running scene and are covered by PlayMode tests. — Met
- Domain and Application still reference no Unity type. — Met

### T-7. Unity Presentation

Status: Completed

Goal:

Render ArenaRun snapshots and consume Application notifications.

Delivered:

- T-7.1: PlayerView and PlayerPresenter. — Done
- T-7.2: EnemyView and EnemyViewRegistry. — Done; a view exists only while the enemy is inside the arena bounds in the published snapshot.
- T-7.3: HUDView. — Done; one presenter per HUD region, rendering Application-provided values without formatting.
- T-7.4: Third-person camera controller. — Done.
- T-7.5: Attack, damage, death, and spawn visual handlers. — Done. Attack is covered by the firearm aim marker, the melee swing, and the enemy attack cue; damage by hit confirmation; spawn by view creation and the entry announcement. Death is represented by view removal when the enemy leaves the snapshot; there is no separate death effect.
- T-7.6: VictoryView and DefeatView. — Done.
- T-7.7: Presentation PlayMode tests. — Done.

Exit criteria:

- Presentation never receives ArenaRun directly. — Met
- Presentation never receives Player or Enemy entities directly. — Met
- Continuous state is rendered from snapshots. — Met
- One-shot facts are delivered through notifications. — Met
- Presentation references Application and Domain read-model value types only (ADR-0020). — Met

## Current Position

T-5 is complete. The typed result dispatcher is delivered end to end: Application contracts, the six-stage dispatcher, the tick service dispatch, and the Presentation handler set with PlayMode order coverage.

T-6 is complete. Every Application port consumed by the tick service has a Unity adapter: the clock, input, player movement, both attack resolvers, enemy movement, spawn placement, the collider identity registry (ADR-0028), and the scene lifecycle adapter. `UnityArenaSceneReloader` calls `SceneManager.LoadScene` in single mode, and `DefeatRestartStage` requests the reload once through `IArenaSceneReloader` immediately after a defeated run restarts (ADR-0029); a 4-test PlayMode fixture proves exactly one reload at that edge and none otherwise.

T-8.1 and T-8.2 are complete. One VContainer composition root owns every Arena registration: `BootstrapLifetimeScope` holds the process-lifetime graph, `ArenaSceneLifetimeScope` registers the scene graph behind the explicit `ArenaSceneObjects` seam, and a single `ArenaRegistrations` catalogue plus a code-defined `ArenaGameConfiguration` feed both (ADR-0030). A 4-test PlayMode fixture proves both graphs resolve, that singletons are shared, and that a missing seam fails loudly. The Arena scene is now bound: `ArenaSceneBinder` supplies the scene objects, registers every presenter at its ADR-0027 dispatch stage, wires the enemy view registry to the collider registration port, and releases `ArenaTickDriver` as the single frame-loop entry point; a missing reference or dependency throws before gameplay (ADR-0031). `BootstrapScene`, `ArenaScene`, and `EnemyView.prefab` exist, and a 5-test PlayMode `ArenaSceneBinderTests` fixture proves the binder end to end. T-8.3 is complete: entering the Arena scene creates exactly one active run with fresh identities, and entering it after a defeat reload creates nothing and presents the restarted run (ADR-0032); both arena scenes are now in Build Settings, bootstrap first. T-8.4 is complete by composition of existing coverage: the defeat delay is the configured singleton policy, the reload is requested once at the restart edge, and the post-reload entry presents the already-restarted run (T-8.3). A narrow composition coverage task (`t8-4-defeat-reload`) landed as `ProcessGraph_PolicyWaitsTheConfiguredRestartDelay` and `SceneScope_ResolvesTheSameRestartDelayPolicyAsTheProcessGraph`: the configured delay reaches the registered policy and the scene scope resolves the same singleton. No production seam was added, so the defeat→reload edge keeps its Application and PlayMode coverage. T-8.5 is complete: victory freezes combat, shows the window once, and never reloads, with the window wired in the real Arena scene; its last clause (zero reloads on a win) is now proven by `ArenaSceneReloadTests.WinningTheRun_RequestsNoReload` — zero reloads at the victory tick and after 20 further terminal ticks (`t8-5-victory-lifecycle`, fixture 5/5). T-8.6 is complete: `ArenaSandboxSmokeTests` is one end-to-end PlayMode fixture through the real bootstrap→arena composition, seven scenarios covering new run, arena-bound movement, ranged hit and miss, melee against three enemies, wave progression, defeat with exactly one reload after the configured delay, and victory with zero reloads — 7/7 green in batchmode (`t8-6-arena-smoke`). No substage of T-8 remains open; T-8 is Completed and its four exit criteria are Met. The T-9 entry audit of 2026-10-25 leaves two gates open: the ADR-0033 performance measurement and the three migration smoke scenarios T-8.6 did not cover.

T-7 is complete as presentation code. Making the Arena scene playable is T-8, not T-7.

## Remaining Stages

### T-8. Bootstrap and Arena Session Lifecycle

Status: Completed

Goal:

Connect BootstrapScene, ArenaScene, Application Session, Repository, Unity adapters, and Presentation.

Substages:

- T-8.1: Bootstrap Composition Root with VContainer LifetimeScope. — Done (`Game.CompositionRoot` carries the code-first root: `BootstrapLifetimeScope` registers the process-lifetime graph and `ArenaSceneLifetimeScope` the scene graph behind the explicit `ArenaSceneObjects` seam, both fed by one `ArenaRegistrations` catalogue and a code-defined `ArenaGameConfiguration`; ADR-0030; the scene scope registers a production no-op dispatch diagnostics; a 4-test PlayMode `ArenaCompositionTests` fixture in the test-only `Game.CompositionRoot.PlayMode.Tests` assembly proves both graphs, singleton identity, and missing-seam failures).
- T-8.2: ArenaScene binder. — Done (`ArenaSceneBinder` on the Arena scene with eighteen serialized references, `ArenaTickDriver` as a scene-scope entry point, `EnemyViewTransformRegistry` bridging the Presentation view registry to the Infrastructure transform port, and thin `ArenaSnapshotHandler`/`ArenaHudHandler` adapters registering every presenter at its ADR-0027 stage; no scene object resolves through a global search, and a missing reference throws naming the field; ADR-0031. `BootstrapScene`, `ArenaScene`, and `EnemyView.prefab` assets; a 5-test PlayMode `ArenaSceneBinderTests` fixture proves scope construction, both failure guards, stage placement, and one end-to-end dispatch).
- T-8.3: ArenaRun creation flow. — Done (ADR-0032: `ArenaBootstrapEntryPoint` loads the configured Arena scene once through the process-lifetime `IArenaSceneReloader` behind `LoadArenaOnStart`, and `ArenaRunStartupDriver` in the scene scope calls `ArenaRunSession.StartInitialRun()`, treating `ActiveRunAlreadyExists` as the defeat-reload path; both scenes are registered in Build Settings, bootstrap first; `ArenaRunCreationTests` 4/4, `ArenaSceneBuildSettingsTests` 2/2, and the extended `ArenaSceneBinderTests` 6/6 prove the fresh-entry, already-active, post-restart, bootstrap-load, build-settings, and first-tick behaviours).
- T-8.4: Defeat delay and ArenaScene reload. — Done (the configured `RestartDelay` feeds the singleton `FixedIntervalRunRestartDelayPolicy`, reset whenever the active run id changes; `DefeatRestartStage` waits for it, restarts the run, and requests the reload once through `IArenaSceneReloader`; Application `DefeatRestartTickTests` + `FixedIntervalRunRestartDelayPolicyTests` and PlayMode `ArenaSceneReloadTests` 4/4 cover the delay and the single reload, and T-8.3 proved the post-reload entry presents the restarted run; composition coverage added as `ProcessGraph_PolicyWaitsTheConfiguredRestartDelay` and `SceneScope_ResolvesTheSameRestartDelayPolicyAsTheProcessGraph`).
- T-8.5: Victory lifecycle. — Done (a won run sets `ArenaRunStatus.Victory` and publishes `ArenaRunVictoryNotification` once; `VictoryView.Show` and `AnnounceVictoryOnce` are idempotent per run/revision, and the real Arena scene wires the window through the presenter's `_victoryView`; `VictoryWindowTests` 4/4 prove show-once, frozen combat, and ignored inputs, `ArenaSceneBinderTests` prove the dispatch path, and `DefeatRestartStage` runs only on `Defeat`, so no reload is requested. `ArenaSceneReloadTests.WinningTheRun_RequestsNoReload` proves a victorious run requests zero reloads — at the victory tick and through 20 further terminal ticks — closing the last clause (`t8-5-victory-lifecycle`).
- T-8.6: ArenaSandbox smoke scenarios. — Done (one end-to-end PlayMode fixture, `ArenaSandboxSmokeTests`, through the real `BootstrapScene → ArenaScene` composition: new run, arena-bound movement, ranged hit/miss, melee against three enemies, wave progression, defeat + exactly one reload after the configured delay, and victory with zero reloads. Scenarios 1–4 and 5 landed on the existing rig; 6–7 are test-side only on the composition seams `BootstrapLifetimeScope.Configuration`, `ArenaSceneObjects.Reloader`, and `ArenaSceneBinder.SceneObjectsOverride` — no production, scene, prefab, `.asmdef`, or package change; `ArenaSandboxSmokeTests` 7/7 in batchmode, `ArenaSceneBinderTests` 6/6 and `ArenaCompositionTests` 6/6 unchanged; `t8-6-arena-smoke`).

Substage exit criteria:

- T-8.1: VContainer is installed and every Arena dependency is registered in one composition root; no scene object resolves a service through a global object search. — Met
- T-8.2: the scene binder resolves the tick service and the presenters; a missing dependency fails loudly at startup. — Met
- T-8.3: entering the scene from bootstrap creates exactly one active run with fresh identities and no state carried over from a previous run; entering it after a defeat reload presents the active run the tick already restarted, and creates nothing (ADR-0029). — Met
- T-8.4: defeat waits the configured delay, requests the arena scene reload exactly once through the Application port (ADR-0029), and the reloaded scene presents the already-restarted run. — Met
- T-8.5: victory stops combat, shows the window once, and does not reload. — Met
- T-8.6: PlayMode smoke scenarios cover new run, movement, both attacks, wave progression, defeat, and victory. — Met

Exit criteria:

- Playing the Arena scene starts a run with no manual wiring. — Met (`ArenaBootstrapEntryPoint` + `ArenaRunStartupDriver`, proven by `ArenaRunCreationTests` 4/4 and the binder fixture; the manual play check is a T-9 gate)
- Both lifecycles complete: defeat reloads, victory does not. — Met (`ArenaSceneReloadTests` 5/5, `ArenaSandboxSmokeTests` defeat and victory scenarios)
- Arena runtime code references no legacy assembly. — Met (`Game.Arena.*` and `Game.CompositionRoot` asmdefs reference no `Game.Core.*`; only legacy and legacy-test assemblies do)
- Architecture tests remain green; Presentation still does not reference Infrastructure. — Met (`Game.Arena.Presentation.asmdef` references Application, Domain, and TextMeshPro only; the Arena PlayMode suite is 197/197 and the Infrastructure suite 10/10, audit of 2026-10-25)

### T-9. Production Cut-Over

Status: Planned

Goal:

Switch the main gameplay scene from the legacy stack to the Arena Combat stack.

Entry criteria (audited 2026-10-25; the QA artefacts were refreshed by `architect-role` as an exception because `qa-role` is not operating):

| Criterion | State | Evidence |
|---|---|---|
| B-001–B-011 covered by target tests | Met (B-011 reclassified) | `docs/qa/behavior-traceability.md` refreshed: every named test matched in source, B-011 now `Implemented in Application`, and four drifted names repointed to the live tests |
| Every legacy defect has a target status | Met | `docs/qa/legacy-defect-register.md` refreshed: rows whose target work landed in T-3–T-7 now read `Implemented in target; production cut-over pending`; the rest wait on T-9 or T-10 |
| Architecture, Domain, Application, Infrastructure, and Presentation tests green | Domain, Application, Infrastructure, and Presentation met; the architecture fixture stands on its last recorded green run | Domain 373/373, Application 215/215, Infrastructure EditMode 10/10, Arena PlayMode 197/197 — all exit 0 on 2026-10-25 |
| ArenaSandbox PlayMode smoke tests green | Met | `ArenaSandboxSmokeTests` 10/10, exit 0 on 2026-10-26 |
| Performance threshold defined and passed | Defined, not measured | ADR-0033 fixes the five thresholds, the measurement procedure, and the 10% legacy comparison; the measurement run is itself a T-9 entry task |
| Migration smoke scenarios (full set) | Met | All eight scenarios in `docs/architect/migration/criteria-for-switching-production-flow.md` are covered: the three missing ones (three-wave start→victory, enemy windup → evasion, enemy windup → defeat) landed in `ArenaSandboxSmokeTests` on 2026-10-26 (commit `f109da0`, fixture 10/10, exit 0) |

### T-10. Legacy Retirement

Status: Planned

Goal:

Remove legacy gameplay code after one validated release cycle.

Substages:

- remove legacy production assemblies;
- remove legacy unit tests;
- remove characterization tests;
- remove legacy defect reproduction tests;
- update defect register statuses;
- remove legacy references from CompositionRoot;
- verify no production assembly references legacy code.

## Mandatory Refactoring Checkpoints

A separate refactoring stage is required when one of the following occurs:

- a Domain type has more than one unrelated responsibility;
- Application duplicates a Domain decision;
- Infrastructure determines gameplay rules;
- Presentation mutates aggregate state;
- a public API exposes mutable aggregate internals;
- repeated code appears in two or more Domain policies;
- a collection has unclear ownership or mutable aliases;
- an interaction type becomes overloaded with unrelated fields;
- performance profiling identifies a hot path;
- a future feature invalidates an accepted assumption.

## Delivery Rules

- New features are implemented only in Game.Arena assemblies.
- Legacy gameplay remains frozen except for approved hotfixes.
- Every state-changing aggregate operation must have Domain tests.
- Every external interaction request/resolution pair must have correlation tests.
- Every new Domain Event must have ordering and revision tests.
- Every Unity adapter requires PlayMode coverage.
- Every architectural change requires either an ADR or an explicit entry in this plan.
