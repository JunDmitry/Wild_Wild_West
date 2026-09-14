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

### Defeat

The terminal state of an Arena Run reached when the Player is defeated.

No gameplay action may alter combat progression after Defeat.

### Victory

The terminal state of an Arena Run reached after the final Wave is completed.

No gameplay action may alter combat progression after Victory.

## Combat Terms

### Health

The current and maximum combat durability of a combatant.

Health reaching zero means that the combatant is defeated.

### Damage

A non-negative amount by which Health is reduced.

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

### Selected Weapon

The Weapon currently used when the Player attempts an Attack.

### Combat Readiness

The earliest game time at which a combatant or weapon may perform its next Attack.

## Movement Terms

### Position

A three-dimensional domain position expressed in the Arena coordinate system.

The current Arena uses Y as the vertical axis.

### Movement Intent

A desired movement produced by domain behavior before the external world resolves collisions or navigation.

Movement Intent is not a guarantee that movement occurred.

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

## External Interaction Terms

### Interaction Request

A typed request produced by the Domain when a decision requires information from an external system.

### Interaction Resolution

A typed answer produced by Application infrastructure for a specific Interaction Request.

A resolution must correspond to the Arena Run revision and interaction that created its request.

### Spawn Request

A request from the Domain to create the identity and world placement required for a new Enemy.

### Spawn Resolution

The resolved Enemy identity and spawn position corresponding to a Spawn Request.

Allocating identity and selecting a physical spawn point do not belong to the Domain.

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