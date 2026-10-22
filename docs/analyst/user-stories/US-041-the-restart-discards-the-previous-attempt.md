# US-041 — The restart discards the previous attempt

- **Feature:** F-010 — Defeat and scene restart
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-028`, `BR-016`, `BR-042`, `BR-050`

## Story

As a player, I want the restarted run to contain nothing from the failed
attempt, so that a fresh attempt is genuinely fresh rather than a continuation
of the run I lost.

## Examples

**E1 — no enemy survives the restart**
- Given the defeated ArenaRun had active Enemies when the player was Defeated,
- when a new ArenaRun begins through a Restart,
- then none of those Enemies is active in the new ArenaRun.

**E2 — the boss does not return**
- Given the defeated ArenaRun ended with its Boss active,
- when a new ArenaRun begins through a Restart,
- then that Boss is not active in the new ArenaRun.

**E3 — a later wave's enemies do not leak into the new run**
- Given the defeated ArenaRun had entered the final Wave,
- when the new ArenaRun begins through a Restart,
- then the new ArenaRun's current Wave is its own first Wave, and no Enemy of
  the final Wave is active in it.

## Notes

None.
