# Interaction

Feature F-001 — Player movement and third-person camera.

| control | behaviour | keyboard path | focus order |
|---------|-----------|---------------|-------------|
| Movement input | moves the player horizontally relative to the current view direction; a diagonal input does not exceed a single direction's magnitude; at Arena Bounds the player stops and may slide along the boundary; ignored while the run is not Playing | W, A, S, D from the gameplay view | 1 |
| View control | rotates the view around the player's position; movement immediately follows the new view; the view and movement can change in the same gameplay step | mouse movement from the gameplay view | 2 |

## Notes

- Both controls are direct inputs on the gameplay view; there is no on-screen control to focus or activate first.
- Movement direction is recomputed from the current view on every step, so a view rotation changes what W, A, S, and D mean immediately.
- No input other than the movement keys changes the player's position; no input other than the mouse changes the view direction.
- While the run is Defeat or Victory, movement input produces no position change. Whether the view may still rotate then is the open question carried by FL-003.
