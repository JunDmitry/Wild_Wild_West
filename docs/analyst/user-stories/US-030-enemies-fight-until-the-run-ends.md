# US-030 — Enemies fight until the run ends

- **Feature:** F-007 — Enemy pursuit and close-range attack
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-045`, `BR-014`, `BR-042`

## Story

As a player, I want enemies to keep pursuing and attacking until they are
defeated or the run is over, so that the fight stays continuous and the run
ends cleanly.

## Examples

**E1 — a wounded enemy keeps coming**
- Given an Enemy has taken Damage but its Health is above zero,
- when the Player moves away,
- then the Enemy resumes pursuit and attacks again once in range.

**E2 — the run ending stops every enemy**
- Given the ArenaRun has transitioned to Defeat,
- when Game Time advances,
- then no Enemy pursues and no Enemy Attack lands.

**E3 — a defeated enemy is out of the fight**
- Given an Enemy's Health has reached zero,
- when Game Time advances,
- then that Enemy neither pursues nor attacks and is removed from the active
  Enemies.

## Notes

Combat stops when the run is terminal; enemy variety and special attacks are
out of scope.
