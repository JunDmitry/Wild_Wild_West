# US-026 — Enemies pursue the player

- **Feature:** F-007 — Enemy pursuit and close-range attack
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-043`, `BR-045`

## Story

As a player, I want every living enemy to close in on me, so that the arena
keeps me moving and the fight never stalls with enemies out of reach.

## Examples

**E1 — an enemy out of reach approaches**
- Given an Active Enemy farther from the Player than its Attack Range,
- when Game Time advances while the run is Playing,
- then the Enemy moves toward the Player.

**E2 — pursuit continues while the player retreats**
- Given an Enemy pursuing the Player,
- when the Player moves away from it,
- then the Enemy keeps moving toward the Player's current Position.

**E3 — a defeated enemy stops pursuing**
- Given an Enemy whose Health has reached zero,
- when Game Time advances,
- then that Enemy no longer moves toward the Player.

## Notes

Movement speed is a tuning value. Enemy variety, flanking, cover use, and
patrol behaviour are out of scope.
