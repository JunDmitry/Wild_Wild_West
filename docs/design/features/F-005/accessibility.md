# Accessibility

Feature F-005 — Player health and death.

| screen | what it needs | label | role | keyboard path | assistive-technology behaviour |
|--------|---------------|-------|------|---------------|--------------------------------|
| S-001 — Arena gameplay view | a labelled health status region alongside the existing equipped-weapon status; the health value distinguishable without relying on colour alone; the defeated state announced once | "Health" (health region); the view keeps its F-001/F-002 label "Arena gameplay view" | application (game view); the health indicator is a status region | none for the health indicator; W, A, S, D move, mouse rotates the view, Q switches the weapon, left mouse button attacks — unchanged from F-001 and F-002; no tab traversal inside the view | the view is announced on entry with the current and maximum health; health is announced again whenever it changes ("Health: 60 / 100"); reaching zero is announced with the defeated state ("Health: 0 / 100; the player can no longer act"); the defeated state is announced once, not on each refused input; a refused input changes nothing and is not announced; the numeric readout is the non-colour cue |

## Focus order

- S-001 — Arena gameplay view: focus enters the view; the health indicator is not focusable and adds no tab stop; Tab order remains F-001's and F-002's — there are none inside the view; movement keys, mouse, Q, and the left mouse button act directly. Focus stays in the view for the rest of the run.
