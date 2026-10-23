# Interaction

Feature F-005 — Player health and death.

| control | behaviour | keyboard path | focus order |
|---------|-----------|---------------|-------------|
| Health indicator | displays the current value against the maximum and a fill proportional to it; updates the instant damage is applied; reads zero and stops changing when the player is defeated; never interactive | none — display only | not in the focus order |
| Movement input while the player is defeated | changes the player's position no more from the moment health reaches zero until a new run begins; the input itself is unchanged from F-001 | W, A, S, D — unchanged from F-001 | 1 |
| View control while the player is defeated | no rotation: the view holds the direction it had when the player was defeated (settled by F-001b) | mouse — unchanged from F-001 | 2 |
| Weapon-switch input while the player is defeated | performs no switch; the equipped weapon stays as it was; the pending-attack refusal is F-002's | Q — unchanged from F-002 | 3 |
| Attack input while the player is defeated | starts no attack; the pending-attack behaviour is F-002's | left mouse button — unchanged from F-002 | 4 |

## Notes

- F-005 adds no interactive control. The health indicator is a display; its states are in `states.md`.
- The defeated state begins in the same operation that reduces health to zero; there is no intermediate "dying" state.
- A refused input changes nothing and shows no error; the player learns of defeat from the health indicator and from the inputs no longer acting.
- Focus order continues the shared S-001 order from F-001 and F-002; the view has no tab stops and all four inputs act directly.
- The restart that clears the defeated state is F-010's behaviour; F-005 states only that a new run starts at full health.
