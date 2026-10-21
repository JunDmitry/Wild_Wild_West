# US-008 — A run starts with the firearm equipped

- **Feature:** F-002 — Weapon switching
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-025`, `BR-027`

## Story

As a player, I want every run to start with the firearm equipped, so that
each attempt begins from the same known weapon and the attack input is
defined from the first moment.

## Examples

**E1 — a new run**
- Given a new ArenaRun is created,
- when the run begins,
- then the firearm is the equipped weapon.

**E2 — after a restart**
- Given the previous player was defeated and the run restarts,
- when the new run begins,
- then the firearm is the equipped weapon.

**E3 — the previous attempt does not carry over**
- Given the melee weapon was equipped when the previous run ended,
- when a new run begins,
- then the firearm is the equipped weapon, not the melee weapon.

## Notes

None.
