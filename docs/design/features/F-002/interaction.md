# Interaction

Feature F-002 — Weapon switching.

| control | behaviour | keyboard path | focus order |
|---------|-----------|---------------|-------------|
| Weapon-switch input | exchanges the equipped weapon for the other one; exactly one weapon remains equipped; the indicator updates; refused while the player is defeated or has a pending attack; no other input changes the equipped weapon | Q from the gameplay view | 3 |
| Attack input | performs only the equipped weapon's attack: a firearm attack while the firearm is equipped, a melee attack while the melee weapon is equipped; starts no attack while the player is defeated | left mouse button from the gameplay view | 4 |

## Notes

- Movement input (focus order 1) and view control (focus order 2) are specified in F-001 and unchanged.
- The equipped-weapon indicator is a display, not a control; its states are in `states.md`.
- A refused switch leaves the equipped weapon unchanged; the player can press Q again once the pending attack resolves, or after the run restarts.
- The attack input's timing, cooldown, and impact are F-003 and F-004; F-002 only determines which weapon's attack the input reaches.
- Focus order continues the shared S-001 order from F-001; the view has no tab stops and all four inputs act directly.
