# Accessibility

Feature F-011 — Victory after three waves.

| screen | what it needs | label | role | keyboard path | assistive-technology behaviour |
|--------|---------------|-------|------|---------------|--------------------------------|
| S-001 — Arena gameplay view | the post-victory change announced once by the window, not by the view; the four gameplay inputs silent once inert | "Arena gameplay view" (unchanged from F-001) | application (game view) | W, A, S, D move, mouse rotates the view, Q switches the weapon, left mouse button attacks — unchanged from F-001, F-002, and F-005; no tab traversal inside the view | the view is announced on entry as before; when the run is won, the change is announced once by S-002, not by the view; a refused input after Victory changes nothing and is not announced; the view holds its last state behind the window and is not re-announced |
| S-002 — Victory window | a labelled modal window with a title and a message, announced once when it appears; focus moves into it and stays | "Victory" (window title); the message follows the title | dialog (modal, non-dismissible) | none — the window has no control and no tab stop; Escape and every other key change nothing | the window is announced once when it appears ("Victory; the run has been won"); the title is read before the message; focus does not leave the window and no tab stop is added; the arena view behind is not announced again; a refused key is not announced |

## Focus order

- S-001 — Arena gameplay view: focus enters the view and stays for the run while Playing; the equipped-weapon and health indicators add no tab stop.
- S-002 — Victory window: when the window appears, focus moves from the view into the window; the window has no tab stops, and focus stays there for as long as the run is shown. No control inside the window receives focus, and focus never returns to the view.
- The window's title and message are one announcement in reading order; the title is never skipped or read after the message.
