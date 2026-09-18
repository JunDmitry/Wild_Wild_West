# Arena Combat Ubiquitous Language

## Bounded Context

### Arena Combat

The bounded context responsible for one player's combat run through consecutive enemy waves inside an arena.

It owns combat progression, player combat state, enemies participating in the run, wave progression, defeat, and victory.

It does not own rendering, Unity scenes, input devices, physics engines, navigation implementations, audio, visual effects, persistence technology, or user interface.

## Core Terms

### Arena Run

One attempt to complete the arena.

An Arena Run begins with a new Player and the first Wave and terminates in either Defeat or Victory.

Reloading a Unity scene is not part of an Arena Run. A restart creates a new Arena Run.

### Player

The combatant controlled by the user during an Arena Run.

Exactly one Player belongs to an Arena Run.

A Player has identity, position, health, selected weapon, and combat readiness.

### Enemy

A hostile combatant participating in an Arena Run.

An Enemy has identity, kind, position, health, and combat readiness.

An Enemy belongs to exactly one Arena Run.

### Regular Enemy

An Enemy participating in the regular combat phase of a Wave.

### Boss

A special Enemy that participates in the boss combat phase of a Wave.

A Boss has stronger configuration than a Regular Enemy.

Boss is an Enemy kind, not a separate aggregate.

### Wave

A progression unit inside an Arena Run.

A Wave consists of a regular combat phase followed by a boss combat phase.

A Wave completes after its Boss has been defeated.

A Wave does not have an independent lifecycle outside its Arena Run.

### Wave Number

The ordered number of a Wave inside an Arena Run.

The first Wave has number one.

### Regular Combat

The phase of a Wave in which Regular Enemies are being spawned or remain alive.

### Boss Combat

The phase of a Wave in which the Boss is waiting to be spawned or is alive.

### Completed Wave

A Wave whose required combat has finished.

### Player Defeated

A domain fact stating that Player Health has been depleted.

### Arena Run Defeated

A domain fact stating that the Arena Run reached Defeat after the Player was defeated.

Defeat is terminal. Restarting gameplay creates a new Arena Run.

### Victory

The terminal state of an Arena Run reached after the final Wave is completed.

No gameplay action may alter combat progression after Victory.

### Wave Completion

The domain transition caused by defeating the Boss of the current Wave.

Completing a non-final Wave immediately starts the next Wave.

Completing the final Wave immediately makes the Arena Run victorious.

### Wave Started

A domain fact stating that a new Wave became the current Wave of the Arena Run.

### Wave Phase Changed

A domain fact stating that the current Wave changed its phase.

### Arena Run Victorious

A domain fact stating that the final Wave has been completed and the Arena Run reached Victory.

## Combat Terms

### Health

The current and maximum combat durability of a combatant, expressed in whole health points.

Maximum Health is always positive. Current Health is never negative and never exceeds Maximum Health.

Health is depleted when Current Health reaches zero, which means the combatant is defeated.

The ratio between current and maximum Health is a presentation concern and is not part of the domain model.

### Damage

A positive whole number of health points by which Health is reduced.

Zero damage is not Damage. An attack that deals no damage is not a damage occurrence.

Damage exceeding the remaining Health depletes it without producing negative Health.

### Attack

An attempt by a combatant to damage one or more valid targets.

An Attack may be rejected when the selected weapon or combatant is not ready.

### Ranged Attack

An Attack whose target resolution requires an external three-dimensional ray query.

### Melee Attack

An Attack whose target resolution requires an external three-dimensional overlap query.

Each Enemy may be affected at most once by one Melee Attack.

### Weapon

The combat capability selected by the Player.

The current game has Ranged and Melee weapons.

A Weapon is not an independently persisted aggregate.

### Weapon Switching

The Player action that changes the Selected Weapon between Ranged and Melee.

A new Arena Run begins with the Ranged weapon selected.

Weapon Switching changes aggregate state and advances Aggregate Revision. It does not produce a Domain Event.

The current game has Ranged and Melee weapons.

A Weapon is not an independently persisted aggregate.

### Selected Weapon

The Weapon currently used when the Player attempts an Attack.

### Combat Readiness

The earliest Game Time at which a combatant or weapon may perform its next Attack.

A successfully executed Attack sets readiness to the current Game Time plus the weapon's cooldown.

### Pending Player Attack

A Player Attack that has started but whose impact has not yet been completed.

It records Attack Id, Player Id, selected Weapon kind, StartedAt, and ImpactAt.

### Pending Enemy Attack

An Enemy Attack that has started but whose impact has not yet been completed.

It records Attack Id, Enemy Id, StartedAt, and ImpactAt.

Player and Enemy pending attacks are separate domain concepts because only Player attacks use a selected Weapon kind.

### Attack Impact

The later domain transition in which a previously started Attack is resolved against the current Arena Run state.

The target affected by an Attack Impact may differ from the target that allowed Attack Start.

### Windup Duration

The duration between Attack Start and Attack Impact.

A zero Windup Duration represents an immediate impact that is still modeled as a separate conceptual stage.

### Cooldown

The minimum time between Attack Starts for the same weapon or combatant.

Cooldown starts at Attack Start, not at Damage application.

### Attack Impact Request

A domain interaction request created when a pending Attack reaches its Impact time. It carries the attacker position at impact, the current aim Direction, and the weapon range.

### Attack Impact Resolution

The external world's answer listing the Enemies hit. An empty list is a legitimate miss, not a protocol failure.

### Enemy Attack Range

The distance within which an Enemy may start an Attack, measured on the ground plane between the Enemy and the Player and extended by both Collision Radii.

### Eligible Enemy

An active Enemy that has no pending Attack, has reached its attack readiness, and has the Player within its Enemy Attack Range.

### Damage Application

The result of applying requested Damage to Health.

It contains the requested Damage, the actually applied Damage, and the Remaining Health.

Applied Damage may be lower than requested Damage because Health cannot become negative.

### Attack Cancellation Cause

The domain reason why a pending Attack did not reach Impact.

Current causes are Attacker Defeated and Arena Run Terminated.

Attack Cancellation Cause is distinct from Interaction Cancellation Reason.

### Due Attack

A pending Attack whose Impact time has been reached according to the current Game Time.

### Enemy Attack Impact Batch

The atomic aggregate operation that resolves all due Enemy Attacks in ascending EnemyId order.

## Movement Terms

### Position

A three-dimensional domain position expressed in the Arena coordinate system.

The current Arena uses Y as the vertical axis.

### Planar Movement Intent

A desired planar movement expressed as source position, Direction, and requested Distance.

The requested position is derived from these values and is never stored separately.

Player movement and Enemy movement use the same Planar Movement Intent.

### Movement Path Policy

The domain rule that decides whether an externally accepted position lies on a Planar Movement Intent: on the ground plane, not behind the origin, not beyond the requested distance, and not laterally displaced beyond tolerance.

The policy is shared by Player and Enemy movement. Actor-specific limits such as Arena Bounds are applied separately by the Arena Run.

### Movement Resolution

The external world's answer describing the actual accepted result of a Movement Intent.

The Domain accepts a Movement Resolution only for the interaction that produced it.

### Raw Displacement

A three-dimensional vector describing an offset or difference between two Positions.

A Raw Displacement may have any length, including zero.

A Raw Displacement is not guaranteed to represent a valid Direction.

### Direction

A three-dimensional vector guaranteed to have unit length.

A Direction cannot be constructed from a zero-length or near-zero-length Raw Displacement.

There is no zero Direction. Absence of movement is expressed by the absence of a valid Direction, not by a degenerate Direction value.

### Movement Input

The raw movement instruction provided by the Player for one gameplay step.

Movement Input is expressed as a Raw Displacement whose length does not exceed one.

Movement Input may be zero, meaning that the Player requested no movement.

### Arena Bounds

The logical horizontal limits of the Arena and its ground height.

Arena Bounds constrain the center of a combatant while accounting for its Collision Radius.

Arena Bounds are not Unity colliders or scene geometry.

### Distance

A non-negative finite spatial length in Arena units.

Distance is used for movement, collision geometry, and attack ranges.

### Collision Radius

The non-zero horizontal radius used when validating a combatant position against Arena Bounds and when requesting external collision resolution.

### Movement Speed

The non-zero maximum distance per second that a combatant may request through movement behavior.

### Enemy Movement Intent

A movement intent for one active Enemy, expressed as Enemy Id, Planar Movement Intent, and Collision Radius.

Enemy Movement Intent is created only for an Enemy that is active, has no pending Attack, and is outside its Enemy Attack Range.

### Enemy Movement Batch

A set of Enemy Movement Intents produced by the Arena Run in deterministic Enemy Id order.

One Enemy Movement Batch is one external Interaction and therefore carries one Interaction Id.

Creating an Enemy Movement Batch Request does not advance Aggregate Revision and does not produce Domain Events.

### Enemy Movement Batch Resolution

The external world's answer to an Enemy Movement Batch Request.

It contains one accepted position for every Enemy Movement Intent in the request.

The Arena Run accepts the batch only when the resolution has exactly the same Enemy Id set as the request and each accepted position follows its corresponding Planar Movement Intent.

A valid batch resolution may shorten movement or keep enemies in place. It cannot extend, reverse, or laterally redirect movement.

## External Interaction Terms

### Interaction Request

A typed request produced by the Domain when a decision requires information from an external system.

### Interaction Resolution

A typed answer produced by Application infrastructure for a specific Interaction Request.

A resolution must correspond to the Arena Run revision and interaction that created its request.

### Enemy Spawn Request

A domain interaction request created when the Arena Run determines that an Enemy spawn is due. It states the Enemy kind and its Collision Radius. The Arena Run does not decide when spawns are attempted; the Application asks.

### Enemy Spawn Resolution

The external world's answer containing the allocated Enemy Id and the spawn position. The position must be on the ground height, outside the Arena Bounds, and within the Spawn Band.

### Spawn Band

The distance beyond the Arena Bounds within which Enemies may be placed when they spawn.

### Enemy Catalog

The definitions of the Regular Enemy and the Boss: initial Health, Movement Speed, Collision Radius, attack Damage, attack range, and attack cooldown.

### Wave Catalog

The ordered definitions of all Waves of an Arena Run. Every Wave definition states its number of Regular Enemies; every Wave has one Boss.

### Interaction Correlation

The triple of Arena Run Id, Interaction Id, and Aggregate Revision captured when an Interaction Request is opened.

A resolution is admitted only when its Interaction Correlation matches the pending interaction and the current Aggregate Revision.

### Pending Interaction

An Interaction Request that has been opened by the Arena Run and has not yet been completed or abandoned.

At most one Pending Interaction exists per Arena Run at any moment.

### Cancelled Interaction

A Pending Interaction that the Arena Run closed through explicit cancellation without applying a resolution.

Cancellation carries a reason such as lifecycle supersession, external resolution timeout, Arena Run termination, or application shutdown.

A resolution for a Cancelled Interaction is rejected as closed.

### Player Movement Request

A domain interaction request created when the Player attempts to move and the Arena Run needs the external world to resolve collision-dependent movement.

The request expresses movement intent as source position, Direction, requested Distance, and Collision Radius. The requested position is derived from these values and is not stored separately.

The requested Distance is already limited by Arena Bounds along the Direction.

Creating a Player Movement Request does not change Aggregate Revision.

### Player Movement Resolution

The external world's answer to a Player Movement Request containing Interaction Correlation and an accepted Player position.

The accepted position must lie on the requested movement path between the source position and the requested position. The external world may shorten the movement but cannot extend, redirect, or relocate it.

A rejected resolution leaves the Pending Interaction open.

A resolution that does not change Player position closes the pending interaction without advancing Aggregate Revision.

A resolution that changes Player position advances Aggregate Revision exactly once.

## Lifecycle Terms

### Run Revision

A monotonically increasing version of an Arena Run used to prevent an external Interaction Resolution from being applied to stale aggregate state.

A Run Revision is local to one Arena Run and is not an entity identity.

### Arena Run Status

The domain lifecycle state of an Arena Run.

The valid statuses are Playing, Defeat, and Victory.

Loading, scene reloading, asset loading, and scene binding are Application lifecycle concepts and are not Arena Run statuses.

### Aggregate Revision

The monotonic consistency version of one Arena Run.

A successfully accepted state-changing aggregate operation advances Aggregate Revision exactly once.

Multiple Domain Events caused by the same atomic aggregate operation share the resulting Aggregate Revision.

### Interaction Id

A monotonically increasing identifier local to one Arena Run that identifies an external interaction initiated by that Arena Run.

Interaction Id is not a domain entity identity and does not use the process-wide entity identity sequence.

### Domain Event

An immutable statement that a meaningful fact has occurred inside the Arena Combat domain.

Domain Events describe completed domain facts and never request Unity behavior.

### Application Notification

An Application-level representation of committed domain facts intended for external consumers such as Presentation, audio, visual effects, analytics, and scene lifecycle.

An Application Notification is not a Domain Event.

### Game Time

The monotonically increasing simulated time of one Arena Run, starting at zero.

Game Time is advanced only by the Arena Run at the request of the Application and is the sole reference for combat readiness.

### Weapon Catalog

The definitions of the Ranged and Melee weapons: Damage, range, and cooldown.

## Identity Terms

### Arena Run Id

The domain identity of an Arena Run.

### Player Id

The domain identity of a Player.

### Enemy Id

The domain identity of an Enemy.

Domain identities are allocated outside the Domain and provided when domain objects are created.

## Terms Outside the Domain

The following terms are not Arena Combat domain concepts unless explicitly translated at a boundary:

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