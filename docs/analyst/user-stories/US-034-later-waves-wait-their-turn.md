# US-034 — Later waves wait their turn

- **Feature:** F-008 — Wave progression
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-076`, `BR-075`

## Story

As a player, I want enemies from later waves to stay out of the arena until
their own wave begins, so that the fight I am in is exactly the fight of the
wave I am on and the next wave is a real change of scene.

## Examples

**E1 — a later wave's enemies stay out**
- Given the current Wave is in progress and a later Wave has Enemies yet to
  enter,
- when the current Wave's Spawn Pacing continues,
- then no Enemy of the later Wave enters the Arena.

**E2 — the arena holds only the current wave's enemies**
- Given Enemies belonging to a later Wave exist in the run,
- when the current Wave is in progress,
- then every Enemy present in the Arena belongs to the current Wave.

**E3 — the end of the run stops all entry**
- Given the ArenaRun has reached Defeat or Victory,
- when Game Time advances,
- then no Enemy of any Wave enters the Arena.

## Notes

This is the guarantee behind "a new wave starts only after the previous one is
cleared": the wave's own enemies are the only ones a player can meet while it
is current.
