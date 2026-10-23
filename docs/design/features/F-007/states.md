# States

Feature F-007 — Enemy pursuit and close-range attack.

| surface | empty | loading | error | disabled | success | partial |
|---------|-------|---------|-------|----------|---------|---------|
| S-001 enemy pursuit | no enemy is Active — nothing pursues | — (pursuit is continuous movement; there is no loading state) | — (a blocked or shortened movement is not an error state) | run terminal — no enemy pursues or closes | an enemy outside Attack Range — it moves toward the player | several enemies are Active — some pursue while others are in range or attacking |
| S-001 enemy Attack Cue | no enemy attack is pending — no cue is shown | an attack is in its wind-up — the cue is visible and the damage has not landed yet | — (a missed or cancelled attack is not an error; the cue simply ends) | run terminal — any cue disappears and no new cue appears | the attack resolves — the cue disappears as the health change lands, or as the attack misses | — (each attacking enemy carries its own cue; a cue is never partially shown) |
| S-001 gameplay inputs | no input this step — nothing changes | an enemy attack is in its wind-up — the inputs still act; the player may leave range and make the attack miss | — (no error state; a refused or inert input changes nothing) | player defeated or run terminal — movement, view, switch, and attack change nothing (F-001, F-005) | run Playing — inputs act as F-001, F-002, F-003, and F-004 define | — (an enemy attack does not partly disable an input) |

## Notes

- The cue's `loading` value is its wind-up: visible, and before the damage. This is the state the ordering rule creates; it is not a placeholder.
- The cue ends in one of two ways — the attack lands (health changes, F-005 announces it) or it misses/is cancelled (no health change). Neither is an error.
- The `disabled` value for pursuit and the cue is the terminal run state, not an unavailable control.
- A defeated enemy's cue disappears with the enemy; no separate removal state is shown.
- The health indicator's own states remain F-005's; F-007 does not add a health state row.
