# US-013 — Swing the equipped melee weapon

- **Feature:** F-004 — Melee attack
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-105`, `BR-107`, `BR-108`, `BR-114`

## Story

As a player, I want to swing the melee weapon when it is equipped and I am
alive, so that I can fight enemies that come to close range.

## Examples

**E1 — the melee weapon answers the attack input**
- Given the Melee Weapon is equipped and the Player is Alive with no pending
  Attack and its Cooldown elapsed,
- when the attack input is pressed,
- then a Melee Attack starts and the Player has a pending Attack.

**E2 — the firearm does not swing**
- Given the Firearm is equipped,
- when the attack input is pressed,
- then no Melee Attack starts.

**E3 — a defeated player cannot swing**
- Given the Player is Defeated,
- when the attack input is pressed,
- then no Melee Attack starts.

**E4 — repeated swings are limited only by cooldown**
- Given a Melee Attack started at Game Time T,
- when the attack input is pressed before the Melee Weapon's Cooldown has
  elapsed,
- then no new Melee Attack starts,
- and when the attack input is pressed after the Cooldown has elapsed, a new
  Melee Attack starts.

**E5 — the swing costs no resource**
- Given the Player is Alive with the Melee Weapon equipped,
- when the attack input is pressed,
- then the swing consumes no ammunition and no other resource.

## Notes

The Melee Weapon's Cooldown and Damage are tuning values, not product
commitments.
