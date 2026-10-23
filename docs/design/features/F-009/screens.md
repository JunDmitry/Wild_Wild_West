# Screens

Feature F-009 — Boss enemy each wave.

| id | screen | one job | flows that reach it |
|----|--------|---------|---------------------|
| S-001 | Arena gameplay view | show the player character in third person inside the arena and reflect movement, view, equipped weapon, health, enemy entry, the firearm's aim, a landed shot, enemies closing on the player, the cue that precedes an enemy attack, the wave in progress, each new wave beginning, and the boss's distinct entry and remaining health | FL-044, FL-045, FL-046, FL-047 |
| S-002 | Victory window | show the end of a completed run, unchanged from F-011 | FL-044, FL-046 |

## Notes

- S-001 is the screen F-001 designed, F-002 extended with the equipped-weapon indicator, F-005 with the health indicator, F-006 with enemy entry, F-003 with the aiming marker and hit confirmation, F-007 with pursuit and the Attack Cue, F-008 with the wave indicator and announcement. F-009 extends it with the boss entry marker and the boss health indicator; it mints no new screen id.
- S-002 is F-011's Victory window, reached here unchanged when a wave's boss is defeated on the third wave. F-009 adds nothing to it.
- The boss entry is marked distinctly from a regular enemy's entry (R6): the entry itself is F-006's, and F-009 makes the boss's entry carry a boss marker and its health indicator appear with it.
- The boss health indicator is a HUD region shown only while the boss is alive. It is labelled as the boss's, so it is not confused with the player's health indicator (F-005), which keeps its place and its states.
- The screen's single job grows by one clause: it now also marks the boss and shows its remaining health.
- F-009 adds no control. The marker and the indicator are displays.
