# Domain Model — Arena Combat

The project's ubiquitous language for the Arena Combat context, in four
parts: glossary, entities and relationships, invariants, and
lifecycles.

The canonical glossary is `docs/analyst/ubiquitous-language.md`. It is
the single place a term such as ArenaRun, Player, Enemy, Wave, Health,
Damage, Weapon, Arena Bounds, Position, or Game Time is defined; this
file does not repeat those definitions. The glossary section below
records only the terms analysis has added since the migration.

The full invariant record, including technical interaction and
aggregation invariants, is `docs/analyst/arena-run-invariants.md`. The
invariants below are the model-level truths a story or rule may rely
on; their numbered, testable form is `docs/analyst/business-rules.md`.

## 1. Glossary

Terms defined in the canonical glossary: ArenaRun, Player, Enemy, Wave,
Regular Combat, Boss Combat, Completed Wave, Victory, Defeat, Health,
Damage, Damage Application, Weapon, Combat Readiness, Attack Start,
Attack Impact, Pending Player Attack, Pending Enemy Attack, Windup
Duration, Cooldown, Attack Cancellation Cause, Due Attack, Position,
Distance, Direction, Raw Displacement, Movement Input, Planar Movement
Intent, Movement Path Policy, Arena Bounds, Collision Radius, Game Time,
ArenaRunId, PlayerId, EnemyId.

Terms added by analysis:

| Term | Definition |
|------|------------|
| Arena | the closed play space in which one ArenaRun is fought, bounded by Arena Bounds |
| Playing | the active ArenaRun state before Defeat or Victory |
| Combatant | a Player or an Enemy; a participant that can have Health and a pending Attack |
| Active | present in the ArenaRun and not defeated |
| Attack | a combatant's attempt to deal Damage, begun at Attack Start and resolved at Attack Impact |
| Attack Range | the distance within which a combatant can start and land its Attack |
| View | the player's visible picture of the Arena |
| First-Person View | a view from the Player's eyes; not available in this product |
| Character | the visible body of a Player in the Arena |
| Firearm | the Ranged Weapon |
| Melee Weapon | the Weapon when it is Melee; the close-range weapon |
| Fire Rate | the firearm's Cooldown; "Cooldown" is the preferred term |
| Attack Rate | the Melee Weapon's Cooldown; "Cooldown" is the preferred term |
| Regular Enemy | an Enemy that is not a Boss |
| Boss | the single high-health Enemy that closes each Wave |
| Final Wave | the third and last Wave of a run |
| Victory Window | the announcement shown to the player when an ArenaRun is won; it states that the run was won |
| Spawn | the entry of an Enemy into the Arena from outside Arena Bounds |
| Spawn Band | the zone outside Arena Bounds from which a Spawn may place an Enemy |
| Spawn Pacing | the schedule by which a Wave's Enemies enter the Arena; how many enter and how far apart is a tuning value |
| Wave Number | the position of the current Wave in the run, beginning at one |
| Movement | the change of a Player's Position in response to Movement Input |
| View Direction | the horizontal direction the camera currently faces, relative to which Movement Input is interpreted |
| Third-Person View | the view in which the camera is positioned away from the Player and the Player's character remains visible |
| Restart | the replacement of a defeated ArenaRun with a new one at Wave one |
| Defeat Message | the announcement shown to the player when an ArenaRun ends in Defeat, within the delay before the Restart, naming the Wave Number reached |
| run | informal synonym for ArenaRun; "ArenaRun" is the preferred term |
| Weapon Switch | the exchange of the Equipped Weapon for the other Weapon on one Weapon-Switch Input |
| Weapon-Switch Input | the player action that requests a Weapon Switch; one press performs one Weapon Switch |
| Equipped Weapon | the one Weapon a Player currently holds and whose Attack the Attack Input performs |
| Attack Input | the player action that performs the Equipped Weapon's Attack |
| Alive | the state of a Combatant that is Active: present in the ArenaRun and not defeated |
| Defeated | the state of a Combatant whose Health has reached zero; a defeated Enemy is removed from the active Enemies, a defeated Player persists until a new ArenaRun begins |
| Ranged Attack | an Attack made with the Firearm |
| Shot | informal synonym for a Ranged Attack with the Firearm; "Ranged Attack" is the preferred term |
| Aim Direction | the Direction in which an Attack is delivered; for both Weapons, the current View Direction |
| Weapon Damage | the Damage an Attack with a Weapon requests |
| Weapon Readiness | the earliest Game Time at which that Weapon can start its next Attack; each Weapon has its own |
| Melee Attack | an Attack made with the Melee Weapon |
| Enemy Attack | an Attack made by an Enemy; its target is the Player |
| Attack Cue | a visible signal shown when an Enemy Attack starts, before that Attack's Damage is applied; its duration is a tuning value |
| Attack Arc | the angular spread around the Aim Direction within which a Melee Attack reaches |
| Pursuit | an Enemy's movement toward the Player, which continues until the Enemy is within its Attack Range; movement speed is a tuning value |

Preferred terms: use **Cooldown** rather than Fire Rate or Attack Rate;
use **Enemy** with Regular Enemy and Boss as its two kinds; use **Arena
Bounds** rather than "arena edge" or "arena boundary"; use **Firearm**
and **Melee Weapon** for the two Weapons.

## 2. Entities and relationships

- An **ArenaRun** contains exactly one **Player**.
- An **ArenaRun** has exactly one current **Wave** and the **Enemies**
  active in it.
- Every **Wave** contains **Regular Combat** followed by **Boss
  Combat**.
- Every **Wave** contains exactly one **Boss** and any number of
  **Regular Enemies**.
- An **Enemy** is either a **Regular Enemy** or a **Boss**.
- A **Player** has one **Health**; an **Enemy** has one **Health**.
- A **Player** has exactly one equipped **Weapon**, either the
  **Firearm** or the **Melee Weapon**.
- A **Player** carries both the **Firearm** and the **Melee Weapon**;
  a **Weapon Switch** changes which one is equipped.
- A **Player's** **Position** lies inside **Arena Bounds**.
- An **Enemy** pursues the **Player**, moving toward the Player's current
  **Position** until it is within its **Attack Range**.
- A **Spawn** places an **Enemy** outside **Arena Bounds**, within the
  **Spawn Band**.
- A **Wave** has a **Spawn Pacing** that schedules when each of its
  **Enemies** enters the **Arena**.
- A **Player Attack** targets **Enemies**; an **Enemy Attack** targets
  the **Player**.
- A **Ranged Attack** is aimed along its **Aim Direction** and affects at
  most one **Enemy**: the nearest one along that direction within
  **Attack Range**.
- A **Melee Attack** is aimed along its **Aim Direction** and affects
  every **Enemy** within **Attack Range** in front of the **Player**,
  within its **Attack Arc**.
- A **Weapon** has a **Weapon Damage** value; an **Attack** with it
  requests that **Damage**.
- An **ArenaRun** contains a monotonically increasing **Game Time**.

## 3. Invariants

**Identity**
- An ArenaRun and a Player each keep one identity for their entire
  lifetime.
- An Enemy identity is unique among the active Enemies of one ArenaRun.

**ArenaRun**
- An ArenaRun is Playing, defeated, or victorious — never two at once.
- Defeat and Victory are terminal.
- Combat progression stops when the ArenaRun is terminal.
- A new attempt is a new ArenaRun; a terminal ArenaRun is never reset.

**Player**
- An ArenaRun contains exactly one Player.
- Health lies between zero and its maximum, inclusive.
- A Player's Health is observable, and an observed value reflects the
  latest Damage applied.
- A defeated Player cannot move, switch weapons, start an attack, or
  resolve an attack.
- A new run starts with the Player at full health and the Firearm
  equipped.
- A Weapon Switch exchanges the Equipped Weapon; it never gains, loses,
  or consumes a Weapon.
- The equipped Weapon changes only through a Weapon Switch or the start
  of a new run.
- Each Weapon's readiness is independent of the other's; a Weapon Switch
  preserves both Weapons' readiness and transfers none.

**Enemy**
- Every active Enemy belongs to exactly one ArenaRun.
- Enemy health lies between zero and its maximum, inclusive.
- An Enemy whose Health reaches zero is Defeated.
- A defeated Enemy is removed from the active Enemies.
- While the ArenaRun is Playing, an Enemy that is not defeated moves toward
  the Player until it is within its Attack Range.
- Every Wave has at most one Boss, and a Boss is tougher than any
  Regular Enemy in the run.

**Wave**
- Exactly one Wave is current while the ArenaRun is active.
- Wave Number begins at one.
- A new Wave's Wave Number is one greater than the Wave Number of the
  Wave it follows.
- A Wave cannot begin before the current Wave is complete.
- A Boss cannot become active while Regular Enemies remain to be
  spawned or alive.
- A Wave completes only when all its required Regular Enemies and its
  Boss are defeated.
- Completing a non-final Wave starts the next Wave; completing the
  final Wave results in Victory.
- A Wave with no Regular Enemies begins in Boss Combat.

**Movement and view**
- Movement Input is planar and has magnitude no greater than one.
- A Player's Position never leaves Arena Bounds.
- A defeated Player's Position does not change in response to Movement
  Input.
- Movement is interpreted relative to the current View Direction.
- The Player is observed from a Third-Person View and remains visible.

**Spawning**
- A Regular Enemy Spawn is due only in Regular Combat while Regular
  Enemies remain to spawn.
- A Boss Spawn is due only in Boss Combat while the Boss has not
  spawned.
- A spawned Enemy enters outside Arena Bounds, within the Spawn Band,
  at arena ground height.
- No Enemy enters after the run ends, and no later-Wave Enemy enters
  while the current Wave is in progress.
- A Wave's Enemies enter on that Wave's Spawn Pacing, and only while it
  is the current Wave.

**Combat**
- Damage is positive; applied damage equals the actual health
  reduction; health never becomes negative.
- Attack Start applies no Damage; Attack Impact resolves against the
  state at impact time.
- A combatant has at most one pending Attack, and Cooldown starts at
  Attack Start.
- A ranged Attack Impact affects at most one Enemy; a melee Attack
  damages a given Enemy at most once.
- An Enemy Attack starts and lands only within Attack Range.
- An Enemy Attack's Attack Cue is visible before that Attack's Damage is
  applied.
- An Enemy within its Attack Range attacks rather than closing further.
- Player health reaching zero transitions the ArenaRun to Defeat in the
  same operation, and pending Attacks of a defeated combatant are
  cancelled.
- A Shot affects only the nearest Enemy along its Aim Direction within
  Attack Range.
- A Shot that reaches no Enemy within Attack Range applies no Damage.
- A Melee Attack affects every Enemy within Attack Range in front of the
  Player, and no Enemy beyond Attack Range or outside its Attack Arc.

**Game Time**
- Game Time never decreases.
- Combat Readiness is compared only against the current Game Time.

## 4. Lifecycles

**ArenaRun**
- Playing → Defeat (Player health reaches zero)
- Defeat shows the Defeat Message before the new ArenaRun begins
- Playing → Victory (final Wave completed)
- Defeat → a new ArenaRun in Playing (Restart)
- Victory is terminal

**Wave**
- Regular Combat → Boss Combat (all required Regular Enemies defeated,
  or none required)
- Boss Combat → Completed (Boss defeated)
- Completed, non-final → the next Wave begins in Regular Combat
- Completed, final → ArenaRun Victory

**Player**
- Alive → Defeated (health reaches zero)
- Defeated → Alive only through a new ArenaRun

**Health**
- Full → damaged → depleted (zero)
- Within one ArenaRun, health only decreases
- A new ArenaRun starts at full health

**Weapon**
- Firearm ⇄ Melee Weapon (one Weapon-Switch Input while the Player is
  Alive)
- Every new ArenaRun starts with the Firearm
- A switch is rejected while a pending Attack exists or the Player is
  defeated
- A switch preserves each Weapon's readiness; it neither transfers,
  resets, nor shares it
- No input other than the Weapon-Switch Input changes the equipped
  Weapon; a new run is the sole exception

**Attack**
- Ready → Started (the attack input is accepted: the attacker is Alive,
  no Attack is pending, and the Weapon is ready)
- Started → Impact (Game Time reaches the impact time; Damage is applied
  or the Attack misses)
- Started → Cancelled (the attacker is defeated)
- Impact or Cancelled → Ready (once the attacker can attack again;
  Cooldown starts at Attack Start)

**Enemy**
- Spawned (outside Arena Bounds) → Active (inside the Arena) → Defeated
  → removed from the run
