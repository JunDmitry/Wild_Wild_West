# Accessibility

Feature F-006 — Enemy spawning outside the arena, unseen.

| screen | what it needs | label | role | keyboard path | assistive-technology behaviour |
|--------|---------------|-------|------|---------------|--------------------------------|
| S-001 — Arena gameplay view | entering enemies distinguishable without relying on colour alone; entry announced once an enemy is Active inside the arena; the pre-entry spawn never announced | "Arena gameplay view" (unchanged from F-001) | application (game view) | W, A, S, D move, mouse rotates the view, Q switches the weapon, left mouse button attacks — unchanged from F-001, F-002, and F-005; no tab traversal inside the view | an enemy is announced once when it crosses Arena Bounds and becomes Active ("Enemy entered the arena"), never while it is Spawned outside the view; movement into the arena and the enemy's silhouette are the non-colour cues; an enemy crossing the boundary is announced once, not per frame; a defeated enemy's removal is F-007's and F-009's announcement; the terminal state stops entry announcements |

## Focus order

- S-001 — Arena gameplay view: focus enters the view; the entering enemy is not focusable and adds no tab stop; Tab order remains F-001's, F-002's, and F-005's — there are none inside the view; movement keys, mouse, Q, and the left mouse button act directly. Focus stays in the view for the rest of the run.
- A Spawned enemy outside the arena is never in the focus order and never receives focus when it enters.
