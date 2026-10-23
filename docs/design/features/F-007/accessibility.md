# Accessibility

Feature F-007 — Enemy pursuit and close-range attack.

| screen | what it needs | label | role | keyboard path | assistive-technology behaviour |
|--------|---------------|-------|------|---------------|--------------------------------|
| S-001 — Arena gameplay view | a labelled attack-cue indication at the attacking enemy, announced once before the damage; the cue and the enemy's stop-and-attack posture distinguishable without relying on colour alone; the health change announced by the existing health region | "Arena gameplay view" (unchanged from F-001); the cue is labelled "Enemy attack" | application (game view); the cue is a status region attached to the enemy | W, A, S, D move, mouse rotates the view, Q switches the weapon, left mouse button attacks — unchanged from F-001 and F-002; no tab traversal inside the view | an enemy's approach is not announced per frame; when an enemy starts an attack the cue is announced once ("Enemy attack incoming"); if the attack lands, F-005 announces the health change; if it misses or is cancelled, it is announced once ("Enemy attack missed"); the cue is not re-announced while it stays visible; the terminal state stops pursuit and cues |

## Focus order

- S-001 — Arena gameplay view: focus enters the view; the cue is not focusable and adds no tab stop; Tab order remains F-001's and F-002's — there are none inside the view; movement keys, mouse, Q, and the left mouse button act directly. Focus stays in the view for the rest of the run.
