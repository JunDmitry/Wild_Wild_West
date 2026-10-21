# Arena Combat Behavior Baseline

## B-001. New Arena Run

Given:

- a new ArenaRun is created;

Then:

- ArenaRun status is Playing;
- Wave 1 is current;
- Wave 1 is in Regular Combat unless it has zero Regular Enemies;
- Player has full Health;
- Player has Ranged weapon selected;
- there are no active Enemies.

## B-002. Player Movement

Given:

- ArenaRun is Playing;
- Player provides planar movement input;

Then:

- movement uses the Arena XZ plane;
- Player remains on Arena ground height;
- Player cannot leave logical Arena Bounds;
- the external world resolves collisions;
- the Domain validates that accepted movement does not extend, redirect, or teleport Player.

## B-003. Weapon Switching

Given:

- Player selected Ranged weapon;

When:

- Player switches weapon;

Then:

- Player selects Melee weapon.

When:

- Player switches weapon again;

Then:

- Player selects Ranged weapon.

## B-004. Switch and Attack in One Gameplay Step

Given:

- Player selected Ranged weapon;
- both weapons are ready;

When:

- weapon switch and attack start are requested in one gameplay step;

Then:

- weapon switching is processed before Player Attack Start;
- Player Attack uses Melee weapon;
- Player Attack cooldown is applied to Melee weapon only;
- later Player Attack Impact uses melee overlap targeting.

## B-005. Ranged Attack

Given:

- Player selected Ranged weapon;
- Player weapon is ready;
- Player Attack reaches ImpactAt;
- external targeting resolves one active Enemy within range;

Then:

- the Enemy receives Damage;
- ranged cooldown started at Attack Start;
- PlayerAttackCompleted and EnemyDamaged are recorded;
- a lethal impact records EnemyDefeated.

## B-006. Melee Attack

Given:

- Player selected Melee weapon;
- Player weapon is ready;
- Player Attack reaches ImpactAt;
- external targeting resolves active EnemyId values;

Then:

- each valid Enemy receives Damage at most once;
- duplicate EnemyId values are collapsed;
- melee cooldown started at Attack Start;
- target selection uses Player position at impact time.

## B-007. Enemy Defeat

Given:

- an Enemy receives lethal Damage;

Then:

- the Enemy is removed from active Enemies;
- EnemyDefeated is recorded;
- pending Enemy Attack is cancelled before Enemy removal;
- defeating a Regular Enemy contributes to Boss Combat eligibility;
- defeating a Boss updates current Wave progression.

## B-008. Wave Progression

Given:

- all required Regular Enemies were spawned and defeated;

Then:

- Wave enters Boss Combat;
- Application may request Boss spawn.

Given:

- current Wave Boss is defeated;

Then:

- current Wave completes.

Given:

- a non-final Wave completes;

Then:

- the next Wave starts in the same aggregate operation.

## B-009. Victory

Given:

- final Wave Boss is defeated;

Then:

- ArenaRun enters Victory;
- combat progression stops;
- ArenaRunVictorious is recorded.

## B-010. Defeat

Given:

- Player Health reaches zero after Enemy Attack Impact;

Then:

- ArenaRun enters Defeat;
- combat progression stops;
- remaining pending attacks are cancelled;
- PlayerDefeated and ArenaRunDefeated are recorded.

## B-011. Restart

Given:

- ArenaRun is in Defeat;
- Application lifecycle delay elapsed;

Then:

- Application reloads ArenaScene;
- Application creates a new ArenaRun;
- the new run has new ArenaRunId and PlayerId;
- the new run starts at Wave 1;
- the defeated run is not reset.