# US-021 — Enemies come in from beyond the arena's edge

- **Feature:** F-006 — Enemy spawning outside the arena, unseen
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-072`

## Story

As a player, I want enemies to enter the arena from beyond its edges, so
that the arena feels like a place under siege rather than a stage on which
enemies simply exist.

## Examples

**E1 — an enemy appears outside the bounds**
- Given a Spawn is due,
- when the Enemy appears,
- then its Position is outside Arena Bounds, within the Spawn Band.

**E2 — the enemy moves inward**
- Given an Enemy has appeared outside Arena Bounds,
- when the run advances,
- then the Enemy moves inward, toward the Arena.

**E3 — entry ends inside the arena**
- Given an Enemy has appeared outside Arena Bounds,
- when it has finished entering,
- then it is Active inside Arena Bounds and takes part in the fight.

## Notes

Spawn effects, telegraphs, and audio are out of scope. Where exactly within
the Spawn Band an Enemy appears is a tuning value.
