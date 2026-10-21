# Arena Combat Application Tick Contract

## Scope

The Application tick coordinates existing ArenaRun operations.

It does not implement combat rules, move Unity objects directly, publish Presentation notifications, reload scenes, or expose mutable aggregate references.

## Input

A tick uses one GameDuration and one PlayerFrameInput.

PlayerFrameInput contains:

- planar MovementInput;
- current AimDirection;
- an attack-start request;
- a weapon-switch request.

AttackPressed requests Attack Start. It does not request immediate Damage.

AimDirection remains available when no new attack is requested because an existing pending Attack may become due.

A zero delta must not be passed to ArenaRun.AdvanceTime.

## Stage Order

Before every normal tick, Pending Interaction Recovery runs.

Normal gameplay order:

1. Advance Game Time.
2. Apply Player weapon switching.
3. Resolve and apply Player movement.
4. Start a requested Player attack.
5. Resolve a due Player attack impact.
6. Resolve and apply Enemy movement batch.
7. Start eligible Enemy attacks.
8. Resolve due Enemy attack impacts.
9. Attempt Enemy spawn according to Application pacing.

A rejected interaction resolution ends the current tick after recording the operation result.

A terminal ArenaRun ends the current tick before later combat stages.

ArenaRunTickCoordinator returns a tick result and does not publish notifications.

## External Resolution Boundary

Resolver ports receive Domain requests but return payload data:

- accepted Player position;
- hit Enemy identities;
- accepted Enemy movement entries;
- Enemy spawn position.

Application constructs Domain resolutions using the correlation of the corresponding saved request.

Domain performs final admission and payload validation.

An empty targeting result is a legitimate miss.

An adapter failure must not be converted silently into a miss or successful blocked movement.

Player stages are implemented as internal Application collaborators composed by PlayerTickPhase. They are not public API and are not invoked by Presentation or CompositionRoot directly.

## Spawn Pacing

IEnemySpawnPacingPolicy belongs to Application.

ShouldRequestSpawn observes the schedule without consuming an opportunity.

RecordSuccessfulSpawn is called only after Domain accepts a spawn.

Reset is called when the active ArenaRun changes to a new attempt.

Identity sources are not reset.

## Tick Result

ArenaRunTickResult contains:

- ArenaRunId;
- final observed AggregateRevision;
- Domain Events in occurrence order;
- executed Application stages.

Each event retains the revision of the aggregate operation that produced it.

The final tick revision is not assigned to all events.

Collections are copied and published read-only.

Tick results are inputs for future Application notification mapping, not direct Presentation messages.

## Atomicity and Failures

A tick consists of several aggregate operations.

A tick is not an aggregate transaction and does not promise rollback of earlier successful operations.

A rejected external resolution stops further processing of the step and leaves its interaction pending.

The next-step cancellation procedure is implemented in T-3.4.

Adapter exceptions propagate as ArenaRunTickFailedException. The exception carries a partial tick result containing Domain Events produced before the failure. Callers must deliver the partial result to notification consumers before handling the failure.

An interaction left pending by a failed or rejected step is cancelled at the beginning of the next step with the SupersededByLifecycle reason.

## Public Entry Point

ArenaRunTickService is the only public type that executes a gameplay step.

CompositionRoot and future Unity game-loop code call ArenaRunTickService.ExecuteTick.

The service returns ArenaRunTickResult on success.

On adapter or orchestration failure it throws ArenaRunTickFailedException. The exception carries the partial tick result produced before the failure. Callers must deliver the partial result to notification consumers and then handle the failure.

The service never returns the mutable ArenaRun aggregate.

## Run-Change Handling

The service observes ActiveArenaRunId.

When the active run changes, it resets the spawn pacing policy before executing the next step.

Identity sources are not reset.

The pending interaction tracker is managed by the recovery stage and requires no explicit reset on run change.

## Deferred Work

- T-4: Snapshots and Application notifications.
- T-5: Notification dispatch.
- T-8: Scene lifecycle and restart delay.