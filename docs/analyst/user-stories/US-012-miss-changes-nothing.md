# US-012 — A shot that reaches no enemy changes nothing

- **Feature:** F-003 — Firearm attack
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-113`

## Story

As a player, I want a shot that reaches no enemy to change nothing, so that
I can trust that only aimed hits affect the fight.

## Examples

**E1 — a shot into empty space**
- Given no Active enemy lies along the shot's direction within Attack Range,
- when the shot resolves,
- then every combatant's Health is unchanged.

**E2 — every enemy is out of range**
- Given enemies exist only outside the firearm's Attack Range,
- when the shot resolves,
- then no Health changes.

**E3 — the enemy left before impact**
- Given the only enemy along the shot's direction was defeated before the
  shot resolved,
- when the shot resolves,
- then no Health changes.

## Notes

None.
