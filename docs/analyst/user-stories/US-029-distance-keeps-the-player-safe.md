# US-029 — Distance keeps the player safe

- **Feature:** F-007 — Enemy pursuit and close-range attack
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-046`, `BR-100`

## Story

As a player, I want to take damage only from enemies close enough to reach
me, so that keeping my distance is a real defence rather than a delay.

## Examples

**E1 — an enemy out of range causes no damage**
- Given an Enemy farther from the Player than its Attack Range,
- when Game Time advances and the Enemy pursues,
- then the Player's Health is unchanged by that Enemy.

**E2 — an attack that outlives its range does not hit**
- Given an Enemy Attack started while the Player was within Attack Range,
- when the Player moves beyond Attack Range before the attack impact,
- then the attack lands on nothing and the Player's Health is unchanged.

**E3 — retreating leaves health untouched**
- Given the Player retreating from an Enemy that was pursuing,
- when the Player stays beyond that Enemy's Attack Range,
- then no Enemy Attack from it reaches the Player.

## Notes

Out-of-range behaviour is the safety counterweight to pursuit; attack timing
and telegraphs are not fixed by this feature.
