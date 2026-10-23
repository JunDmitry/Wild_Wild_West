# Screens

Feature F-004 — Melee attack.

| id | screen | one job | flows that reach it |
|----|--------|---------|---------------------|
| S-001 | Arena gameplay view | show the player character in third person inside the arena and reflect movement, view, equipped-weapon, health, enemy entry, and the melee swing with its hit confirmation | FL-029, FL-030, FL-031, FL-032, FL-033, FL-034 |

## Notes

- S-001 is the only screen F-004 touches. The feature mints no new screen; the swing and the hit confirmation are in-world displays inside this view.
- The swing is transient: it appears in front of the player when the melee attack starts and ends when the attack ends. It is not a persistent region and does not replace any indicator.
- The hit confirmation is transient as well: a mark appears at each enemy whose health the swing reduced, at the moment of impact. A swing that damages nothing produces none.
- F-004 adds no standing aim or reach marker. The swing is the only melee-specific surface; the firearm's aiming marker (F-003) is absent while the melee weapon is equipped.
- The equipped-weapon indicator (F-002) and the health indicator (F-005) stay in their fixed corners and are unchanged by F-004. F-004 draws no control.
- After Victory, F-011's S-002 window covers the centre of the view; after Defeat, F-010's defeat message appears within the restart delay. F-004 adds nothing to either terminal surface.
