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

Before normal gameplay stages, the coordinator handles an unfinished interaction from a previously interrupted step according to the cancellation policy.

Normal gameplay order:

1. Advance Game Time.
2. Apply the Player weapon-switch request.
3. Request, resolve, and apply Player movement.
4. Apply the Player attack-start request.
5. Request, resolve, and apply Player attack impact when Domain reports it due.
6. Request, resolve, and apply the Enemy movement batch.
7. Start eligible Enemy attacks.
8. Resolve due Enemy attack impacts.
9. Attempt Enemy spawn when Application pacing permits it.

The coordinator stops further combat stages when ArenaRun becomes terminal.

An Enemy spawned at stage 9 does not move or attack earlier in the same tick.

No Player input does not imply an idle simulation.

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

Adapter exceptions propagate. Preservation of already produced events across an exceptional exit must be addressed in T-3.4 before the coordinator is considered production-ready.

## Deferred Work

- T-3.2: Player movement and attack stages.
- T-3.3: Enemy movement and spawn stages; fixed-interval pacing.
- T-3.4: Interrupted-step recovery, cancellation, and error handling.
- T-3.5: Complete coordinator and ordered-step tests.
- T-4: Snapshots and Application notifications.
- T-5: Notification dispatch.
- T-8: Scene lifecycle and restart delay.