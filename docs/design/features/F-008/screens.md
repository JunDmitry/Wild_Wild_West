# Screens

Feature F-008 — Wave progression.

| id | screen | one job | flows that reach it |
|----|--------|---------|---------------------|
| S-001 | Arena gameplay view | show the player character in third person inside the arena and reflect movement, view, equipped weapon, health, enemy entry, the firearm's aim, a landed shot, enemies closing on the player, the cue that precedes an enemy attack, the wave in progress against the total, and each new wave beginning | FL-040, FL-041, FL-042, FL-043 |
| S-002 | Victory window | show the end of a completed run, unchanged from F-011 | FL-040, FL-042 |

## Notes

- S-001 is the screen F-001 designed, F-002 extended with the equipped-weapon indicator, F-005 with the health indicator, F-006 with enemy entry, F-003 with the aiming marker and hit confirmation, F-007 with pursuit and the Attack Cue. F-008 extends it with the wave indicator and the new-wave announcement; it mints no new screen id.
- S-002 is F-011's Victory window, reached here unchanged when the third wave is cleared. F-008 adds nothing to it.
- The wave indicator is a persistent HUD region: the current wave and the total, always three.
- The announcement is a transient region that appears when a wave begins and disappears on its own. It is not a pause, a menu, or a countdown; the run keeps going (out of scope: inter-wave menus and countdowns).
- The screen's single job grows by one clause: it now also shows which wave is in progress and announces each new one.
- F-008 adds no control. The indicator and the announcement are displays.
