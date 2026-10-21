# US-001 — Camera-relative movement

- **Feature:** F-001 — Player movement and third-person camera
- **Actor:** player
- **Priority:** P0 (from `docs/product/backlog.md`)
- **Rules:** `BR-030`, `BR-031`, `BR-032`

## Story

As a player, I want to move my character with W, A, S, and D relative to the
camera, so that I can position myself anywhere in the arena without fighting
the controls.

## Examples

**E1 — forward, back, and sideways follow the view**
- Given the run is Playing and the camera looks at the player from behind,
- when the player holds W, the player moves away from the camera;
- when the player holds S, the player moves toward the camera;
- when the player holds A or D, the player moves to the camera's left or right.

**E2 — the same key means a different direction after the view turns**
- Given the run is Playing and the player rotates the view by 90 degrees,
- when the player holds W,
- then the player moves in the new away-from-camera direction, not the old one.

**E3 — diagonal input is not faster than a single direction**
- Given the run is Playing,
- when the player holds W and D together,
- then the player moves diagonally with a magnitude no greater than a single
  direction's movement.

**E4 — the run is not active**
- Given the run has reached Defeat or Victory,
- when the player holds W,
- then the player does not move.

## Notes

None. Movement speed and acceleration are tuning values and are not fixed here.
