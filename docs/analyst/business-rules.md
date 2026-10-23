# Business Rules

Every rule is one testable statement, numbered `BR-<NNN>` and grouped by
the concept it constrains. Stories link a rule by id; they never copy
its text.

The baseline rules in this file adopt the behaviour-level statements
from the migrated `docs/analyst/arena-run-invariants.md`. Invariants
that only constrain internal interaction, consistency bookkeeping, or
application orchestration remain architectural and are not restated
here. Terms are defined in `docs/analyst/domain-model.md` and its
canonical glossary, `docs/analyst/ubiquitous-language.md`.

## Identity

### BR-001 — ArenaRun identity is immutable

An ArenaRun has exactly one identity for its entire lifetime.

### BR-002 — Player identity is immutable

A Player has exactly one identity for its entire lifetime.

### BR-003 — Enemy identity is unique among active enemies

An Enemy identity is unique among the active Enemies of one ArenaRun.

## ArenaRun

### BR-010 — One player per run

An ArenaRun contains exactly one Player.

### BR-011 — A run starts Playing

An ArenaRun starts in the Playing state.

### BR-012 — Defeat and Victory are terminal

Defeat and Victory are terminal ArenaRun states.

### BR-013 — Defeat and Victory are exclusive

An ArenaRun cannot be both defeated and victorious.

### BR-014 — Combat stops when the run is terminal

Combat progression does not continue after Defeat or Victory.

### BR-015 — Player death defeats the run

Player defeat transitions the ArenaRun to Defeat.

### BR-016 — A new attempt is a new run

Restarting gameplay creates a new ArenaRun; a terminal ArenaRun is never
reset.

### BR-017 — Defeat restarts automatically

A defeated run restarts automatically, without a menu or confirmation.

### BR-117 — No attack after victory

Once an ArenaRun is Victory, the player can start no Attack.

### BR-118 — Victory is announced

When an ArenaRun reaches Victory, a Victory window stating that the run was won
is shown to the player.

## Player and Health

### BR-020 — A run starts at full health

Every new ArenaRun starts with the player at full health.

### BR-021 — Health never exceeds its maximum

A player's health never exceeds its maximum.

### BR-022 — Health never goes negative

No health value ever falls below zero.

### BR-023 — A dead player cannot act

A defeated player cannot move, switch weapons, start an attack, or
resolve an attack.

### BR-024 — Death lasts until restart

A defeated player remains defeated until the run restarts.

### BR-025 — A run starts with the firearm equipped

Every new ArenaRun starts with the firearm equipped.

### BR-026 — Health is observable

The player's current health is observable and changes as soon as damage
is applied.

### BR-027 — Restart resets the run

A restarted run begins at the first Wave with full health and the
firearm equipped.

### BR-028 — Restart discards the previous attempt

No enemy from a previous attempt exists after a restart.

## Movement and View

### BR-030 — Movement input is planar and bounded

A movement input is planar and has a magnitude no greater than one.

### BR-031 — Movement requires a Playing run

The player can move only while the ArenaRun is Playing.

### BR-032 — Movement follows the view direction

The player's movement direction follows the current view direction:
forward is away from the camera, back toward it, and left and right are
to its sides.

### BR-033 — The player stays inside the arena

The player's position never leaves Arena Bounds.

### BR-034 — A dead player does not move

A defeated player's position does not change in response to movement
input.

### BR-035 — The view can turn while the run is Playing

The view direction can be changed by the player while the ArenaRun is
Playing.

### BR-036 — View and movement can change together

The view direction and player movement can change in the same gameplay
step.

### BR-037 — The view is third-person only

The player is observed from a third-person view; no first-person view is
available.

### BR-038 — The player stays visible

The player's character remains visible in the third-person view at all
times.

## Enemy

### BR-040 — Active enemies belong to one run

Every active Enemy belongs to exactly one ArenaRun.

### BR-041 — Enemy health never exceeds its maximum

An enemy's health never exceeds its maximum.

### BR-042 — Defeated enemies are removed

A defeated enemy is removed from the active Enemies of its ArenaRun.

### BR-043 — Enemies pursue the player

Every living enemy moves toward the player while the ArenaRun is
Playing.

### BR-044 — Enemies attack in range

An enemy attacks instead of closing further once the player is within
its attack range.

### BR-045 — Enemies keep fighting until the run ends

An enemy continues pursuing and attacking until it is defeated or the
run ends.

### BR-046 — Out-of-range enemies cause no damage

An enemy outside its attack range causes no damage to the player.

### BR-047 — Enemy attacks damage the player

A landed enemy attack reduces the player's health by that attack's
damage.

### BR-048 — Zero health defeats an enemy

An Enemy whose health reaches zero is Defeated.

## Wave

### BR-050 — One current wave

Exactly one Wave is current while an ArenaRun is active.

### BR-051 — Wave numbering starts at one

Wave numbers begin at one.

### BR-052 — A wave waits for its predecessor

A new Wave cannot begin before the current Wave is complete.

### BR-053 — Every wave has one boss

Every Wave contains exactly one Boss.

### BR-054 — A wave runs regular combat then boss combat

Every Wave begins in Regular Combat and moves to Boss Combat when its
Regular Enemies are done.

### BR-055 — A boss waits for the regular enemies

A Boss cannot become active while Regular Enemies remain to be spawned
or alive.

### BR-056 — At most one boss at a time

At most one Boss participates in a Wave.

### BR-057 — A wave needs all its enemies defeated

A Wave cannot complete before all its required Regular Enemies and its
Boss are defeated.

### BR-058 — Defeating the boss completes the wave

Defeating the Boss completes the current Wave.

### BR-059 — Completing a wave starts the next

Completing a non-final Wave starts the next Wave.

### BR-060 — A run has three waves

A run comprises three Waves; the third is the final Wave.

### BR-061 — The final wave completes the run

Completing the final Wave results in Victory.

### BR-062 — An empty regular phase is skipped

A Wave with zero Regular Enemies begins in Boss Combat.

### BR-063 — A boss is tougher than a regular enemy

A Boss has more health than any Regular Enemy in the run.

### BR-064 — A new wave advances the wave number by one

When a new Wave begins, its Wave Number is one greater than the Wave Number of
the Wave it follows.

## Enemy Spawning

### BR-070 — Regular spawns belong to regular combat

A Regular Enemy spawn is due only during Regular Combat while Regular
Enemies remain to spawn.

### BR-071 — The boss spawn belongs to boss combat

A Boss spawn is due only during Boss Combat while the Boss has not
spawned.

### BR-072 — Enemies enter from outside the arena

A spawned Enemy enters outside the Arena Bounds, within the Spawn Band,
at arena ground height, and moves inward.

### BR-073 — Spawning is never observed

An Enemy is never observed materializing inside the player's view; it
becomes visible only after entering from outside.

### BR-074 — Entered enemies count

Every Enemy that has entered is alive and counts toward the active
Wave.

### BR-075 — Spawning stops when the run ends

Enemies enter only while a Wave is active; none enter after the run has
ended.

### BR-076 — Later waves wait their turn

No Enemy of a later Wave enters while the current Wave is in progress.

## Game Time

### BR-080 — Game Time never runs backwards

Game Time never decreases within one ArenaRun.

### BR-081 — Readiness uses current time

Combat readiness is compared only against the current Game Time.

## Combat

### BR-090 — Damage is positive

Damage is a positive number.

### BR-091 — Applied damage equals health lost

Applying damage never reduces health below zero, and applied damage
equals the actual health reduction.

### BR-092 — Starting an attack deals no damage

Starting an attack applies no damage; only its impact does.

### BR-093 — One pending attack per combatant

A combatant has at most one pending attack.

### BR-094 — Cooldown starts with the attack

Cooldown starts when an attack starts.

### BR-095 — Impact uses current state

An attack's impact resolves against the state at impact time, not the
state at attack start.

### BR-096 — No weapon switch during a pending attack

Switching the player's weapon is rejected while the player has a
pending attack.

### BR-097 — Ranged impact hits at most one enemy

A ranged attack impact affects at most one enemy.

### BR-098 — Melee impact hits an enemy once

A melee attack damages a specific enemy at most once per impact.

### BR-099 — Enemy attacks need range and readiness

An enemy attack starts only when the enemy is active, has no pending
attack, is ready, and the player is within attack range.

### BR-100 — Enemy impact checks range again

An enemy attack impact hits only if the player is within attack range
at impact time.

### BR-101 — Lethal damage defeats the run

Player health reaching zero transitions the ArenaRun to Defeat in the
same operation.

### BR-102 — Defeated combatants lose pending attacks

A pending attack of a defeated combatant is cancelled.

### BR-103 — One weapon equipped

Exactly one weapon is equipped at any moment.

### BR-104 — Switching alternates weapons

A weapon switch alternates between the firearm and the melee weapon.

### BR-105 — The equipped weapon owns the attack input

The attack input performs only the equipped weapon's attack.

### BR-106 — The firearm needs no ammunition

The firearm uses no ammunition; firing never requires or consumes a
resource.

### BR-107 — Melee needs no resource

The melee attack has no ammunition or resource cost.

### BR-108 — Only a living player attacks

The attack is available only while the player is alive.

### BR-109 — Weapon switching needs only a living player

A weapon switch is available whenever the player is alive; no cooldown or
combat-readiness gate applies.

### BR-110 — Only the switch input changes the equipped weapon

The equipped weapon changes only in response to the weapon-switch input;
starting a new run with the firearm equipped is the sole exception.

### BR-111 — The shot follows the view direction

A firearm shot travels along the player's current View Direction.

### BR-112 — The shot affects the nearest enemy in its path

A firearm shot that reaches enemies affects only the nearest enemy along
its direction within Attack Range.

### BR-113 — A missed shot changes no health

A firearm shot that reaches no enemy within Attack Range applies no
Damage.

### BR-114 — A player attack needs readiness

A player attack can start only when the player has no pending attack and
the equipped weapon's Cooldown has elapsed.

### BR-115 — The swing affects enemies in front within reach

A melee attack impact affects every enemy within Attack Range in front of
the player.

### BR-116 — The swing spares enemies outside reach or behind

A melee attack applies no Damage to an enemy outside Attack Range or
outside the melee weapon's Attack Arc.

### BR-119 — Each weapon keeps its own readiness

Each weapon's attack readiness is its own: a weapon switch neither
transfers, resets, nor shares the other weapon's pending attack or
Cooldown.
