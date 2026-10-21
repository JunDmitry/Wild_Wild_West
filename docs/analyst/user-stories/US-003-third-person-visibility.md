# US-003 — Third-person visibility

- **Feature:** F-001 — Player movement and third-person camera
- **Actor:** player
- **Priority:** P0 (from `docs/product/backlog.md`)
- **Rules:** `BR-037`, `BR-038`

## Story

As a player, I want the camera to stay in a third-person position that keeps my
character visible, so that I can always see myself and judge my position in the
arena.

## Examples

**E1 — the view is third-person**
- Given the run is Playing,
- when the player looks at the screen,
- then the camera is positioned away from the player and the player's character
  is visible in the view.

**E2 — visibility survives rotation**
- Given the run is Playing,
- when the player rotates the view in any direction,
- then the player's character remains visible throughout the rotation.

**E3 — no first-person view**
- Given the run is Playing,
- when the player moves the mouse to the limit of its travel,
- then the view never becomes a first-person view centered on the player's eyes.

## Notes

Camera distance, height, and smoothing are tuning values. Camera collision with
scenery and camera shake are out of scope for F-001.
