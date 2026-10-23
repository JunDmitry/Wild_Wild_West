# Accessibility

Feature F-001 — Player movement and third-person camera.

| screen | what it needs | label | role | keyboard path | assistive-technology behaviour |
|--------|---------------|-------|------|---------------|--------------------------------|
| S-001 — Arena gameplay view | a single focusable in-world view with direct keyboard movement and mouse view control; the player character and Arena Bounds distinguishable without relying on colour alone; the terminal freeze announced once, not per refused input | "Arena gameplay view" | application (game view) | W, A, S, D move; mouse rotates the view; no tab traversal inside the view | the view is announced as one labelled region on entry; no per-frame movement is announced; when the run reaches Defeat or Victory the terminal state is announced once by the defeat or victory surface that follows; a refused mouse movement after the run ends changes nothing and is not announced; the view holds its last state and is not re-announced |

## Focus order

- S-001 — Arena gameplay view: focus enters the view; there are no tab stops inside it; movement keys and mouse act directly. Focus leaves only when the run ends and the defeat/restart or victory surface takes over.
- Once the run is terminal, focus does not return to the view for view rotation: the view holds its last direction, and the surface that follows owns the focus.
