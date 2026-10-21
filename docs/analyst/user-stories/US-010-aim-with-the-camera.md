# US-010 — Aim the shot with the camera

- **Feature:** F-003 — Firearm attack
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-111`

## Story

As a player, I want my shot to travel where the camera is facing, so that I
can aim at enemies I can see.

## Examples

**E1 — the shot follows the current view**
- Given the firearm is equipped and the current View Direction points north,
- when the player fires,
- then the shot travels north.

**E2 — the shot follows a turned view**
- Given the player turns the view so that the View Direction points east,
- when the player fires,
- then the shot travels east.

**E3 — aim is horizontal**
- Given the camera's vertical pitch differs from level,
- when the player fires,
- then the shot's direction is the horizontal View Direction.

## Notes

The vertical component of view movement is presentation tuning (`US-002`);
the analysed aim direction is the horizontal View Direction.
