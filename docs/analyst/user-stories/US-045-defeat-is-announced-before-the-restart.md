# US-045 — Defeat is announced before the restart

- **Feature:** F-010 — Defeat and scene restart
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-015`, `BR-121`, `BR-123`

## Story

As a player, I want to be told the run ended in defeat before the scene
restarts, so that I understand what happened instead of being dropped into a
new run with no explanation.

## Examples

**E1 — the message appears before the new run**
- Given the player's Health reaches zero,
- when the ArenaRun enters Defeat,
- then a defeat message is shown,
- and it is shown before the new ArenaRun begins.

**E2 — the message asks nothing of the player**
- Given the defeat message is shown and the player gives no input,
- when the delay between defeat and Restart elapses,
- then the new ArenaRun begins anyway.

**E3 — the message fits inside the delay**
- Given the defeat message is shown,
- when it is displayed,
- then it is displayed within the delay between Defeat and the Restart, and
  it does not outlast that delay.

## Notes

The delay's duration is a tuning value. The message's placement in the
system is an architectural decision and is not settled here.
