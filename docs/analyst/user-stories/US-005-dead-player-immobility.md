# US-005 — Dead player immobility

- **Feature:** F-001 — Player movement and third-person camera
- **Actor:** player
- **Priority:** P0 (from `docs/product/backlog.md`)
- **Rules:** `BR-023`, `BR-034`

## Story

As a player, I want my dead character to ignore movement input, so that death is
unambiguous and the run cannot continue by accident.

## Examples

**E1 — movement input does nothing while dead**
- Given the player's health has reached zero and the run is in Defeat,
- when the player holds W, A, S, or D,
- then the player's position does not change.

**E2 — the same input works before death**
- Given the player is alive and the run is Playing,
- when the player holds W,
- then the player moves; this example contrasts with E1 and shows that only the
  death state suppresses movement.

**E3 — death does not move the player anywhere else**
- Given the player is alive at a position inside the arena,
- when the player's health reaches zero,
- then the player remains at that position and does not drift or respawn.

## Notes

F-001 fixes movement only. Whether the view can still rotate after death is not
stated by the spec; see `docs/product/requirements/F-010-defeat-and-restart.md`
for the restart that follows.
