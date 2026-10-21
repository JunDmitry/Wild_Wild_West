# US-004 — Arena Bounds containment

- **Feature:** F-001 — Player movement and third-person camera
- **Actor:** player
- **Priority:** P0 (from `docs/product/backlog.md`)
- **Rules:** `BR-031`, `BR-033`

## Story

As a player, I want movement to stop at Arena Bounds, so that the closed
arena stays the whole playable space and I cannot leave it.

## Examples

**E1 — walking into Arena Bounds stops there**
- Given the run is Playing and the player stands next to Arena Bounds,
- when the player holds the direction that points out of the arena,
- then the player stops at Arena Bounds and does not cross it.

**E2 — sliding along Arena Bounds stays inside**
- Given the run is Playing and the player is held against Arena Bounds,
- when the player also holds a direction along Arena Bounds,
- then the player moves along Arena Bounds while remaining inside the arena.

**E3 — a diagonal into a corner stays inside**
- Given the run is Playing and the player stands in a corner,
- when the player holds a diagonal direction pointing out of Arena Bounds,
- then the player remains inside the arena.

## Notes

The arena is a closed space; traversal outside it is not required by F-001.
