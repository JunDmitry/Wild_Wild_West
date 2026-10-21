# US-004 — Arena bounds containment

- **Feature:** F-001 — Player movement and third-person camera
- **Actor:** player
- **Priority:** P0 (from `docs/product/backlog.md`)
- **Rules:** `BR-031`, `BR-033`

## Story

As a player, I want movement to stop at the arena bounds, so that the closed
arena stays the whole playable space and I cannot leave it.

## Examples

**E1 — walking into an edge stops at the edge**
- Given the run is Playing and the player stands next to an arena edge,
- when the player holds the direction that points out of the arena,
- then the player stops at the edge and does not cross it.

**E2 — sliding along an edge stays inside**
- Given the run is Playing and the player is held against an edge,
- when the player also holds a direction along that edge,
- then the player moves along the edge while remaining inside the arena.

**E3 — a diagonal into a corner stays inside**
- Given the run is Playing and the player stands in a corner,
- when the player holds a diagonal direction pointing out of both edges,
- then the player remains inside the arena.

## Notes

The arena is a closed space; traversal outside it is not required by F-001.
