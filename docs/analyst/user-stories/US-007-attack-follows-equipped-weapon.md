# US-007 — Attack follows the equipped weapon

- **Feature:** F-002 — Weapon switching
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-103`, `BR-105`

## Story

As a player, I want the attack input to perform only my equipped weapon's
attack, so that the weapon I chose determines how I fight.

## Examples

**E1 — melee equipped**
- Given the melee weapon is equipped and the player is alive,
- when the player uses the attack input,
- then a melee attack is performed,
- and no firearm attack is performed.

**E2 — firearm equipped**
- Given the firearm is equipped and the player is alive,
- when the player uses the attack input,
- then a firearm attack is performed,
- and no melee attack is performed.

**E3 — the mapping follows a switch**
- Given the firearm is equipped,
- when the player switches to the melee weapon and then uses the attack
  input,
- then a melee attack is performed.

## Notes

None. The firearm and melee attacks themselves are F-003 and F-004; this
story constrains only which one the attack input performs.
