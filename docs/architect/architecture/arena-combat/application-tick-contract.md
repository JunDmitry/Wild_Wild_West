# Arena Combat Application Tick Contract

## Scope

The Application tick coordinates existing ArenaRun operations.

It does not implement combat rules, move Unity objects directly, publish Presentation notifications, reload scenes, or expose mutable aggregate references.

## Input

A tick uses one GameDuration and one PlayerFrameInput.

PlayerFrameInput contains:

- MovementAxes: forward and right movement axes, relative to the view;
- the current horizontal View Direction;
- an attack-start request;
- a weapon-switch request.

The Application translates MovementAxes into a planar world-space MovementInput. View Direction is forward; its right-hand perpendicular is right. The resulting magnitude is clamped to one, so diagonal input is not faster, and zero axes produce no movement request.

The View Direction is also the Aim Direction for both weapons.

AttackPressed requests Attack Start. It does not request immediate Damage.

The View Direction remains available when no new attack is requested because an existing pending Attack may become due.

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

A defeated ArenaRun runs the Defeat Restart stage and then ends the current tick before later combat stages.

ArenaRunTickCoordinator returns a tick result and does not publish notifications.

## External Resolution Boundary

Resolver ports receive Domain requests but return payload data:

- accepted Player position;
- hit Enemy identities;
- accepted Enemy movement entries;
- Enemy spawn position.

Application constructs Domain resolutions using the correlation of the corresponding saved request.

Domain performs final admission and payload validation.

Player attack targeting returns the nearest Enemy that the horizontal ray
from the request origin along its Aim Direction reaches within the request
range, and never more than one Enemy for a ranged weapon. The adapter owns
that selection; the Domain validates range and cardinality, not proximity.

Melee targeting returns every Enemy whose horizontal offset from the request
origin lies within the request range and within the request Attack Arc around
its Aim Direction, each Enemy at most once. The adapter owns that selection;
the Domain independently rejects any melee target outside the range or the arc
(`TargetOutOfRange`, `TargetOutsideArc`) and dedupes repeated identities. The
Attack Arc is a Domain value carried by the weapon catalog and copied into the
request; a weapon that does not tune one is unrestricted (`AttackArc.Full`).

An empty targeting result is a legitimate miss.

An adapter failure must not be converted silently into a miss or successful blocked movement.

Enemy spawn placement returns a position in the Spawn Band, at arena ground
height, that lies outside the player's current view whenever the band offers
such a point; if every band candidate is visible, it returns the band point
farthest from the Aim Direction, so an Enemy never materializes in front of
the player. The adapter owns that choice — it is the only layer that knows the
camera. The Domain validates the result only: outside Arena Bounds, inside the
Spawn Band, on the ground, plus wave-phase and duplicate-identity rules.

Enemy movement resolution returns one accepted position per intent in the
batch. The Domain builds the batch: it computes each pursuit intent (direction
toward the Player's current position, distance the Enemy can travel this step)
and omits Enemies that are defeated, have a pending attack, or already stand
within their attack range — so the stop-to-attack rule is not the adapter's.
The adapter may only shorten or stop an intent for scene geometry; it must not
re-aim it, extend it beyond the requested distance, add an Enemy, or drop one.
The Domain validates that every accepted position lies on its intent's path
and inside the Arena Bounds.

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

A tick that performs a Defeat Restart reports the new ArenaRun: its identity, its initial revision, and its initial snapshot. Consumers must read ArenaRunId from the result and must not assume it is stable across ticks.

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

When the active run changes, it resets the spawn pacing policy and the run restart delay policy before executing the next step.

Identity sources are not reset.

The pending interaction tracker is managed by the recovery stage and requires no explicit reset on run change.

## Defeat and Restart

A defeated ArenaRun restarts itself. The tick, not the view, owns the transition (BR-017).

IRunRestartDelayPolicy belongs to Application. FixedIntervalRunRestartDelayPolicy is constructed with a GameDuration tuning value supplied by composition; no other layer reads or writes it.

The Defeat Restart stage runs only while the active run is in the Defeat status. It records ArenaRunTickStage.DefeatRestart, feeds the tick delta to the policy, and restarts when the policy reports that the delay has elapsed.

The stage calls ArenaRunSession.RestartDefeatedRun. The Session allocates a new ArenaRunId and PlayerId, builds a fresh run through ArenaRunFactory.Start (first Wave, full Health, Firearm), adds it, and removes the defeated run. The defeated aggregate is never reset (BR-016).

After a restart the tick returns the new run's result and ends. It reads no player input and runs no gameplay stage for that frame. The restarted run is ticked normally from the next tick.

The delay is measured in GameDuration from IGameClock. A zero delta also does not advance it.

The Domain has no restart timer and no delay value. No defeat window, message, confirmation, or manual restart exists in the contract.

## Deferred Work

- T-4: Snapshots and Application notifications.
- T-5: Notification dispatch.
- T-8: Scene lifecycle (the restart delay is owned by the tick; see § Defeat and Restart).