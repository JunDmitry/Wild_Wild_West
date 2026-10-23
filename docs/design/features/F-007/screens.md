# Screens

Feature F-007 — Enemy pursuit and close-range attack.

| id | screen | one job | flows that reach it |
|----|--------|---------|---------------------|
| S-001 | Arena gameplay view | show the player character in third person inside the arena and reflect movement, view, equipped weapon, health, enemy entry, the firearm's aim, a landed shot, enemies closing on the player, and the cue that precedes an enemy attack | FL-035, FL-036, FL-037, FL-038, FL-039 |

## Notes

- S-001 is the screen F-001 designed, F-002 extended with the equipped-weapon indicator, F-005 with the health indicator, F-006 with enemy entry, and F-003 with the aiming marker and hit confirmation. F-007 extends it with pursuit and the Attack Cue; it mints no new screen id.
- Pursuit is in-world enemy movement, observed through the view. The Attack Cue is an in-world signal at the attacking enemy. Neither is an overlay or a HUD region, and no flow can reach either on its own.
- F-007 adds no counter, no enemy health display, and no warning banner. The cue is attached to the Enemy that is attacking, not to the screen edge.
- The equipped-weapon, health, aim, and entry behaviour is unchanged. The screen's single job grows by one clause: it now also shows enemies closing and the cue before an attack lands.
- F-005's health indicator is the surface that shows damage; F-007 does not duplicate it.
