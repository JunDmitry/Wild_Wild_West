# Arena Combat Ubiquitous Language

## Bounded Context

### Arena Combat

The bounded context responsible for one Player attempt to defeat consecutive Waves of Enemies inside an Arena.

It owns combat progression, Player state, Enemy state, Wave state, Defeat, Victory, Domain Events, and aggregate consistency rules.

It does not own Unity scenes, GameObjects, rendering, input devices, physics engines, navigation implementations, UI, audio, VFX, or persistence technology.

## Core Terms

### ArenaRun

One gameplay attempt to complete all Waves.

ArenaRun begins in Playing and terminates in Defeat or Victory.

Restarting gameplay creates a new ArenaRun.

### Player

The combatant controlled by the user.

ArenaRun owns exactly one Player.

### Enemy

A hostile combatant active in ArenaRun.

Enemy is either Regular or Boss.

### Wave

A progression unit containing Regular Combat followed by Boss Combat.

Every Wave contains one Boss.

### Regular Combat

The Wave phase in which Regular Enemies remain to be spawned or alive.

### Boss Combat

The Wave phase in which the Boss is waiting to spawn or alive.

### Completed Wave

A Wave whose Boss was defeated.

### Victory

The terminal ArenaRun state reached after final Wave completion.

### Defeat

The terminal ArenaRun state reached after Player defeat.

## Combat Terms

### Health

Current and maximum combat durability expressed in whole health points.

Health is valid when maximum is positive and current is between zero and maximum.

### Damage

A positive number of health points requested to reduce Health.

### Damage Application

The result of applying requested Damage.

It contains requested Damage, applied Damage, and remaining Health.

Applied Damage may be lower than requested Damage because Health cannot become negative.

### Weapon

The Player combat capability currently selected as Ranged or Melee.

### Combat Readiness

The earliest Game Time at which a combatant may start the next Attack.

### Attack Start

The transition in which a combatant begins an Attack.

Attack Start records attack timing, creates a pending Attack, and starts cooldown.

Attack Start does not apply Damage.

### Attack Impact

The later transition in which a pending Attack is resolved against current ArenaRun state.

### Pending Player Attack

A Player Attack started but not yet completed or cancelled.

### Pending Enemy Attack

An Enemy Attack started but not yet completed or cancelled.

### Windup Duration

The duration between Attack Start and Attack Impact.

### Cooldown

The minimum interval between Attack Starts.

Cooldown starts at Attack Start.

### Attack Cancellation Cause

The domain reason a pending Attack cannot reach Impact.

Current causes are AttackerDefeated and ArenaRunTerminated.

### Due Attack

A pending Attack whose ImpactAt is reached by current Game Time.

## Movement Terms

### Position

A three-dimensional Arena coordinate.

Y is the vertical axis.

### Distance

A non-negative finite spatial length in Arena units.

### Direction

A normalized three-dimensional direction.

A zero direction is invalid.

### Raw Displacement

A three-dimensional offset that may have any length, including zero.

### Movement Input

A Player raw planar movement instruction with magnitude not greater than one.

### Planar Movement Intent

A desired movement represented by source Position, Direction, and requested Distance.

Requested position is derived from those values.

### Movement Path Policy

The Domain policy validating that an accepted position lies on a valid planar movement path.

### Arena Bounds

Logical horizontal Arena limits and ground height.

Arena Bounds are not Unity colliders.

### Collision Radius

A non-zero horizontal radius of a combatant.

### Enemy Movement Batch

A deterministic set of Enemy movement intents.

One Enemy Movement Batch is one external Interaction.

## External Interaction Terms

### Interaction Request

A typed Domain request for external information or resolution.

### Interaction Resolution

A typed external response applied only through ArenaRun.

### Interaction Correlation

ArenaRunId, InteractionId, and AggregateRevision captured when an Interaction Request is opened.

### Pending Interaction

An external Interaction Request opened by ArenaRun and not yet completed or cancelled.

Only one Interaction may be pending for ArenaRun.

### Cancelled Interaction

A pending external Interaction closed through explicit cancellation.

Interaction cancellation is different from Attack cancellation.

### Player Movement Request

A request to resolve Player collision-dependent movement.

### Enemy Movement Batch Request

A request to resolve multiple Enemy movement intents.

### Enemy Spawn Request

A request to allocate EnemyId and choose a valid spawn position for a due Enemy.

### Player Attack Impact Request

A request to resolve ranged raycast or melee overlap targeting at Player Attack impact time.

## Lifecycle Terms

### AggregateRevision

The monotonic consistency version of ArenaRun.

A successful state-changing aggregate operation advances it exactly once.

### InteractionId

A local monotonically increasing identifier of one ArenaRun interaction.

### Game Time

The simulated monotonic time of one ArenaRun.

### Domain Event

An immutable statement that a meaningful Arena Combat fact already occurred.

### Application Notification

An Application-level representation of committed Domain Events for Presentation, audio, VFX, analytics, or lifecycle handling.

### Arena Run Repository

The Domain-owned collection abstraction over ArenaRun aggregate roots.

The current runtime implementation is an in-memory Infrastructure adapter.

The repository supports adding an ArenaRun, retrieving one by ArenaRunId, and removing one by ArenaRunId.

It is not an ORM, persistence session, unit of work, query engine, or snapshot store.

The repository returns the managed aggregate reference. Aggregate mutations are performed only through ArenaRun's public API.

### Arena Run Session

The Application service that owns which ArenaRunId is currently active.

The session does not own Player, Enemy, or Wave state directly; it delegates aggregate state to the repository.

The session exposes only ActiveArenaRunId and HasActiveRun publicly. Access to the mutable ArenaRun aggregate is internal to the Application layer.

### Initial Run Start

The Application operation that creates the first ArenaRun of a session. It is rejected if an active ArenaRun already exists.

### Defeated Run Restart

The Application operation that replaces a defeated ArenaRun with a new one at Wave 1.

The previous defeated run is removed from the repository, and new ArenaRunId and PlayerId are allocated.

Defeated Run Restart is rejected if the active run is still Playing or has reached Victory.

### Player Tick Phase

The Application sequence of weapon switch, movement, attack start, and attack impact stages executed for the Player within one gameplay step.

### Tick Recorder

The Application collector of Domain Events and executed stages of one gameplay step, from which the tick result is built.

### Enemy Tick Phase

The Application sequence of enemy movement, enemy attack start, enemy attack impact, and enemy spawn stages executed within one gameplay step.

### Spawn Pacing Policy

The Application rule that decides whether a spawn attempt is permitted at the current Game Time. It observes the schedule without consuming an opportunity and records only successful spawns.

### Pending Interaction Recovery

The first stage of a gameplay step. It cancels an interaction left pending by a previous step so that the aggregate can accept new requests.

Recovery produces no Domain Events and does not advance Aggregate Revision.

### Partial Tick Result

The tick result built from Domain Events produced before a step failed. It is carried by the tick failure and must still be delivered to notification consumers.

### Tick Service

The public Application entry point that executes one gameplay step for the active ArenaRun.

The service owns the internal interaction tracker and tick coordinator, resets spawn pacing when the active run changes, and reports failures with a partial tick result.

## Identity Terms

### ArenaRunId

The domain identity of ArenaRun.

### PlayerId

The domain identity of Player.

### EnemyId

The domain identity of Enemy.

### Identity Source

An Application service that allocates identities of one specific domain identity type.

ArenaRunId, PlayerId, and EnemyId use independent identity sources.

### Identity Allocation

The Application operation that obtains a new typed identity before creating a Domain object.

### Identity Lifetime

Identity sources are process-lifetime services within the application root scope.

They survive ArenaScene reloads and never reset or reuse allocated values.

Domain identities are allocated outside Domain and supplied when Domain objects are created.

Unity InstanceID and pooled GameObject identity are not domain identities.

## Terms Outside the Domain

- GameObject
- MonoBehaviour
- Transform
- Collider
- Rigidbody
- CharacterController
- NavMeshAgent
- Animator
- Camera
- Scene
- Prefab
- ScriptableObject
- InputAction
- Time.deltaTime
- LayerMask
- RaycastHit