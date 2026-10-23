# Accessibility

Feature F-003 — Firearm attack.

| screen | what it needs | label | role | keyboard path | assistive-technology behaviour |
|--------|---------------|-------|------|---------------|--------------------------------|
| S-001 — Arena gameplay view | a labelled aim-direction indication and a one-shot hit confirmation alongside the existing equipped-weapon and health status regions; both distinguishable without relying on colour alone; a miss is silent; the marker's direction is not announced per frame | "Arena gameplay view" (unchanged from F-001); the marker is labelled "Aim direction" | application (game view); the aiming marker is a status region | W, A, S, D move, mouse rotates the view, Q switches the weapon, left mouse button fires — unchanged from F-001 and F-002; no tab traversal inside the view | the view is announced on entry with the equipped weapon and health; when the firearm becomes equipped the marker is announced once ("Aim direction shown"); the marker follows the view silently and is not announced per frame; a landed shot is announced once ("Hit") when the confirmation appears; a miss changes nothing and is not announced; the confirmation's shape and the enemy's reaction are the non-colour cues; the terminal state stops the marker and confirmations |

## Focus order

- S-001 — Arena gameplay view: focus enters the view; the aiming marker and the hit confirmation are not focusable and add no tab stop; Tab order remains F-001's and F-002's — there are none inside the view; movement keys, mouse, Q, and the left mouse button act directly. Focus stays in the view for the rest of the run.
