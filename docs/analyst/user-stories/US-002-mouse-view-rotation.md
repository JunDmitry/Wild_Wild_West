# US-002 — Mouse view rotation

- **Feature:** F-001 — Player movement and third-person camera
- **Actor:** player
- **Priority:** P0 (from `docs/product/backlog.md`)
- **Rules:** `BR-032`, `BR-035`, `BR-036`

## Story

As a player, I want to rotate the view around my character with the mouse, so
that I can look toward threats and choose the direction my movement is
relative to.

## Examples

**E1 — the mouse turns the view around the player**
- Given the run is Playing,
- when the player moves the mouse horizontally,
- then the view direction rotates around the player's position.

**E2 — looking and moving at the same time**
- Given the run is Playing and the player is holding W,
- when the player moves the mouse,
- then the view rotates and the player keeps moving in the same gameplay step.

**E3 — movement follows the new view immediately**
- Given the run is Playing and the player has just rotated the view,
- when the player holds W,
- then the player moves away from the camera's current facing.

## Notes

The vertical component of mouse movement is presentation tuning and is not
fixed by this feature.
