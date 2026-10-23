# US-006 — Weapon switch while alive

- **Feature:** F-002 — Weapon switching
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-023`, `BR-096`, `BR-104`, `BR-109`, `BR-110`, `BR-119`

## Story

As a player, I want one weapon-switch input to exchange my equipped weapon
while I am alive, so that I can choose the right weapon for the fight
without losing the other one.

## Examples

**E1 — firearm to melee and back**
- Given the run is Playing, the player is alive, the firearm is equipped,
  and the player has no pending attack,
- when the player presses the weapon-switch input,
- then the melee weapon becomes the equipped weapon,
- when the player presses the weapon-switch input again,
- then the firearm becomes the equipped weapon again.

**E2 — exactly one weapon is equipped**
- Given either weapon is equipped,
- when the player presses the weapon-switch input,
- then the other weapon becomes the equipped weapon and the previous one
  is no longer equipped.

**E3 — a dead player cannot switch**
- Given the player is defeated and the firearm is equipped,
- when the player presses the weapon-switch input,
- then the firearm remains the equipped weapon.

**E4 — a pending attack blocks the switch**
- Given the player is alive, the firearm is equipped, and the player has a
  pending attack,
- when the player presses the weapon-switch input,
- then the firearm remains the equipped weapon.

**E5 — no other input changes the weapon**
- Given the melee weapon is equipped,
- when the player uses movement or attack inputs without the
  weapon-switch input,
- then the melee weapon remains the equipped weapon.

**E6 — a cooldown survives a switch away and back**
- Given the player has just attacked with the firearm and its cooldown
  has not elapsed,
- when the player switches to the melee weapon and back to the firearm
  before the cooldown elapses,
- then the firearm's attack is still unavailable for the remainder of
  its original cooldown,
- and the melee weapon's own readiness was unaffected by the switch.

## Notes

`BR-096` still rejects a switch while an attack is pending. This story's
E6 concerns readiness after that attack has impacted.
