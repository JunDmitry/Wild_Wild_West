# US-018 — I die at zero health and can no longer act

- **Feature:** F-005 — Player health and death
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-023`

## Story

As a player, I want to be defeated when my health reaches zero, so that the
run has a genuine failure state I have to avoid.

## Examples

**E1 — zero health defeats the player**
- Given the player's current Health reaches zero,
- when the Damage that brought it to zero is applied,
- then the player is Defeated in that same operation.

**E2 — a defeated player cannot move**
- Given the player is Defeated,
- when Movement Input is given,
- then the player's Position does not change.

**E3 — a defeated player cannot switch weapons**
- Given the player is Defeated,
- when the weapon-switch input is pressed,
- then the equipped Weapon does not change.

**E4 — a defeated player cannot attack**
- Given the player is Defeated,
- when the attack input is pressed,
- then no Attack starts.

## Notes

Death animation and audio are out of scope. What happens to the run after
the player is defeated is F-010.
