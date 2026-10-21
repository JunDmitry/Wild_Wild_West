# US-024 — Every entered enemy counts toward the wave

- **Feature:** F-006 — Enemy spawning outside the arena, unseen
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-074`, `BR-057`, `BR-042`

## Story

As a player, I want every enemy that has entered to be alive and counted in
the wave, so that I can trust the wave is over only when nothing is left
standing.

## Examples

**E1 — an entered enemy is alive and counted**
- Given an Enemy has entered the Arena,
- when the remaining Enemies of the current Wave are considered,
- then that Enemy counts toward the active Wave.

**E2 — a living entered enemy holds the wave open**
- Given all but one of the entered Enemies are Defeated,
- when that last entered Enemy is still alive,
- then the Wave does not complete.

**E3 — a defeated enemy stops counting**
- Given an entered Enemy is Defeated,
- when the remaining Enemies of the current Wave are considered,
- then it no longer counts toward the active Wave.

## Notes

The Wave completes only when all its required Regular Enemies and its Boss
are defeated. Enemy counts per wave are a tuning value.
