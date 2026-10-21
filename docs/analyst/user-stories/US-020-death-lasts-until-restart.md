# US-020 — Death lasts until the run restarts

- **Feature:** F-005 — Player health and death
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-024`, `BR-027`

## Story

As a player, I want a defeated player to stay defeated until the run
restarts, so that death is final and cannot be undone mid-run.

## Examples

**E1 — death persists for the rest of the run**
- Given the player has been Defeated,
- when Game Time advances within the same ArenaRun,
- then the player remains Defeated.

**E2 — nothing in the run restores the player**
- Given the player is Defeated,
- when any input available in the run is given,
- then the player does not become Alive.

**E3 — a restart restores the player**
- Given the previous ArenaRun ended with the player Defeated,
- when a new ArenaRun begins,
- then the player is Alive at full Health.

## Notes

Respawning without a restart is out of scope (F-010). How the restart is
triggered is F-010's behaviour.
