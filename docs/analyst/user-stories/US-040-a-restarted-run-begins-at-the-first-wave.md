# US-040 — A restarted run begins at the first wave

- **Feature:** F-010 — Defeat and scene restart
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-027`, `BR-051`, `BR-016`

## Story

As a player, I want a restarted run to begin at the first wave, so that every
attempt starts from the same opening challenge.

## Examples

**E1 — the restarted run's current Wave is the first Wave**
- Given the previous ArenaRun ended in Defeat during a later Wave,
- when the Restart begins a new ArenaRun,
- then the new ArenaRun's current Wave is the first Wave.

**E2 — the Wave Number is one**
- Given a new ArenaRun has begun through a Restart,
- when the player observes the Wave Number,
- then it is one.

**E3 — the failed attempt's progress does not carry over**
- Given the previous ArenaRun had completed two Waves before the player was
  Defeated,
- when the new ArenaRun begins,
- then its current Wave is still the first Wave and its Wave Number is one.

## Notes

None.
