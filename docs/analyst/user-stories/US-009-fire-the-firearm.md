# US-009 — Fire the firearm

- **Feature:** F-003 — Firearm attack
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-093`, `BR-094`, `BR-103`, `BR-105`, `BR-106`, `BR-108`, `BR-114`

## Story

As a player, I want to fire the firearm when it is equipped and I am alive,
so that I can damage enemies at a distance.

## Examples

**E1 — firing with the firearm equipped**
- Given the run is Playing, the player is alive, the firearm is the equipped
  weapon, the firearm is ready, and the player has no pending attack,
- when the player uses the attack input,
- then the firearm's attack starts and becomes the player's pending attack.

**E2 — the melee weapon does not fire the firearm**
- Given the melee weapon is the equipped weapon,
- when the player uses the attack input,
- then no firearm shot occurs.

**E3 — a dead player cannot fire**
- Given the player is defeated,
- when the player uses the attack input,
- then no firearm shot occurs.

**E4 — repeated fire is limited by the cooldown**
- Given the firearm is equipped and ready and the player is alive,
- when the player uses the attack input and the shot resolves,
- then the player cannot start another shot while the firearm's cooldown
  has not elapsed,
- and can start another shot once the cooldown has elapsed.

**E5 — no ammunition and no reload**
- Given the firearm is equipped and the player is alive,
- when the player fires repeatedly over time,
- then every shot starts as soon as the firearm is ready,
- and no ammunition is consumed and no reload action is required.

## Notes

The firearm's damage and cooldown values are tuning values, not
product commitments.
