# US-028 — Enemy attacks damage the player

- **Feature:** F-007 — Enemy pursuit and close-range attack
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-047`, `BR-091`, `BR-101`

## Story

As a player, I want a landed enemy attack to cost me health, so that the
damage I take is legible and my defeat is a real possibility.

## Examples

**E1 — a landed attack reduces health by its damage**
- Given the Player is within an Enemy's Attack Range at attack impact,
- when that Enemy Attack lands,
- then the Player's Health falls by exactly that attack's Damage.

**E2 — an attack that lands again costs health again**
- Given the Player has already taken one enemy attack,
- when another Enemy Attack lands,
- then the Player's Health falls by that attack's Damage in addition to the
  first.

**E3 — a lethal landed attack ends the run**
- Given the Player's Health equals the Damage of a landing Enemy Attack,
- when that attack lands,
- then the Player's Health reaches zero and the ArenaRun transitions to
  Defeat in the same operation.

## Notes

Enemy damage is a tuning value.
