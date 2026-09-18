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