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

Arena Bounds resolve the movement with sliding (ADR-0023). When the requested travel would cross a bound, the Player travels to the bound and then along it; the Player Movement Request that results therefore may carry a direction different from the input direction, and its requested distance is the length of that resolved path. A direction with no component along the bound stops at the bound, and a diagonal into a corner stops in the corner.

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

ArenaRunTickCoordinator returns a tick result and does not publish notifications. ArenaRunTickService publishes Application notifications after the result is built (see § Victory and Terminal Runs).

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
The Domain validates that every accepted position lies on its intent's path.
The adapter must also keep the Enemy's collision radius inside the Arena
Bounds — an Enemy spawned in the Spawn Band may legitimately start outside
them — and the Domain does not re-validate the Bounds for enemy movement.

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

Tick results are inputs for Application notification mapping, not direct Presentation messages.

A tick that performs a Defeat Restart reports the new ArenaRun: its identity, its initial revision, and its initial snapshot. Consumers must read ArenaRunId from the result and must not assume it is stable across ticks.

Presentation renders from the reported ArenaRunSnapshot. The result also publishes a presentation-ready ArenaRunHudSnapshot (Hud), derived from that snapshot, so both describe one revision. The HUD model carries the labels and formatted values the S-001 HUD regions show: health as text and fraction, the equipped weapon as text, the current wave against the run's total wave count as text (WaveCurrent, WaveTotal, WaveText), and the defeated flag; Application owns that formatting (ADR-0024). The wave total is the run's configuration, published by the Domain state snapshot from the run's WaveCatalog (WaveTotal) and carried into ArenaRunSnapshot, so the S-003 defeat window and the wave readout share one contract (ADR-0025). Presentation binds the HUD to Hud and does not format gameplay values itself; other presenters keep reading Snapshot (ADR-0020). Application notifications stay reserved for one-shot facts (ADR-0022) and carry no state a snapshot already publishes.

The wave text reads as the S-001 region shows it: WaveText is "Wave 2 of 3", formatted by Application with the same ownership as the health and weapon text (ADR-0024), and a surface that wants the numbers alone reads WaveCurrent and WaveTotal — which is what the defeat window does. A wave beginning is announced from published state, not from a notification: the current wave stays readable in WaveCurrent, so the fact has a state to read afterwards, unlike a damage or a missed attack. Presentation announces once when the run id changes to a Playing run, which is wave 1, and once each time WaveCurrent advances, at the same moment the region updates. Nothing is announced while the run is terminal, and an advancing number that does not begin a wave does not exist — the counter moves only when a wave begins.

An enemy is presented only once it is inside Arena Bounds: Presentation renders it when ArenaBounds.Contains(enemy.Position, enemy.CollisionRadius) holds in the published snapshot. The outside-to-inside transition between successive snapshots is the entry moment; nothing is drawn at the spawn position, and no placeholder, flash, or effect announces it. The entry is derived from published state, not a Domain fact, so no event or notification is added for it. The placement adapter still chooses Spawn Band points outside the current view (§ External Resolution Boundary).

The HUD model carries the boss region too: its label, the boss's current and maximum health, the formatted text and fraction, and a presence flag. The flag and the values follow the same rule as rendering — they describe the boss only while ArenaBounds.Contains holds for it in the published snapshot — so an indicator never appears for a boss that is not presented. Application formats the values with the same ownership as the rest of the HUD (ADR-0024), and Presentation binds the region, which appears with the boss and disappears when the boss is defeated.

The boss's entry needs no notification either: its distinct marker is drawn from the outside-to-inside edge for an enemy whose Kind is Boss, and the two announcements follow the same edges — "Boss enters" once on that entry, "Boss defeated" once when a boss that was presented stops being presented while the run is Playing. Both facts stay readable in published state, unlike a damage or a missed attack, so ADR-0022 adds nothing here.

An Enemy Attack's cue window is its windup: the attack starts in stage 7 and its Damage is applied only when its ImpactAt is reached in stage 8, so a started attack is published as pending in every snapshot until it lands. Presentation shows the attack cue from that published state — EnemySnapshot.HasPendingAttack with PendingAttack.StartedAt and PendingAttack.ImpactAt. The cue's duration and form are tuning and design; the ordering is not, and the cue itself needs no notification (ADR-0022) — the outcome of the attack does, and it is described in § Victory and Terminal Runs. Enemy tuning must keep AttackWindupDuration longer than one tick's delta, or the attack starts and impacts inside a single tick and no snapshot ever reports it pending.

The firearm's aiming marker is Presentation-owned in the same way: it is drawn from the current View Direction and the published snapshot alone, and shown only while the equipped weapon is the firearm and the run is Playing with the player alive. It selects nothing, locks nothing, and adds no Application state.

The melee swing is Presentation-owned on the same terms. It is drawn from the published pending attack — PlayerSnapshot.HasPendingAttack with PendingAttack.WeaponKind, StartedAt and ImpactAt — and it is redrawn from the current View Direction while the attack is pending, so the arc covers the area the impact will check. Damage resolves along the View Direction supplied to stage 5, not the direction the attack started with, so a player who turns during the windup swings into the new direction; the arc follows for the same reason. The swing is shown only while the run is Playing with the player alive, so a pending melee attack left unresolved by a terminal transition is not drawn, and no standing reach marker is added. With melee equipped the firearm's aiming marker is absent.

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

The Domain has no restart timer and no delay value. The Application owns no defeat message and no defeat notification: the published snapshot keeps the Defeat status and the wave reached for the whole delay, and Presentation renders the defeat window S-003 from that published state — no control, no confirmation, no manual restart (ADR-0025). The window's lifetime is exactly the defeated state; it ends when the restart publishes the fresh run. A manual restart, a restart control, and a defeat confirmation do not exist in the contract.

## Victory and Terminal Runs

A run reaches the Victory status when the last Wave of its WaveCatalog is completed. The Domain emits ArenaRunVictorious in the same revision as the WaveCompleted that closed that Wave, so one committed transition produces both events.

Application maps Domain Events to Application notifications. ArenaRunNotificationMapper is the single mapping place: it maps ArenaRunVictorious to ArenaRunVictoryNotification, which carries the ArenaRunId and the revision of the committed operation. Events without a notification mapping produce nothing; today that is every event except victory, enemy damage, and the outcome of an Enemy Attack (below).

IArenaRunNotification marks an Application notification. IArenaRunNotificationSink is the outbound port; Presentation implements it and the CompositionRoot supplies it, so Application never references a view.

ArenaRunTickService publishes the notifications of a tick after the result is built, in Domain Event order. A tick whose events map to nothing publishes nothing.

The notification is published once, because the event exists only in the tick that produced it; a later tick on a victorious run produces no new events and therefore no repeat.

ArenaRunNotificationMapper maps a second one-shot fact: EnemyDamaged becomes ArenaRunEnemyDamagedNotification, carrying the run id, the revision, and the enemy id. Damage is a one-shot fact with no state to read afterwards — a later snapshot shows the reduced health but not that this impact produced it — so it stays on the notification channel (ADR-0022). The mapping is damage-source-agnostic: a firearm impact and a melee impact both produce it. Presentation shows the hit confirmation at that enemy's published position; the mark's form and lifetime are design and tuning. One impact produces one EnemyDamaged per enemy it damages, all carrying that impact's AggregateRevision, so the confirmation needs no batch signal: Presentation marks each named enemy and announces the hit once per revision — once per impact, however many enemies it damaged. A swing that damages nothing produces no event, no mark, and no announcement. Application publishes a tick's notifications inside the tick, before the result exists, so Presentation resolves the confirmation's position from the snapshot it most recently presented — the pre-impact one, which still contains an enemy the impact defeated; a killing blow therefore confirms, while an enemy absent from that snapshot shows no mark.

ArenaRunNotificationMapper maps the outcome of an Enemy Attack as well: EnemyAttackCompleted becomes ArenaRunEnemyAttackResolvedNotification with Hit or Miss, and EnemyAttackCancelled becomes the same notification with Cancelled. The notification carries the run id, the revision, the enemy id, and that outcome. A hit needs no announcement of its own — PlayerDamaged carries the source enemy and the S-001 health region announces the change — but a miss and a cancellation have no state to read afterwards: a pending attack that disappears looks the same whatever ended it, and one enemy hitting while another misses in the same tick leaves a single health change that names neither. Presentation announces the cue once when an enemy's pending attack appears and announces the miss once on Miss or Cancelled; a cue that stays pending is not re-announced. Cancellation follows the same rule whatever its cause, so an attacker the player defeats and an attack cut short by a terminal transition both announce the miss.

A sink failure stops the tick with ArenaRunTickFailedException carrying the built result as its partial result. There is no retry and no rollback: the aggregate transition is already committed when the sink is called.

While the run is terminal, the view holds the direction it had at the terminal transition: the third-person camera stops responding to mouse movement, and the input adapter keeps returning that last view direction, so movement and aim requests stay unchanged no-ops. A new run restores control. The freeze is Presentation-owned and derived from the published Status — the tick still reads input, but the Application runs no gameplay stage once the run is not Playing, and the view direction is neither Domain nor Application state. Defeat and Victory behave the same, and the surfaces above them (S-002, S-003) do not own the camera.

Victory does not restart. A victorious run is terminal and is replaced only when composition starts another run; Defeat restarts automatically (see § Defeat and Restart). Notifications describe a committed fact and are never commands back into Application.

Presentation reacts to the victory notification, not to a snapshot edge. The S-002 victory window appears once when ArenaRunVictoryNotification is received, in the same tick as the winning operation, and stays for as long as the run is shown. It is modal and non-dismissible, holds the focus, and offers no control; the arena view behind it holds its last snapshot, and the four gameplay inputs change nothing. The notification carries no display text: the window's title and message are Presentation copy, so the notification stays a fact. No new port or notification is needed for the window.

## Deferred Work

- T-4: Remaining Application notifications beyond victory.
- T-5: Notification dispatch to the view (the Application port and the mapper exist; the adapter does not).
- T-8: Scene lifecycle (the restart delay is owned by the tick; see § Defeat and Restart).