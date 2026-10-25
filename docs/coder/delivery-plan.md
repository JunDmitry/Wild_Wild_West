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

Presentation renders snapshots and consumes Application notifications through the notification sink.

CompositionRoot does not wire anything yet; DI composition is T-8.

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

T-5 is in progress. The Application dispatcher, its contracts, and its Application tests are delivered; the Presentation handler extraction and the PlayMode migration are with `coder-role`.

After T-5, the remaining work is T-6.1, T-6.8, and T-6.9, then T-8, then T-9 and T-10.

T-7 is complete as presentation code. Making the Arena scene playable is T-8, not T-7.

## Remaining Stages

### T-5. Typed Result Dispatcher

Status: In Progress

Goal:

Dispatch the tick result through explicit typed handlers in six ordered pipeline stages (ADR-0027).

Substages:

- T-5.1: Define dispatcher pipeline stages. — Done (`ArenaRunDispatchStage`, six ordered values).
- T-5.2: Define typed notification handler contracts. — Done (`IArenaRunSnapshotHandler`, `IArenaRunNotificationHandler<TNotification>`).
- T-5.3: Implement deterministic handler ordering between stages. — Done (`ArenaRunResultDispatcher`: stage order, publication order, registration order).
- T-5.4: Define handler error policy. — Done (isolate and report through `IArenaRunDispatchDiagnostics`; the tick survives).
- T-5.5: Add dispatcher tests. — Done for Application (routing, stage order, registration order, no-handler, duplicate and late registration, isolated failure, diagnostics). PlayMode migration is part of the open handoff.

Remaining:

- Extract `ArenaRunNotificationPresenter` into handlers registered on the dispatcher and migrate `PlayModeTickHarness` and the PlayMode fixtures. Handed to `coder-role` (`docs/coder/architect-inbox/t5-dispatcher.md`).

Pipeline stages:

1. Snapshot publication.
2. Entity view synchronization.
3. Gameplay visual notifications.
4. HUD notifications.
5. Audio and VFX notifications.
6. Analytics and diagnostics notifications.

Substage exit criteria:

- T-5.1: the six pipeline stages exist as ordered Application contracts; no call site identifies a stage by string.
- T-5.2: every notification type has a typed handler contract, and registration is checked at compile time.
- T-5.3: handlers within a stage run in a documented deterministic order, proven stable across registration order by test.
- T-5.4: the policy states whether a failing handler stops the handler, the stage, or the tick; the tick result survives a handler failure and the failure is observable.
- T-5.5: routing, ordering, error policy, and the no-handler case are covered by Application tests.

Exit criteria:

- Snapshot publication precedes entity view synchronization, which precedes gameplay, HUD, audio/VFX, and analytics delivery.
- Each one-shot fact reaches every registered handler exactly once per tick.
- A handler failure follows the documented policy without losing the tick result or the Domain Events.
- Application references no Presentation type.
- Existing notification-sink call sites are either migrated or recorded as an adapter with a removal follow-up.

### T-6. Unity Infrastructure

Status: In Progress

Goal:

Implement Unity adapters for Application ports.

Substages:

- T-6.1: Unity clock adapter. — Open
- T-6.2: Unity input adapter. — Done (`UnityPlayerInputSource` over `IUnityInputDevice`).
- T-6.3: Unity player movement resolver. — Done (`UnityPlayerMovementResolver`).
- T-6.4: Unity ranged attack resolver. — Done (`UnityPlayerAttackTargetingResolver.ResolveRanged`).
- T-6.5: Unity melee attack resolver. — Done (`UnityPlayerAttackTargetingResolver.ResolveMelee`).
- T-6.6: Unity enemy movement resolver. — Done (`UnityEnemyMovementResolver` over `IEnemyTransformRegistry`).
- T-6.7: Unity spawn point provider. — Done (`UnityEnemySpawnPlacementResolver`).
- T-6.8: Collider identity registry. — Open. The `IEnemyColliderRegistry` contract exists; no production implementation exists.
- T-6.9: Unity scene lifecycle adapter. — Open.
- T-6.10: Infrastructure PlayMode tests. — Done for the delivered adapters; each new adapter carries its own PlayMode coverage.

Substage exit criteria:

- T-6.1: an `IGameClock` implementation advances from Unity frame time, never wall-clock time; a PlayMode test proves monotonic advance across frames.
- T-6.8: an `IEnemyColliderRegistry` implementation populated from live enemy views; an unknown collider resolves to `EnemyId.None`; a PlayMode test covers registration, unregistration, and lookup.
- T-6.9: an Application port for scene reload (defeat delay and restart) implemented with `SceneManager`; a PlayMode test proves a defeated run triggers the configured reload exactly once.

Exit criteria:

- Every Application port consumed by the tick service has a Unity adapter.
- No adapter decides gameplay rules; it supplies data or applies an already-made decision.
- Adapters are constructible without a running scene and are covered by PlayMode tests.
- Domain and Application still reference no Unity type.

### T-8. Bootstrap and Arena Session Lifecycle

Status: Planned

Goal:

Connect BootstrapScene, ArenaScene, Application Session, Repository, Unity adapters, and Presentation.

Substages:

- T-8.1: Bootstrap Composition Root with VContainer LifetimeScope. — Open
- T-8.2: ArenaScene binder. — Open
- T-8.3: ArenaRun creation flow. — Open
- T-8.4: Defeat delay and ArenaScene reload. — Open
- T-8.5: Victory lifecycle. — Open
- T-8.6: ArenaSandbox smoke scenarios. — Open

Substage exit criteria:

- T-8.1: VContainer is installed and every Arena dependency is registered in one composition root; no scene object resolves a service through a global object search.
- T-8.2: the scene binder resolves the tick service and the presenters; a missing dependency fails loudly at startup.
- T-8.3: entering the scene creates exactly one active run with fresh identities and no state carried over from a previous run.
- T-8.4: defeat waits the configured delay, reloads the Arena scene once, and the reloaded scene starts a new run.
- T-8.5: victory stops combat, shows the window once, and does not reload.
- T-8.6: PlayMode smoke scenarios cover new run, movement, both attacks, wave progression, defeat, and victory.

Exit criteria:

- Playing the Arena scene starts a run with no manual wiring.
- Both lifecycles complete: defeat reloads, victory does not.
- Arena runtime code references no legacy assembly.
- Architecture tests remain green; Presentation still does not reference Infrastructure.

### T-9. Production Cut-Over

Status: Planned

Goal:

Switch the main gameplay scene from the legacy stack to the Arena Combat stack.

Entry criteria:

- B-001 through B-011 in `docs/qa/behavior-baseline.md` are covered by target tests.
- All relevant legacy defects have target behavior coverage.
- Architecture, Domain, Application, Infrastructure, and Presentation tests are green.
- ArenaSandbox PlayMode smoke tests are green.
- Performance threshold is defined and passed.

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
