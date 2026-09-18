# Arena Combat Identity Policy

## Identity Types

The Arena Combat bounded context uses three typed domain identities:

- ArenaRunId;
- PlayerId;
- EnemyId.

The numeric value of one identity type has no semantic relationship with the numeric value of another identity type.

ArenaRunId(1), PlayerId(1), and EnemyId(1) are distinct domain values.

## Identity Sources

The Application layer owns three separate contracts:

- IArenaRunIdSource;
- IPlayerIdSource;
- IEnemyIdSource.

Each contract has one operation:

`Allocate()`
Each source owns an independent monotonic sequence beginning at one.

## Lifetime

Identity source instances are registered in the Bootstrap root scope.

They survive ArenaScene reloads.

A new ArenaRun receives a new ArenaRunId.

A new Player receives a new PlayerId.

A newly spawned Enemy receives a new EnemyId.

The sequences are not reset when ArenaScene is reloaded.

## Prohibited Operations

Identity sources do not expose:

- Reset;
- Release;
- Reuse;
- Peek;
- Commit.

Once a value is allocated, it is never returned to the source.

## Domain Boundary

Domain objects receive typed identities from Application.

Domain objects do not allocate identities.

Unity InstanceID, GameObject identity, prefab identity, and pooled object identity are not domain identities.

## Exhaustion

If an identity source reaches the maximum representable value, it throws an explicit exception instead of wrapping and reusing an existing identity.

## Testing

The following properties are mandatory:

- each source starts at one;
- sources do not share counters;
- values are monotonic;
- values are never zero;
- allocated values are never reused;
- exhaustion is explicit;
- typed source contracts return their corresponding identity types.