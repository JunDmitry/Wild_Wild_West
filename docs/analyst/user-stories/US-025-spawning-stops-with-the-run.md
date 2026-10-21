# US-025 — Spawning stops when the run ends

- **Feature:** F-006 — Enemy spawning outside the arena, unseen
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-075`, `BR-012`, `BR-016`

## Story

As a player, I want no enemy to enter once the run has ended, so that a won
or lost run is truly over and nothing new arrives to spoil the result.

## Examples

**E1 — nothing enters after the player is defeated**
- Given the player is Defeated and the run is therefore over,
- when Game Time advances,
- then no Enemy enters the Arena.

**E2 — nothing enters after the run is won**
- Given the final Wave is complete and the run is victorious,
- when Game Time advances,
- then no Enemy enters the Arena.

**E3 — a restart is a new run with its own spawning**
- Given a defeated run restarts,
- when the new ArenaRun begins,
- then spawning follows the new run's wave one, not the old run's remainder.

## Notes

Spawning effects, telegraphs, and audio are out of scope.
