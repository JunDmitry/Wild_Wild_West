# Interaction

Feature F-004 — Melee attack.

| control | behaviour | keyboard path | focus order |
|---------|-----------|---------------|-------------|
| Melee attack input | performs the swing while the melee weapon is equipped and the player is alive; the swing becomes visible in front of the player; starts no swing while a pending attack exists or the melee Cooldown has not elapsed; with the firearm equipped the same input performs the firearm attack (F-003) and no swing | left mouse button from the gameplay view | 4 |
| Melee swing | displays an in-world arc in front of the player from Attack Start through impact, then ends; it follows the current View Direction through the attack, redrawn as the view turns, so the arc covers the area the impact will check; it is transient and never interactive | none — display only | not in the focus order |
| Hit confirmation | displays a transient mark at each enemy whose health the swing reduced, at the moment of impact; absent when the swing damages nothing; ends with the swing; never interactive | none — display only | not in the focus order |

## Notes

- Movement input (focus order 1), view control (2), and weapon-switch input (3) are specified in F-001 and F-002 and unchanged.
- F-002 routes the attack input to the equipped weapon; F-004 defines what the melee weapon does once routed, as F-003 defines the firearm case.
- Attack Start applies no Damage; Impact resolves against the state at impact time (BR-092, BR-095). The swing is visible from Attack Start, and the hit confirmation appears only at impact. The swing follows the live View Direction until impact — the same direction the impact resolves along — so a player who turns during the windup sees the arc where damage will be checked.
- A refused attack changes nothing and shows no error; the player learns of readiness from the attack input acting again once the Cooldown elapses.
- The swing and the hit confirmation are displays, not controls; their states are in `states.md`. F-004 adds no interactive control.
- Focus order continues the shared S-001 order from F-001 and F-002; the view has no tab stops and all inputs act directly.
