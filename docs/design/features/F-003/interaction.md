# Interaction

Feature F-003 — Firearm attack.

| control | behaviour | keyboard path | focus order |
|---------|-----------|---------------|-------------|
| Firearm attack input | performs a firearm shot only while the firearm is equipped, the player is alive, no attack is pending, and the firearm's Cooldown has elapsed; the shot starts along the current View Direction and resolves at impact; with the melee weapon equipped, while the player is defeated, or once the run is terminal it starts no shot | left mouse button from the gameplay view | 4 |
| Aiming marker | displays the direction a shot would travel along the current View Direction while the firearm is equipped and the player is alive; absent while the melee weapon is equipped or the run is terminal; follows the view as it turns; never interactive | none — display only | not in the focus order |
| Hit confirmation | appears at the enemy a shot damaged, at the moment of impact; absent when a shot reaches no enemy; short-lived; never interactive | none — display only | not in the focus order |

## Notes

- Movement input (focus order 1), view control (2), and weapon-switch input (3) are specified in F-001 and F-002 and are unchanged.
- The attack input row is the firearm case of F-002's attack-input routing; F-004 specifies the melee case. F-002 alone decides which weapon's attack the input reaches.
- The aiming marker is present while the firearm is equipped and the player is alive. A momentary Cooldown does not hide it; the marker shows the aim direction, not whether a shot is ready this instant.
- The marker is a display, not a targeting control: it does not select, lock, or highlight an enemy. The shot still resolves against the nearest enemy along the direction within Attack Range at impact (BR-112).
- The hit confirmation is tied to Damage actually applied. A shot that reaches no enemy produces none (R9, A9).
- Both the marker and the confirmation are silent on a refused input; the player learns of readiness from the shot starting or not starting.
- Focus order continues the shared S-001 order from F-001 and F-002; the view has no tab stops and all four inputs act directly.
