# US-023 — A wave's enemies arrive during that wave

- **Feature:** F-006 — Enemy spawning outside the arena, unseen
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-070`, `BR-071`, `BR-076`

## Story

As a player, I want each wave's enemies to arrive during that wave, so
that the pressure builds as I hold the arena and the next wave is never
already waiting at the edge.

## Examples

**E1 — regular enemies arrive in regular combat**
- Given the current Wave is in Regular Combat and Regular Enemies remain
  to spawn,
- when the Wave's Spawn Pacing calls for an Enemy,
- then a Regular Enemy enters the Arena.

**E2 — the boss arrives only in boss combat**
- Given the current Wave is still in Regular Combat,
- when its Spawn Pacing is considered,
- then no Boss enters the Arena.

**E3 — a later wave's enemies wait**
- Given a later Wave exists,
- when the current Wave is still in progress,
- then no Enemy of the later Wave enters the Arena.

## Notes

How many enemies a wave has and how far apart its spawns are fall under the
Wave's Spawn Pacing and are tuning values. Wave composition and difficulty
scaling are out of scope.
