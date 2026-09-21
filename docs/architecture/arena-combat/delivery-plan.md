# Arena Combat Delivery Plan

## Purpose

This document is the authoritative implementation plan for the Arena Combat bounded context.

The plan is updated only at explicit planning or refactoring milestones.

Completed work remains recorded for traceability. Future work must follow the order defined here unless a new ADR or an explicit refactoring stage changes the plan.

## Current Architecture

The project is migrating from legacy gameplay assemblies to an IDDD-oriented Arena Combat bounded context.

The primary aggregate root is ArenaRun.

ArenaRun owns:

- one Player;
- the current Wave;
- active Enemy entities;
- aggregate lifecycle state;
- aggregate revision;
- pending external interaction protocol state.

The Domain layer owns gameplay consistency rules.

The Application layer will own use-case orchestration, identity allocation, repository usage, external interaction resolution, session lifecycle, and notification dispatch.

Infrastructure will adapt Unity systems.

Presentation will render snapshots and consume Application notifications.

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

## Remaining Stages

### T-2. Repository and Application Identity Integration

Status: Completed

Goal:

Introduce repository-backed aggregate ownership and typed identity sources without Unity dependencies.

Substages:

- T-2.1: Define IArenaRunRepository in Domain. — Completed
- T-2.2: Define IArenaRunIdSource, IPlayerIdSource, and IEnemyIdSource in Application. — Completed
- T-2.3: Implement InMemoryArenaRunRepository in Infrastructure. — Completed
- T-2.4: Implement ArenaRunSession in Application. — Completed
- T-2.5: Add repository and session tests. — Completed

Exit criteria:

- Application stores ActiveArenaRunId, not ArenaRun state fragments.
- ArenaRun is retrieved through IArenaRunRepository.
- InMemoryArenaRunRepository behaves as a collection of aggregate roots.
- Identity sources are independent, monotonic, process-lifetime services.
- No Unity dependency appears in Domain or Application.

### T-3. Application Interaction Orchestration

Status: Completed

Goal:

Coordinate existing ArenaRun operations and resolve external interactions without duplicating Domain rules.

Substages:

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

application-tick-contract.md

Exit criteria:

- Application does not duplicate ArenaRun gameplay rules.
- Resolutions are constructed from matching Domain request correlations.
- Rejected interactions stop the corresponding phase.
- Pending interactions cannot leak into the next gameplay step.
- Adapter failures preserve previously produced Domain Events.
- Terminal ArenaRun stops remaining gameplay stages.
- Tick results preserve event occurrence order and event revisions.
- Spawn pacing resets when the active ArenaRun changes.
- Identity sources remain process-lifetime services and are never reset.
- Domain, Application, and architecture tests are green.

### T-4. Read Models and Application Notifications

Status: In Progress
Current substage:
- T-4.2: Define immutable ArenaRunSnapshot.

Goal:

Provide immutable read models for Presentation and transform Domain Events into Application notifications.

Substages:

- T-4.1: Define ArenaRunSnapshot.
- T-4.2: Define PlayerSnapshot, EnemySnapshot, WaveSnapshot, and combat snapshots.
- T-4.3: Define Application notifications.
- T-4.4: Implement Domain Event to Application Notification mapping.
- T-4.5: Add snapshot and notification tests.

Exit criteria:

- Presentation never receives ArenaRun directly.
- Presentation never receives Player or Enemy entities directly.
- Continuous state is rendered from snapshots.
- One-shot facts are delivered through notifications.

### T-5. Typed Result Dispatcher

Status: Planned

Goal:

Dispatch Application notifications through explicit typed handlers.

Substages:

- T-5.1: Define dispatcher pipeline stages.
- T-5.2: Define typed notification handler contracts.
- T-5.3: Implement deterministic handler ordering between stages.
- T-5.4: Define handler error policy.
- T-5.5: Add dispatcher tests.

Pipeline stages:

1. Snapshot publication.
2. Entity view synchronization.
3. Gameplay visual notifications.
4. HUD notifications.
5. Audio and VFX notifications.
6. Analytics and diagnostics notifications.

### T-6. Unity Infrastructure

Status: Planned

Goal:

Implement Unity adapters for Application ports.

Substages:

- T-6.1: Unity clock adapter.
- T-6.2: Unity input adapter.
- T-6.3: Unity player movement resolver.
- T-6.4: Unity ranged attack resolver.
- T-6.5: Unity melee attack resolver.
- T-6.6: Unity enemy movement resolver.
- T-6.7: Unity spawn point provider.
- T-6.8: Collider identity registry.
- T-6.9: Unity scene lifecycle adapter.
- T-6.10: Infrastructure PlayMode tests.

### T-7. Unity Presentation

Status: Planned

Goal:

Render ArenaRun snapshots and consume Application notifications.

Substages:

- T-7.1: PlayerView and PlayerPresenter.
- T-7.2: EnemyView and EnemyViewRegistry.
- T-7.3: HUDView.
- T-7.4: Third-person camera controller.
- T-7.5: Attack, damage, death, and spawn visual handlers.
- T-7.6: VictoryView and DefeatView.
- T-7.7: Presentation PlayMode tests.

### T-8. Bootstrap and Arena Session Lifecycle

Status: Planned

Goal:

Connect BootstrapScene, ArenaScene, Application Session, Repository, Unity adapters, and Presentation.

Substages:

- T-8.1: Bootstrap Composition Root with VContainer LifetimeScope.
- T-8.2: ArenaScene binder.
- T-8.3: ArenaRun creation flow.
- T-8.4: Defeat delay and ArenaScene reload.
- T-8.5: Victory lifecycle.
- T-8.6: ArenaSandbox smoke scenarios.

### T-9. Production Cut-Over

Status: Planned

Goal:

Switch the main gameplay scene from the legacy stack to the Arena Combat stack.

Entry criteria:

- B-001 through B-011 are covered by target tests.
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