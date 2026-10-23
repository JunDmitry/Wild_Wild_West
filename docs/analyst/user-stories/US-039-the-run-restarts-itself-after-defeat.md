# US-039 — The run restarts itself after defeat

- **Feature:** F-010 — Defeat and scene restart
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-017`, `BR-015`, `BR-016`, `BR-101`

## Story

As a player, I want the run to restart by itself when I am defeated, so that I
can try again immediately instead of being left in a dead run.

## Examples

**E1 — zero health defeats the run**
- Given the player's Health is above zero and the ArenaRun is Playing,
- when Damage reduces the player's Health to zero,
- then the ArenaRun is in Defeat in that same operation.

**E2 — the restart needs no player input**
- Given the ArenaRun is in Defeat,
- when no player input is given,
- then a new ArenaRun begins and is Playing.

**E3 — nothing blocks or waits for the player**
- Given the ArenaRun has entered Defeat,
- when the Restart happens,
- then no confirmation, menu, or input requirement stands between the
  defeat and the new ArenaRun,
- and the defeat message shown inside the delay requires nothing from the
  player.

## Notes

The time between Defeat and the Restart is a tuning value. A defeat message
is shown inside that delay and is told in `US-045` and `US-046`; it never
waits for the player.
